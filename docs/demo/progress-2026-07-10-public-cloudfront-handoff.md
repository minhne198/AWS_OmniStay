# OmniStay public demo handoff - 2026-07-10

File nay la ban ban giao tien do moi nhat cho nguoi tiep tuc lam du an. Khong ghi password, JWT secret, access key, token, database password vao file nay.

## Tom tat ngan gon

Du an da deploy duoc backend len AWS account cu va frontend len AWS account moi. CloudFront public domain da truy cap duoc backend API, health check, search API va frontend UI. Admin login qua CloudFront da thanh cong sau khi sua loi JWT secret qua SSM tren EC2 dang chay.

Trang public hien tai:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
https://d1gm8xt8e0mar4.cloudfront.net/public/home.html
```

CloudFront domain:

```text
d1gm8xt8e0mar4.cloudfront.net
```

Trang dang dung de demo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

## Kien truc thuc te hien tai

Du an dang dung 2 AWS account khac nhau:

### Account cu - backend/runtime

```text
AWS account id: 306656769227
Region: ap-southeast-1
Purpose: backend, ALB, ASG/EC2, RDS MySQL, Redis/Valkey, artifact bucket
```

Tai nguyen quan trong:

```text
ALB name: omnistay-alb
ALB DNS: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
ALB scheme: Internet-facing
ALB listener: HTTP:80 -> target group omnistay-api-tg
Target group: omnistay-api-tg
Target group health path: /health
Backend EC2 app port: 8080
ASG: omnistay-api-asg
Launch template: omnistay-api-lt-v2
Launch template id: lt-077ae45544a46ef24
RDS endpoint: omnistay-mysql.c5wo68sksevz.ap-southeast-1.rds.amazonaws.com
RDS database: hotel_booking
Redis endpoint: master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com:6379
Artifact bucket: omnistay-artifacts-306656769227-new
Backend artifact: s3://omnistay-artifacts-306656769227-new/omnistay-api.zip
```

ALB security group da xac nhan co inbound:

```text
Type: HTTP
Protocol: TCP
Port: 80
Source: 0.0.0.0/0
```

Backend health qua ALB da chay:

```text
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health/aws
```

### Account moi - frontend/CloudFront

```text
AWS account id: 484504929670
Region for S3: ap-southeast-1
CloudFront scope: Global
Purpose: frontend S3 bucket va CloudFront distribution
```

Tai nguyen quan trong:

```text
Frontend bucket: omnistay-frontend-484504929670
CloudFront distribution id: E25IVE7NGAHBJP
CloudFront domain: d1gm8xt8e0mar4.cloudfront.net
S3 origin domain: omnistay-frontend-484504929670.s3.ap-southeast-1.amazonaws.com
ALB origin domain: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
```

CloudFront behavior hien tai:

```text
/api/*     -> origin omnistay-api-alb, cache disabled
/health*  -> origin omnistay-api-alb, cache disabled
Default * -> S3 frontend origin, caching optimized
```

Default root object:

```text
public/index.html
```

S3 bucket policy phai cho CloudFront distribution doc object qua OAC. Policy dung dang:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowCloudFrontServicePrincipal",
      "Effect": "Allow",
      "Principal": {
        "Service": "cloudfront.amazonaws.com"
      },
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::omnistay-frontend-484504929670/*",
      "Condition": {
        "StringEquals": {
          "AWS:SourceArn": "arn:aws:cloudfront::484504929670:distribution/E25IVE7NGAHBJP"
        }
      }
    }
  ]
}
```

Khong can tat Block Public Access. Bucket nen giu private va cho CloudFront doc qua OAC.

## Cac moc da hoan thanh ngay 2026-07-10

### 1. Frontend da upload len S3 account moi

Frontend production da duoc upload len bucket:

```text
omnistay-frontend-484504929670
```

S3 object path da dung:

```text
public/index.html
public/home.html
public/admin.html
public/config.js
src/api.js
src/session.js
src/admin.js
assets/
```

Luu y: truoc khi upload da sua file production config:

```js
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

File local da sua:

```text
C:\tmp\omnistay-frontend\public\config.js
```

### 2. CloudFront da cau hinh xong

Distribution da ton tai san, khong tao moi:

```text
Distribution id: E25IVE7NGAHBJP
Domain: d1gm8xt8e0mar4.cloudfront.net
```

Da cau hinh:

- Default root object = `public/index.html`.
- Them origin ALB `omnistay-api-alb`.
- Sua origin ALB protocol ve `HTTP only`, port `80`.
- Them behavior `/api/*` tro ve ALB.
- Them behavior `/health*` tro ve ALB.
- Default behavior tro ve S3 frontend origin.
- Da tao invalidation `/*`.

### 3. Da sua cac loi CloudFront

Loi 504 Gateway Timeout khi vao `/health` qua CloudFront:

- Nguyen nhan: origin ALB dang de `HTTPS only`, trong khi ALB chi co listener `HTTP:80`.
- Da sua: CloudFront origin `omnistay-api-alb` doi sang `HTTP only`, port `80`.
- Ket qua: `/health/aws` qua CloudFront da chay.

Loi S3 `AccessDenied` khi vao `/public/index.html`:

- Nguyen nhan 1: S3 origin ban dau tro nham bucket `omnistay-frontend-887329217416...`.
- Nguyen nhan 2: OAC/bucket policy can khop voi bucket moi `omnistay-frontend-484504929670`.
- Da sua: S3 origin tro ve `omnistay-frontend-484504929670.s3.ap-southeast-1.amazonaws.com`.
- Da tao/chon OAC moi cho bucket moi.
- Da cap nhat bucket policy cho distribution `E25IVE7NGAHBJP`.
- Ket qua: `/public/index.html` qua CloudFront da mo duoc UI.

Loi HTTP 500 khi login admin:

- Nguyen nhan xac nhan tu journal log: `IDX10653`, key HS256 chi co 96 bits.
- Nghia la `Jwt__Secret` tren backend qua ngan.
- Da sua tam thoi tren EC2 dang chay bang SSM/systemd drop-in, dung JWT secret dai hon.
- Da restart service `omnistay-api`.
- Ket qua: admin login qua CloudFront da thanh cong va vao duoc trang home.

Khong ghi JWT secret that vao file nay.

### 4. Cac URL da test thanh cong

CloudFront health:

```text
https://d1gm8xt8e0mar4.cloudfront.net/health/aws
```

Ket qua health/aws co cac field quan trong:

```text
environment = Production
region = ap-southeast-1
databaseProvider = MySql
redisConfigured = true
redisConnected = true
searchCacheTtlSeconds = 120
apiBasePath = /api
```

CloudFront API search:

```text
https://d1gm8xt8e0mar4.cloudfront.net/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Ket qua: JSON list phong khach san Da Nang tra ve thanh cong.

CloudFront frontend UI:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

Ket qua: UI login OmniStay mo duoc.

Admin login:

```text
Email: admin@omnistay.local
Password: xem ghi chu secret rieng cua chu project, khong nam trong repo
```

Ket qua: login thanh cong, vao duoc:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/home.html
```

Header hien:

```text
OmniStay Admin (Admin)
```

## Dang lam do / can lam tiep ngay

Day la phan ban be can tiep tuc.

### Viec 1 - Test admin dashboard qua CloudFront

Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/home.html
```

Neu da login admin, bam menu:

```text
Admin
```

Can xac nhan:

- Admin dashboard load duoc.
- Users load duoc.
- Hotels load duoc.
- Bookings load duoc.
- Khong bi HTTP 401, 403, 500.

Can chup bang chung:

- URL CloudFront tren thanh dia chi.
- Header co `OmniStay Admin (Admin)`.
- Bang/list admin co data.

Neu Admin dashboard loi:

1. Mo DevTools -> Network.
2. Kiem endpoint nao loi:
   - `/api/admin/users`
   - `/api/admin/hotels`
   - `/api/admin/bookings`
3. Neu 401/403: kiem token/localStorage hoac login lai.
4. Neu 500: vao EC2 SSM chay:

```bash
sudo journalctl -u omnistay-api --since "10 minutes ago" --no-pager
```

### Viec 2 - Test customer flow public

Logout admin truoc, sau do test bang customer moi:

1. Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

2. Bam `Dang ky`.
3. Tao customer moi bang email moi.
4. Login customer.
5. Search:

```text
City: Da Nang
Check-in: chon ngay trong tuong lai
Check-out: sau check-in
Guests: 2
```

6. Chon hotel/room.
7. Tao booking.
8. Mock payment.
9. Vao `Booking cua toi`.
10. Kiem booking hien `Da thanh toan`, `Confirmed`, hoac trang thai tuong duong.

Can chup bang chung:

- Register/customer login.
- Search result.
- Hotel detail.
- Booking confirmation co booking code.
- Mock payment success.
- Booking cua toi co booking vua tao.

### Viec 3 - Chup evidence CloudFront/S3/API

Can chup cac man sau neu chua co:

- CloudFront distribution detail:
  - ID `E25IVE7NGAHBJP`.
  - Domain `d1gm8xt8e0mar4.cloudfront.net`.
  - Status enabled/deployed.
- CloudFront origins:
  - S3 origin `omnistay-frontend-484504929670.s3.ap-southeast-1.amazonaws.com`.
  - ALB origin `omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com`.
- CloudFront behaviors:
  - `/api/*` -> `omnistay-api-alb`.
  - `/health*` -> `omnistay-api-alb`.
  - `Default (*)` -> S3 frontend.
- CloudFront invalidation `/*` status `Completed`.
- S3 bucket object list:
  - `public/`
  - `src/`
  - `assets/`
- S3 bucket policy/OAC evidence.
- Browser:
  - `/health/aws`.
  - `/api/hotels/search?...`.
  - `/public/index.html`.

### Viec 4 - Chup evidence backend/AWS services

Account cu `306656769227`, region `ap-southeast-1`:

- ALB:
  - `omnistay-alb`
  - Scheme `Internet-facing`
  - DNS name
  - Listener `HTTP:80`
  - SG inbound `HTTP 80 0.0.0.0/0`
- Target group:
  - `omnistay-api-tg`
  - target `Healthy`
  - health path `/health`
- EC2/ASG:
  - instance running
  - ASG `omnistay-api-asg`
  - launch template attached
- RDS:
  - `omnistay-mysql`
  - endpoint
  - security group
  - connections/CPU metric
- Redis/Valkey:
  - `omnistay-cache`
  - endpoint
  - security group
  - connection/CPU metrics
- CloudWatch dashboard:
  - `OmniStay-Demo` neu da tao
  - ALB request count, EC2 CPU, RDS connections, Redis connections, ASG capacity

### Viec 5 - Lam evidence Redis cache HIT/MISS

Mo URL search qua CloudFront 2 lan:

```text
https://d1gm8xt8e0mar4.cloudfront.net/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Sau do vao EC2 SSM account cu, chay:

```bash
sudo journalctl -u omnistay-api -n 300 --no-pager
```

Tim log dang:

```text
Hotel search cache MISS
Hotel search cache HIT
```

Neu khong thay do log qua cu, goi search them 2 lan va xem lai log.

## Viec can sua cho ben vung truoc/sau demo

### 1. Sua Launch Template de khong mat fix JWT khi ASG tao instance moi

Hien tai loi JWT secret ngan da duoc sua tam thoi tren EC2 dang chay bang systemd drop-in. Day la fix tren instance hien tai. Neu ASG terminate instance va tao instance moi, instance moi co the quay lai user data/secret cu va login lai loi `HTTP 500`.

Can vao account cu:

```text
EC2 -> Launch Templates -> omnistay-api-lt-v2 -> Actions -> Modify template (Create new version)
```

Trong user data, sua:

```text
Environment=Jwt__Secret=<JWT secret dai hon 32 ky tu>
```

Yeu cau:

- Secret phai dai hon 32 ky tu.
- Khong ghi secret vao repo.
- Khong paste secret vao file Markdown.

Dong thoi nen update:

```text
Environment=AWS__S3FrontendBucket=omnistay-frontend-484504929670
Environment=AWS__CloudFrontDomain=d1gm8xt8e0mar4.cloudfront.net
```

Sau khi tao Launch Template version moi:

```text
EC2 -> Auto Scaling Groups -> omnistay-api-asg -> Edit -> Launch template version = Latest
```

Chi nen refresh/replace instance sau khi da chac user data moi dung. Neu dang can demo gap, co the giu instance hien tai va khong terminate.

### 2. Cap nhat `/health/aws` metadata cho dep

Hien tai `/health/aws` van tra:

```text
s3FrontendBucket = omnistay-frontend-306656769227-new
cloudFrontDomain = not-created-yet
```

Dieu nay khong lam hong app, nhung evidence chua dep vi frontend thuc te dang o account moi:

```text
omnistay-frontend-484504929670
d1gm8xt8e0mar4.cloudfront.net
```

Can cap nhat backend env/Parameter Store/Launch Template de `/health/aws` hien dung:

```text
AWS__S3FrontendBucket=omnistay-frontend-484504929670
AWS__CloudFrontDomain=d1gm8xt8e0mar4.cloudfront.net
```

Sau khi sua, restart service hoac refresh instance.

### 3. Dua secret ra SSM/Secrets Manager sau demo

Hien user data/systemd service van chua phai cach production tot nhat vi co secret trong config runtime. Sau demo nen chuyen:

- RDS password -> Secrets Manager.
- JWT secret -> SSM SecureString hoac Secrets Manager.
- Admin seed password -> SSM SecureString hoac Secrets Manager.

## Troubleshooting nhanh theo loi da gap

### CloudFront `/health` bi 504

Da tung gap. Cach sua:

```text
CloudFront -> Origins -> omnistay-api-alb -> Edit
Protocol = HTTP only
HTTP port = 80
Origin path = blank
```

Kiem them:

```text
ALB scheme = Internet-facing
ALB SG inbound HTTP 80 = 0.0.0.0/0
ALB listener HTTP:80 -> target group
Target group Healthy
```

### CloudFront `/api/...` loi 403/404

Kiem:

```text
Behavior /api/* -> origin omnistay-api-alb
Allowed methods = GET, HEAD, OPTIONS, PUT, POST, PATCH, DELETE
Cache policy = Managed-CachingDisabled
Origin request policy = Managed-AllViewerExceptHostHeader
```

### CloudFront `/public/index.html` bi S3 AccessDenied

Da tung gap. Cac nguyen nhan va cach sua:

1. S3 origin tro nham bucket.

Dung:

```text
omnistay-frontend-484504929670.s3.ap-southeast-1.amazonaws.com
```

Sai tung gap:

```text
omnistay-frontend-887329217416.s3.ap-southeast-1.amazonaws.com
```

2. OAC/bucket policy khong khop.

Dung:

```text
Origin access = Origin access control settings
OAC = oac-omnistay-frontend-484504929670
Bucket policy Resource = arn:aws:s3:::omnistay-frontend-484504929670/*
Bucket policy SourceArn = arn:aws:cloudfront::484504929670:distribution/E25IVE7NGAHBJP
```

Sau khi sua:

```text
CloudFront invalidation /*
```

### Login admin bi HTTP 500

Da tung gap. Log:

```text
IDX10653: The encryption algorithm 'HS256' requires a key size of at least '128' bits.
Key ... is of size: '96'.
```

Nguyen nhan:

```text
Jwt__Secret qua ngan.
```

Fix tam tren EC2 dang chay:

```bash
sudo mkdir -p /etc/systemd/system/omnistay-api.service.d
printf '[Service]\nEnvironment=Jwt__Secret=<JWT_SECRET_DAI_HON_32_KY_TU>\n' | sudo tee /etc/systemd/system/omnistay-api.service.d/10-jwt-secret.conf
sudo systemctl daemon-reload
sudo systemctl restart omnistay-api
```

Khong ghi secret that vao file nay.

Kiem log neu con loi:

```bash
sudo journalctl -u omnistay-api --since "10 minutes ago" --no-pager
```

Fix ben vung:

```text
Update Launch Template user data Jwt__Secret thanh secret dai hon 32 ky tu.
```

## Checklist hoan thanh du an/demo

### Bat buoc truoc khi nop/demo

- [x] Backend chay tren EC2/ASG qua ALB.
- [x] ALB public HTTP 80 chay `/health/aws`.
- [x] Frontend upload len S3 account moi.
- [x] CloudFront co S3 origin va ALB origin.
- [x] CloudFront `/health/aws` chay.
- [x] CloudFront `/api/hotels/search` chay.
- [x] CloudFront `/public/index.html` mo duoc UI.
- [x] Admin login qua CloudFront thanh cong.
- [ ] Admin dashboard load users/hotels/bookings qua CloudFront.
- [ ] Customer register/login qua CloudFront.
- [ ] Customer search Da Nang qua UI.
- [ ] Tao booking qua UI.
- [ ] Mock payment qua UI.
- [ ] Booking cua toi hien booking da thanh toan/confirmed.
- [ ] Chup du evidence UI public.
- [ ] Chup Redis cache HIT/MISS evidence.
- [ ] Chup RDS/Redis/ALB/Target Group/CloudWatch evidence.
- [ ] Sua Launch Template `Jwt__Secret` dai hon 32 ky tu.
- [ ] Cap nhat health metadata `AWS__S3FrontendBucket` va `AWS__CloudFrontDomain`.
- [ ] Cleanup chi phi sau demo neu khong can chay tiep.

### Nen lam neu con thoi gian

- [ ] Tao Launch Template version moi doc secret tu SSM/Secrets Manager thay vi ghi trong user data.
- [ ] Instance refresh co kiem soat de chung minh ASG tao instance moi tu template moi.
- [ ] Load test nhe bang Locust de co metric ALB/EC2/Redis/RDS.
- [ ] Tao/kiem tra CloudWatch dashboard `OmniStay-Demo`.
- [ ] Cap nhat README voi public demo URL.

## Cleanup chi phi sau demo

Neu khong dung tiep, lam de tranh ton tien:

Account cu `306656769227`:

- ASG `omnistay-api-asg`: set desired = 0, min = 0.
- RDS `omnistay-mysql`: stop instance neu co the.
- ElastiCache/Valkey `omnistay-cache`: delete neu khong dung tiep, vi khong co stop.
- NAT Gateway: delete neu khong can, release Elastic IP.
- ALB `omnistay-alb`: delete neu khong can public test.

Account moi `484504929670`:

- CloudFront: disable/delete neu khong can public URL.
- S3 bucket: co the giu neu chi phi thap, nhung xem Billing.

## File lien quan trong repo

Tien do cu:

```text
docs/demo/progress-2026-07-09-aws-deploy.md
```

Runbook chi tiet:

```text
docs/runbooks/phase-b-c-deploy-code-omnistay-aws.md
```

Frontend config production mau:

```text
frontend/public/config.aws.example.js
```

Frontend source:

```text
frontend/public/
frontend/src/
```

Backend source:

```text
backend/src/HotelBooking.Api/
```

## Noi ngan cho nguoi tiep tuc

Neu can lam tiep ngay, thu tu nen la:

1. Dung URL `https://d1gm8xt8e0mar4.cloudfront.net/public/home.html` va session admin da login neu con.
2. Chup screenshot home/admin login thanh cong.
3. Vao Admin dashboard, kiem data load va chup anh.
4. Logout, dang ky customer moi, search Da Nang, tao booking, mock payment, chup anh tung man.
5. Lay Redis HIT/MISS log tren EC2.
6. Chup AWS evidence: CloudFront behaviors/origins/invalidation, S3 object list/policy, ALB/Target Group healthy, RDS/Redis metrics.
7. Sua ben vung Launch Template `Jwt__Secret`, `AWS__S3FrontendBucket`, `AWS__CloudFrontDomain`.
8. Sau demo cleanup chi phi.

