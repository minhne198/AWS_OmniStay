# OmniStay AWS deploy progress - 2026-07-09

File nay la ban ban giao tien do Pha B-C. Khong ghi secret that, password, JWT secret, access key, token vao file nay.

## Muc tieu dang lam

Deploy code OmniStay len AWS de demo public:

- Backend ASP.NET Core chay tren EC2 Auto Scaling Group, qua ALB.
- Frontend static HTML/JS chay tren S3 va CloudFront.
- CloudFront route:
  - Static frontend -> S3 frontend bucket.
  - `/api/*` -> ALB/backend.
  - `/health*` -> ALB/backend.
- Smoke test public full flow: admin, customer, search, booking, mock payment, booking history.

## Thong tin AWS da biet

```text
AWS account id: 306656769227
Region: ap-southeast-1

Artifact bucket: omnistay-artifacts-306656769227-new
Frontend bucket: omnistay-frontend-306656769227-new

RDS MySQL endpoint: omnistay-mysql.c5wo68sksevz.ap-southeast-1.rds.amazonaws.com
RDS database: hotel_booking
RDS username: admin
RDS password: KHONG_GHI_VAO_REPO

Redis/Valkey primary endpoint: master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com:6379
Redis connection string used: master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com:6379,ssl=True,abortConnect=false

ALB name: omnistay-alb
ALB DNS: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com

Target group: omnistay-api-tg
Target group port: HTTP 8080
Target group health path: /health

Launch template: omnistay-api-lt-v2
Launch template id: lt-077ae45544a46ef24
ASG: omnistay-api-asg
IAM role / instance profile: EC2-HotelAPI-Role

CloudFront domain: CHUA_TAO_HOAC_CHUA_GHI_LAI
JWT secret: KHONG_GHI_VAO_REPO
Admin seed password: KHONG_GHI_VAO_REPO
```

## Da hoan thanh

### Code va local MVP

- Da sua admin frontend/API mismatch:
  - Them admin user API wrappers.
  - Doi admin.js dung ten wrapper backend.
  - Doi role mac dinh tu `User` sang `Customer`.
- Da them `.gitignore` cho runtime logs/cache:
  - `.codex-*.log`
  - `.omc/`
- Backend tests pass: 11/11.
- JS syntax check pass.
- Local full flow MVP da pass:
  - Admin login.
  - Admin load users/hotels/bookings.
  - Customer register/login.
  - Search hotel.
  - Create booking.
  - Mock payment.
  - Booking history.
  - System status.
- Da commit va push:
  - Commit: `54888e9 Prepare OmniStay MVP for AWS deploy`
  - Branch: `main`

### Backend artifact

- Da publish backend local.
- Da tao zip:

```text
C:\tmp\omnistay-api.zip
```

- Da upload zip len S3 artifact bucket:

```text
s3://omnistay-artifacts-306656769227-new/omnistay-api.zip
```

### Parameter Store / config

Da tao hoac da co cac parameter can thiet:

```text
/omnistay/prod/AWS__Region = ap-southeast-1
/omnistay/prod/Cache__SearchTtlSeconds = 120
/omnistay/prod/Database__Provider = MySql
/omnistay/prod/AWS__S3FrontendBucket = omnistay-frontend-306656769227-new
/omnistay/prod/Jwt__Secret = SecureString, KHONG_GHI_GIA_TRI
/omnistay/prod/Admin__SeedPassword = SecureString, KHONG_GHI_GIA_TRI
```

Chua tao hoac chua cap nhat:

```text
/omnistay/prod/AWS__CloudFrontDomain
```

Ly do: luc kiem tra CloudFront chua co distribution/domain.

### Launch Template / ASG / EC2

- Launch template thuc te la Ubuntu 24.04, khong phai Amazon Linux.
- Loi ban dau:
  - User data co `awscli` trong `apt-get install`, tren Ubuntu 24.04 package nay khong co san.
  - Cloud-init fail truoc khi tao `omnistay-api.service`.
- Da sua tam tren EC2 bang SSM:
  - Cai AWS CLI v2 bang zip tu AWS.
  - Chay lai bootstrap fixed.
  - Tao lai systemd service tu phan service trong script.
- Service tren EC2 da chay:

```text
omnistay-api.service active (running)
Now listening on: http://0.0.0.0:8080
Hosting environment: Production
```

- Test tren EC2 da pass:

```text
curl -i http://localhost:8080/health
HTTP/1.1 200 OK
```

- Target group da Healthy.
- Da sua Launch Template user data cho ben hon:
  - Cai AWS CLI v2 dung cach cho Ubuntu.
  - Cai `aspnetcore-runtime-9.0`.
  - Download zip tu S3 artifact bucket.
  - Tao va start `omnistay-api.service`.
  - Them IAM instance profile `EC2-HotelAPI-Role`.
- ASG da duoc set dung launch template version Latest theo thong tin nguoi dung bao.

### ALB

- ALB `omnistay-alb`:
  - Status: Active.
  - Scheme: Internet-facing.
  - DNS: `omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com`.
- Da mo inbound HTTP port 80 cho security group cua ALB:

```text
Type: HTTP
Port: 80
Source: 0.0.0.0/0
```

- User bao test ALB da duoc.
- Buoc can chup bang chung:

```text
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health/aws
```

### Frontend production folder

Da tao folder frontend production local:

```text
C:\tmp\omnistay-frontend
```

Da tao file:

```text
C:\tmp\omnistay-frontend\public\config.js
```

Noi dung da verify:

```js
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

Da verify co cac file can upload:

```text
C:\tmp\omnistay-frontend\public\index.html
C:\tmp\omnistay-frontend\public\home.html
C:\tmp\omnistay-frontend\public\admin.html
C:\tmp\omnistay-frontend\src\api.js
C:\tmp\omnistay-frontend\src\session.js
C:\tmp\omnistay-frontend\src\admin.js
C:\tmp\omnistay-frontend\public\config.js
```

## Chua lam

### Frontend S3

- Chua upload frontend len S3 bucket:

```text
omnistay-frontend-306656769227-new
```

- Can upload noi dung ben trong:

```text
C:\tmp\omnistay-frontend
```

Len bucket sao cho object path tren S3 la:

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

Khong upload thanh:

```text
omnistay-frontend/public/index.html
```

### CloudFront

- Luc truoc CloudFront distribution list dang trong.
- Can tao CloudFront distribution hoac neu da tao thi can cau hinh:
  - Default behavior -> S3 frontend origin.
  - `/api/*` -> ALB origin.
  - `/health*` -> ALB origin.
  - Disable cache cho API/health.
  - Forward query strings cho API.
- Sau khi co domain CloudFront, can cap nhat:

```text
/omnistay/prod/AWS__CloudFrontDomain = <cloudfront-domain>
```

- Neu can, tao Launch Template version moi thay:

```text
Environment=AWS__CloudFrontDomain=not-created-yet
```

Bang domain CloudFront that.

### Public smoke test

Chua test cac URL CloudFront:

```text
https://<cloudfront-domain>/health
https://<cloudfront-domain>/health/aws
https://<cloudfront-domain>/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
https://<cloudfront-domain>/public/index.html
```

Chua test UI public:

- Login admin.
- Admin dashboard load users/hotels/bookings.
- Register customer moi.
- Search Da Nang.
- Dat phong.
- Mock payment.
- My bookings hien `Da thanh toan` hoac trang thai tuong duong.

### Evidence chua chup day du

Can chup:

- `/health/aws` qua ALB.
- S3 frontend object list.
- CloudFront behaviors.
- CloudFront invalidation completed.
- Public UI:
  - Login page.
  - Admin dashboard.
  - Search result.
  - Booking confirmation.
  - My bookings.
- Redis cache HIT/MISS logs neu co.
- RDS metrics/endpoint/security group.
- Target group healthy va health path `/health`.
- ALB listener HTTP 80 va ALB DNS.
- CloudWatch dashboard `OmniStay-Demo`.

## Rui ro / can nho

- Secret dang nam trong Launch Template user data de demo nhanh. Day la chap nhan tam thoi cho demo, nhung sau do nen chuyen sang SSM Parameter Store/Secrets Manager.
- Current EC2 instance da tung duoc sua tay bang SSM. Launch Template da duoc sua lai, nhung chua test bang cach cho ASG tao instance moi tu dau sau khi sua.
- Redis/Valkey bat transit encryption, nen connection string can co `ssl=True`.
- Neu ASG tao instance moi bi unhealthy:
  - Kiem `/var/log/cloud-init-output.log`.
  - Kiem `systemctl status omnistay-api`.
  - Kiem IAM role co `AmazonS3ReadOnlyAccess`.
  - Kiem user data con `awscli` trong `apt-get install` khong.
- Neu ALB timeout:
  - Kiem ALB scheme `Internet-facing`.
  - Kiem ALB SG inbound port 80 tu `0.0.0.0/0`.
  - Kiem listener HTTP 80 forward target group.
- Neu ALB 502:
  - Kiem target group healthy.
  - Kiem EC2 SG inbound 8080 tu ALB SG.
  - Kiem app nghe port 8080.

## Buoc tiep theo nen lam

1. Upload frontend len S3 bucket `omnistay-frontend-306656769227-new`.
2. Tao/cau hinh CloudFront distribution.
3. Invalidate CloudFront cache `/*`.
4. Test public health/API/UI qua CloudFront.
5. Chup evidence.
6. Sau demo, cleanup chi phi neu khong dung tiep.

