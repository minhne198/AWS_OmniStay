# Pha B-C: Deploy Code OmniStay Len AWS Va Test Demo

Runbook nay dung de tiep tuc deploy code OmniStay len AWS sau khi Pha A da tao resource nen tang. Khong ghi secret that vao file nay.

## Nguyen tac an toan

- Khong commit password, JWT secret, access key, token.
- Gia tri secret luu rieng ngoai repo, vi du file local khong dua len Git.
- User data co secret chi dung tam cho demo nhanh. Sau khi demo nen doi sang doc tu SSM Parameter Store va Secrets Manager.
- Neu dua file nay cho AI khac, chi dua placeholder, khong dua secret that.

## Pha B.1 - Ghi lai gia tri can dung

Tao file note rieng ngoai repo, vi du:

```text
C:\tmp\omnistay-prod-secrets.txt
```

Ghi cac gia tri sau, nhung khong commit:

```text
ARTIFACT_BUCKET=omnistay-artifacts-306656769227-new
FRONTEND_BUCKET=omnistay-frontend-306656769227-new
RDS_ENDPOINT=omnistay-mysql.c5wo68sksevz.ap-southeast-1.rds.amazonaws.com
RDS_DATABASE=hotel_booking
RDS_USERNAME=admin
RDS_PASSWORD=<database-password>
REDIS_ENDPOINT=master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com
REDIS_PORT=6379
ALB_DNS=omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
CLOUDFRONT_DOMAIN=<cloudfront-domain-sau-khi-tao>
JWT_SECRET=<chuoi-32-ky-tu-tro-len>
ADMIN_PASSWORD=<password-admin-production>
```

Luu y Redis/Valkey cua project nay bat transit encryption, nen connection string can dung:

```text
master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com:6379,ssl=True,abortConnect=false
```

## Pha B.2 - Publish backend tren may local

O repo:

```text
D:\AWS_OmniStay
```

Chay:

```powershell
dotnet test backend\AWSOmniStay.slnx --no-restore
dotnet publish backend\src\HotelBooking.Api\HotelBooking.Api.csproj -c Release -o C:\tmp\omnistay-publish
Compress-Archive -Path C:\tmp\omnistay-publish\* -DestinationPath C:\tmp\omnistay-api.zip -Force
```

Ket qua can co:

```text
C:\tmp\omnistay-api.zip
```

## Pha B.3 - Upload backend zip len S3 artifact bucket

Vao AWS Console:

```text
S3 -> omnistay-artifacts-306656769227-new
```

Lam:

1. Bam **Upload**.
2. Bam **Add files**.
3. Chon:

```text
C:\tmp\omnistay-api.zip
```

4. Bam **Upload**.
5. Sau khi xong, ghi lai object path:

```text
s3://omnistay-artifacts-306656769227-new/omnistay-api.zip
```

Can chup anh:

- Object `omnistay-api.zip` trong bucket artifact.

## Pha B.4 - Kiem Parameter Store / Secrets

Vao:

```text
Systems Manager -> Parameter Store
```

Can co:

```text
/omnistay/prod/Database__Provider = MySql
/omnistay/prod/AWS__Region = ap-southeast-1
/omnistay/prod/Cache__SearchTtlSeconds = 120
/omnistay/prod/AWS__S3FrontendBucket = omnistay-frontend-306656769227-new
/omnistay/prod/AWS__CloudFrontDomain = <cloudfront-domain>
/omnistay/prod/Jwt__Secret = <JWT secret, SecureString>
/omnistay/prod/Admin__SeedPassword = <admin password, SecureString>
```

Neu CloudFront chua tao, tam thoi bo qua:

```text
/omnistay/prod/AWS__CloudFrontDomain
```

Sau khi co CloudFront domain thi tao/cap nhat lai.

RDS password co the nam trong Secrets Manager:

```text
omnistay/prod/hotelbookingdb
```

Neu demo nhanh bang user data, password co the duoc paste vao Launch Template user data, nhung day chi la tam thoi.

## Pha B.5 - Sua Launch Template user data cho Ubuntu 24.04

Vao:

```text
EC2 -> Launch Templates -> omnistay-api-lt-v2
```

Lam:

1. Tick chon launch template.
2. **Actions -> Modify template (Create new version)**.
3. Kiem tra **IAM instance profile**:

```text
EC2-HotelAPI-Role
```

4. Keo xuong **Advanced details -> User data**.
5. Xoa user data cu.
6. Paste script nay, thay 3 placeholder secret:

```bash
#!/bin/bash
set -euxo pipefail

export DEBIAN_FRONTEND=noninteractive

apt-get update -y
apt-get install -y unzip curl software-properties-common ca-certificates

cd /tmp
rm -rf aws awscliv2.zip
curl -fsSL "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip -q -o awscliv2.zip

if [ -x /usr/local/bin/aws ]; then
  /tmp/aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
else
  /tmp/aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli
fi

add-apt-repository -y ppa:dotnet/backports
apt-get update -y
apt-get install -y aspnetcore-runtime-9.0

mkdir -p /opt/omnistay/api
cd /opt/omnistay

/usr/local/bin/aws s3 cp s3://omnistay-artifacts-306656769227-new/omnistay-api.zip /opt/omnistay/omnistay-api.zip
unzip -o /opt/omnistay/omnistay-api.zip -d /opt/omnistay/api

cat >/etc/systemd/system/omnistay-api.service <<'EOF'
[Unit]
Description=AWS OmniStay API
After=network.target

[Service]
WorkingDirectory=/opt/omnistay/api
ExecStart=/usr/bin/dotnet /opt/omnistay/api/HotelBooking.Api.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
Environment=Database__Provider=MySql
Environment=ConnectionStrings__HotelBookingDb=server=omnistay-mysql.c5wo68sksevz.ap-southeast-1.rds.amazonaws.com;port=3306;database=hotel_booking;user=admin;password=RDS_PASSWORD_CUA_ANH;SslMode=Required
Environment=ConnectionStrings__Redis=master.omnistay-cache.v9zb0n.apse1.cache.amazonaws.com:6379,ssl=True,abortConnect=false
Environment=Cache__SearchTtlSeconds=120
Environment=Jwt__Issuer=AWSOmniStay
Environment=Jwt__Audience=AWSOmniStay.Web
Environment=Jwt__Secret=JWT_SECRET_CUA_ANH
Environment=Admin__Email=admin@omnistay.local
Environment=Admin__SeedPassword=ADMIN_PASSWORD_CUA_ANH
Environment=AWS__Region=ap-southeast-1
Environment=AWS__S3FrontendBucket=omnistay-frontend-306656769227-new
Environment=AWS__CloudFrontDomain=not-created-yet

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable omnistay-api
systemctl restart omnistay-api
```

Can thay:

```text
RDS_PASSWORD_CUA_ANH
JWT_SECRET_CUA_ANH
ADMIN_PASSWORD_CUA_ANH
```

Khong tick:

```text
User data has already been base64 encoded
```

Bam:

```text
Create template version
```

## Pha B.6 - Cho ASG dung launch template version moi

Vao:

```text
EC2 -> Auto Scaling Groups -> omnistay-api-asg
```

Lam:

1. Tab **Details**.
2. Neu Launch template version chua la **Latest**, bam **Edit**.
3. Chon:

```text
Version = Latest
```

4. Set capacity:

```text
Desired capacity = 1
Minimum capacity = 1
Maximum capacity = 2 hoac 4
```

5. Bam **Update**.

Neu muon test launch template moi that su tu dau:

1. Vao tab **Instance management**.
2. Terminate instance cu hoac start Instance refresh.
3. Cho ASG tao instance moi.
4. Doi target group Healthy.

## Pha B.7 - Kiem EC2 service neu target unhealthy

Dung SSM Session Manager:

```text
EC2 -> Instances -> chon instance -> Connect -> Session Manager
```

Lenh kiem tra:

```bash
sudo tail -n 120 /var/log/cloud-init-output.log
sudo systemctl status omnistay-api --no-pager
curl -i http://localhost:8080/health
curl -i http://localhost:8080/health/aws
```

Expected service:

```text
Active: active (running)
Now listening on: http://0.0.0.0:8080
```

Expected health:

```text
HTTP/1.1 200 OK
```

## Pha B.8 - Kiem Target Group Healthy

Vao:

```text
EC2 -> Target Groups -> omnistay-api-tg -> Targets
```

Can thay:

```text
Healthy = 1
Unhealthy = 0
```

Health check settings:

```text
Protocol: HTTP
Path: /health
Port: Traffic port
Success codes: 200
```

Neu target khong duoc register:

1. Vao `EC2 -> Auto Scaling Groups -> omnistay-api-asg`.
2. Tab **Integrations**.
3. Edit load balancing.
4. Tick **Application, Network or Gateway Load Balancer target groups**.
5. Chon `omnistay-api-tg`.
6. Update.

Neu target unhealthy:

- Kiem EC2 SG inbound `8080` tu ALB SG.
- Kiem app co chay port `8080`.
- Kiem target group path `/health`.
- Kiem cloud-init/systemd logs.

## Pha B.9 - Test backend qua ALB

Vao:

```text
EC2 -> Load Balancers -> omnistay-alb
```

Can thay:

```text
Status: Active
Scheme: Internet-facing
DNS name: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
```

Kiem listener:

```text
HTTP:80 -> forward to omnistay-api-tg
```

Kiem ALB security group inbound:

```text
Type: HTTP
Protocol: TCP
Port: 80
Source: 0.0.0.0/0
```

Mo browser:

```text
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health
http://omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com/health/aws
```

Expected `/health`:

- Trang trang hoac 200 OK.
- Khong timeout.
- Khong 502/503/504.

Expected `/health/aws`:

```json
{
  "environment": "Production",
  "databaseProvider": "MySql",
  "redisConfigured": true,
  "redisConnected": true,
  "apiBasePath": "/api"
}
```

Neu `redisConnected` la `false`:

- Kiem Redis/Valkey endpoint dung.
- Kiem connection string co `ssl=True`.
- Kiem SG Redis inbound `6379` tu SG EC2.

Chup anh:

- `/health/aws` qua ALB.
- ALB listener HTTP 80.
- Target group Healthy.

## Pha B.10 - Chuan bi frontend production local

Tren may local:

```powershell
Remove-Item C:\tmp\omnistay-frontend -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item frontend C:\tmp\omnistay-frontend -Recurse -Force
Copy-Item frontend\public\config.aws.example.js C:\tmp\omnistay-frontend\public\config.js -Force
```

Kiem:

```text
C:\tmp\omnistay-frontend\public\config.js
```

Noi dung phai la:

```js
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

Can co cac file:

```text
C:\tmp\omnistay-frontend\public\index.html
C:\tmp\omnistay-frontend\public\home.html
C:\tmp\omnistay-frontend\public\admin.html
C:\tmp\omnistay-frontend\src\api.js
C:\tmp\omnistay-frontend\src\session.js
C:\tmp\omnistay-frontend\src\admin.js
```

## Pha B.11 - Upload frontend len S3

Vao:

```text
S3 -> omnistay-frontend-306656769227-new
```

Lam:

1. Bam **Upload**.
2. Bam **Add folder** hoac keo tha cac folder/file.
3. Chon noi dung ben trong:

```text
C:\tmp\omnistay-frontend
```

4. Dam bao tren S3 se co path:

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

5. Bam **Upload**.

Chup anh:

- Bucket object list co `public/`, `src/`, `assets/`.

## Pha B.12 - Tao hoac cau hinh CloudFront

Neu chua co distribution, vao:

```text
CloudFront -> Distributions -> Create distribution
```

### Origin 1 - S3 frontend

Origin domain:

```text
omnistay-frontend-306656769227-new.s3.ap-southeast-1.amazonaws.com
```

Origin name:

```text
omnistay-frontend-s3
```

Origin access:

```text
Origin access control settings (recommended)
```

Tao OAC moi neu chua co. Sau khi create distribution, CloudFront co the hien nut/prompt de copy bucket policy. Lam theo prompt de update S3 bucket policy.

Default behavior:

```text
Origin: omnistay-frontend-s3
Viewer protocol policy: Redirect HTTP to HTTPS
Allowed methods: GET, HEAD
Cache policy: CachingOptimized
```

Default root object:

```text
public/index.html
```

### Origin 2 - ALB backend

Them origin:

```text
Origin domain: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
Origin name: omnistay-api-alb
Protocol: HTTP only
HTTP port: 80
```

### Behavior `/api/*`

Them behavior:

```text
Path pattern: /api/*
Origin: omnistay-api-alb
Viewer protocol policy: Redirect HTTP to HTTPS
Allowed methods: GET, HEAD, OPTIONS, PUT, POST, PATCH, DELETE
Cache policy: CachingDisabled
Origin request policy: AllViewerExceptHostHeader neu co, hoac policy co forward query strings
```

Quan trong:

- Forward query strings phai bat.
- Khong cache API dynamic.

### Behavior `/health*`

Them behavior:

```text
Path pattern: /health*
Origin: omnistay-api-alb
Viewer protocol policy: Redirect HTTP to HTTPS
Allowed methods: GET, HEAD, OPTIONS
Cache policy: CachingDisabled
```

Chup anh:

- CloudFront behaviors co:

```text
Default (*) -> S3 frontend origin
/api/* -> ALB origin
/health* -> ALB origin
```

## Pha B.13 - Cap nhat CloudFront domain vao Parameter Store

Sau khi CloudFront deploy xong, lay domain dang:

```text
dxxxxxxxxxxxxx.cloudfront.net
```

Vao:

```text
Systems Manager -> Parameter Store
```

Tao hoac cap nhat:

```text
/omnistay/prod/AWS__CloudFrontDomain = dxxxxxxxxxxxxx.cloudfront.net
```

Neu muon backend environment dung gia tri that, tao Launch Template version moi thay:

```text
Environment=AWS__CloudFrontDomain=not-created-yet
```

Bang:

```text
Environment=AWS__CloudFrontDomain=dxxxxxxxxxxxxx.cloudfront.net
```

Sau do ASG dung version moi. Buoc nay khong bat buoc neu app khong can field nay trong demo.

## Pha B.14 - Invalidate CloudFront cache

Vao:

```text
CloudFront -> Distribution -> Invalidations -> Create invalidation
```

Object paths:

```text
/*
```

Cho status:

```text
Completed
```

Chup anh invalidation completed.

## Pha C.1 - Test health qua CloudFront

Mo:

```text
https://<cloudfront-domain>/health
https://<cloudfront-domain>/health/aws
```

Expected `/health/aws`:

```text
environment = Production
databaseProvider = MySql
redisConfigured = true
redisConnected = true
apiBasePath = /api
```

Neu bi 403:

- Kiem behavior `/health*` da tro ALB chua.
- Kiem behavior order co dung khong.

Neu bi 502/504:

- Kiem ALB health.
- Kiem CloudFront origin ALB dung DNS.
- Kiem ALB security group port 80.

## Pha C.2 - Test API search qua CloudFront

Mo:

```text
https://<cloudfront-domain>/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Expected:

- Tra JSON array.
- Co hotel o Da Nang neu seed data da co.
- Khong loi 403/502/504.

Neu API search loi:

- Kiem behavior `/api/*`.
- Kiem allowed methods.
- Kiem query strings duoc forward.
- Kiem cache policy la `CachingDisabled`.

## Pha C.3 - Test frontend UI public

Mo:

```text
https://<cloudfront-domain>/public/index.html
```

Flow demo:

1. Login admin:
   - Email: `admin@omnistay.local`
   - Password: password admin production, khong ghi vao file.
2. Vao `Admin`.
3. Kiem users/hotels/bookings load duoc.
4. Logout.
5. Register customer moi.
6. Search `Da Nang`.
7. Chon hotel.
8. Dat phong.
9. Mock payment.
10. Vao booking cua toi.
11. Kiem booking hien da thanh toan/confirmed.

Chup anh:

- Login page.
- Admin dashboard.
- Search result.
- Booking confirmation.
- My bookings.

## Pha C.4 - Test Redis cache evidence

Mo URL search 2 lan:

```text
https://<cloudfront-domain>/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Sau do tim log:

```text
Hotel search cache MISS
Hotel search cache HIT
```

Co the xem tren EC2:

```bash
sudo journalctl -u omnistay-api -n 300 --no-pager
```

Hoac CloudWatch Logs neu da day log len CloudWatch.

Chup anh log neu co.

## Pha C.5 - Test RDS evidence

Vao:

```text
RDS -> Databases -> omnistay-mysql
```

Chup:

- Endpoint.
- Security group.
- CPU metric.
- Connections metric.

Neu co cach query DB, kiem cac bang:

```text
Users
Hotels
RoomTypes
Bookings
```

## Pha C.6 - Test ALB evidence

Vao:

```text
EC2 -> Target Groups -> omnistay-api-tg
```

Chup:

- Target healthy.
- Health check path `/health`.

Vao:

```text
EC2 -> Load Balancers -> omnistay-alb
```

Chup:

- Listener HTTP 80.
- ALB DNS.
- Scheme Internet-facing.

## Pha C.7 - Test CloudWatch Dashboard

Vao:

```text
CloudWatch -> Dashboards -> OmniStay-Demo
```

Sau khi da tao traffic, chup dashboard co:

- ALB request count.
- EC2 CPU.
- RDS connections.
- Redis connections.
- ASG capacity.

## Troubleshooting nhanh

### Browser vao ALB bi timeout

Kiem:

- ALB scheme la `Internet-facing`.
- ALB SG inbound HTTP 80 tu `0.0.0.0/0`.
- Listener HTTP 80 ton tai.
- ALB subnet la public subnet co route ra Internet Gateway.

### ALB 502

Kiem:

- Target group healthy.
- EC2 service:

```bash
sudo systemctl status omnistay-api --no-pager
```

- App listen port 8080:

```bash
curl -i http://localhost:8080/health
```

- EC2 SG inbound 8080 tu ALB SG.

### Target group unhealthy

Kiem:

- Health path `/health`.
- Target group port 8080.
- App running.
- User data log:

```bash
sudo tail -n 200 /var/log/cloud-init-output.log
```

### User data fail vi awscli

Neu log co:

```text
E: Package 'awscli' has no installation candidate
```

Thi user data cu sai voi Ubuntu 24.04. Dung user data trong runbook nay, cai AWS CLI v2 bang zip.

### `/health/aws` van la InMemory

Sai hoac thieu:

```text
Database__Provider=MySql
```

Sua Launch Template/user data, tao version moi, cho ASG tao instance moi.

### Redis `redisConnected: false`

Kiem:

- Redis endpoint dung.
- Connection string co `ssl=True`.
- SG Redis inbound 6379 tu SG EC2.
- EC2 cung VPC/private subnet voi Redis.

### Frontend load duoc nhung API loi

Kiem:

- `public/config.js` tren S3:

```js
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

- CloudFront behavior `/api/*` tro ALB.
- Forward query strings bat.
- Da invalidate CloudFront cache.

### Admin login sai

Kiem:

- `Admin__SeedPassword` trong user data.
- Neu admin da seed bang password cu trong DB, password moi co the khong override. Can update password trong DB hoac tao DB moi neu demo cho phep.

## Cleanup sau demo de giam chi phi

Neu khong chay tiep:

1. ASG:

```text
Desired = 0
Min = 0
```

2. RDS:

```text
Stop DB instance
```

3. ElastiCache/Valkey:

```text
Delete neu khong dung tiep
```

4. NAT Gateway:

```text
Delete neu muon tiet kiem manh
Release Elastic IP
```

5. ALB:

```text
Delete neu khong can public test
```

6. CloudFront/S3:

```text
Co the giu neu chi phi thap, nhung nho xem Billing
```

## Handoff cho AI khac

Neu can hoi AI khac, gui kem:

- File tien do:

```text
docs/demo/progress-2026-07-09-aws-deploy.md
```

- File runbook nay:

```text
docs/runbooks/phase-b-c-deploy-code-omnistay-aws.md
```

- Khong gui password/JWT secret.
- Noi AI khac tiep tuc tu muc "Chua lam" trong file tien do.

