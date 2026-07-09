# AWS OmniStay MVP Public Deploy Checklist

Checklist nay dung sau khi MVP local da pass test. Khong ghi secret that vao file nay.

## 1. Local MVP acceptance

- Backend tests pass: `dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj`.
- Frontend static page load duoc: `http://127.0.0.1:5173/public/index.html`.
- Full flow local:
  - Register/login customer.
  - Search hotel.
  - Create booking.
  - Mock payment -> booking `Confirmed`.
  - My bookings hien booking vua tao.
  - Admin login bang seed account va xem hotels/bookings.

Default local admin:

```text
Email: admin@omnistay.local
Password: Admin@123456
```

Khi deploy AWS, doi password nay bang `Admin__SeedPassword` trong SSM/Secrets hoac environment variable.

## 2. AWS values can thu thap

Ghi rieng ngoai repo cac gia tri sau:

```text
RDS endpoint:
RDS database: hotel_booking
RDS username:
RDS password:
Redis endpoint:
S3 frontend bucket:
S3 artifact bucket:
ALB DNS:
CloudFront domain:
JWT secret:
Admin seed password:
```

## 3. Backend production environment

Set tren EC2/systemd/user-data:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
Database__Provider=MySql
ConnectionStrings__HotelBookingDb=server=<rds-endpoint>;port=3306;database=hotel_booking;user=<user>;password=<password>;SslMode=Required
ConnectionStrings__Redis=<redis-endpoint>:6379,abortConnect=false
Jwt__Issuer=AWSOmniStay
Jwt__Audience=AWSOmniStay.Web
Jwt__Secret=<32+ character secret>
Admin__Email=admin@omnistay.local
Admin__SeedPassword=<strong admin password>
AWS__Region=ap-southeast-1
AWS__S3FrontendBucket=<frontend-bucket>
AWS__CloudFrontDomain=<cloudfront-domain>
```

## 4. Backend deploy

- Publish backend:

```powershell
C:\tmp\dotnet10\dotnet.exe publish backend\src\HotelBooking.Api\HotelBooking.Api.csproj -c Release -o C:\tmp\omnistay-publish
```

- Zip output and upload to S3 artifact bucket.
- EC2 Launch Template/user-data downloads artifact and runs:

```text
/usr/bin/dotnet /opt/omnistay/api/HotelBooking.Api.dll
```

- Target group:
  - Protocol: HTTP.
  - Port: 8080.
  - Health path: `/health`.

## 5. Frontend deploy

- Copy `frontend/public/config.aws.example.js` to production `config.js`.
- Confirm production config:

```javascript
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

- Upload frontend static files to S3 frontend bucket.
- CloudFront:
  - Default behavior -> S3 origin with OAC.
  - `/api/*` behavior -> ALB origin.
  - API behavior should forward query strings and not cache dynamic API responses.

## 6. Public acceptance

Run these through CloudFront URL:

```text
GET /health/aws
GET /api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Expected `/health/aws` after real AWS wiring:

```json
{
  "environment": "Production",
  "region": "ap-southeast-1",
  "databaseProvider": "MySql",
  "redisConfigured": true,
  "redisConnected": true,
  "apiBasePath": "/api"
}
```

## 7. Cost guardrails

- AWS Budget active before running full stack continuously.
- CloudWatch alarms:
  - ALB 5XX high.
  - Target unhealthy.
  - ASG CPU high.
  - RDS CPU/storage.
  - Estimated charges.
- Review Billing daily while RDS, Redis, ALB, NAT and public IPv4 are running.
