# OmniStay AWS redeploy progress - 2026-07-12

File nay ghi lai tien do deploy lai AWS sau dot bo sung tinh nang local ngay 2026-07-11. Khong ghi password, JWT secret, access key, RDS password, token hoac thong tin nhay cam vao file nay.

## Tom tat ngan

Da deploy lai ban moi len AWS theo huong Console-first:

- Da cap nhat schema RDS MySQL cho cac tinh nang moi.
- Da upload backend artifact moi len S3 artifact bucket.
- Da deploy backend moi tren EC2 bang artifact moi.
- Backend service tren EC2 da restart thanh cong.
- Local EC2 health check da pass.
- Frontend moi da duoc upload len S3 frontend bucket theo xac nhan thao tac.
- CloudFront da duoc invalidate theo xac nhan thao tac.

Ngay mai can test lai public web sau deploy qua CloudFront.

## AWS resources dang dung

Backend/runtime account:

```text
AWS account id: 306656769227
Region: ap-southeast-1
ALB DNS: omnistay-alb-1274180740.ap-southeast-1.elb.amazonaws.com
RDS endpoint: omnistay-mysql.c5wo68sksevz.ap-southeast-1.rds.amazonaws.com
RDS database: hotel_booking
Artifact bucket: omnistay-artifacts-306656769227-new
Backend artifact object: s3://omnistay-artifacts-306656769227-new/omnistay-api.zip
EC2 app path: /opt/omnistay/api
EC2 app port: 8080
Service name: omnistay-api
```

Frontend/CloudFront account:

```text
AWS account id: 484504929670
Frontend bucket: omnistay-frontend-484504929670
CloudFront distribution id: E25IVE7NGAHBJP
CloudFront domain: d1gm8xt8e0mar4.cloudfront.net
Public app URL: https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

## Local artifacts used

Backend artifact da tao local:

```text
D:\AWS_OmniStay\artifacts\omnistay-api-20260711-230944.zip
```

Sau do upload len S3 voi ten deploy chuan:

```text
omnistay-api.zip
```

Frontend artifact da tao local:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260711-230944.zip
```

Frontend production config can giu dung:

```js
window.__APP_CONFIG__ = {
  API_BASE: '/api'
};
```

## Database migration da lam

Da dang nhap MySQL bang master user `admin` va password lay tu runtime service backend dang chay. Khong ghi password vao repo.

Da them cac cot moi:

```text
Users.AvatarUrl
Users.Balance
Hotels.OwnerUserId
RoomTypes.IsHidden
Bookings.UserId
Bookings.PaymentStatus
Bookings.PaidAt
```

Trang thai khi chay migration:

- `Users.AvatarUrl`: them thanh cong.
- `Users.Balance`: them thanh cong.
- `Hotels.OwnerUserId`: them thanh cong.
- `RoomTypes.IsHidden`: them thanh cong.
- `Bookings.UserId`: da ton tai san.
- `Bookings.PaymentStatus`: da ton tai san.
- `Bookings.PaidAt`: da ton tai san.

Da tao cac bang moi:

```text
HotelReviews
Notifications
BalanceTransactions
```

Da tao cac index bo sung:

```text
IX_Hotels_OwnerUserId
IX_HotelReviews_HotelId
IX_HotelReviews_UserId
IX_Notifications_UserId_IsRead_CreatedAt
IX_BalanceTransactions_UserId_CreatedAt
IX_BalanceTransactions_BookingId
```

Kiem tra data sau migration:

```text
Users: 2
Hotels: 12
RoomTypes: 30
Bookings: 2
BalanceTransactions: 0
```

Ket luan: schema production da du cot/bang can thiet cho backend moi. Data cu van con.

## Backend deploy da lam

Tren EC2, da tai artifact moi tu S3 va unzip vao app path:

```bash
cd /opt/omnistay
sudo aws s3 cp s3://omnistay-artifacts-306656769227-new/omnistay-api.zip /opt/omnistay/omnistay-api.zip
sudo systemctl stop omnistay-api
sudo rm -rf /opt/omnistay/api/*
sudo unzip -o /opt/omnistay/omnistay-api.zip -d /opt/omnistay/api
sudo systemctl start omnistay-api
```

Health check tren EC2 da pass:

```text
sudo systemctl status omnistay-api --no-pager
Result: Active: active (running)

curl -i http://localhost:8080/health
Result: HTTP/1.1 200 OK

curl -i http://localhost:8080/health/aws
Result:
environment = Production
region = ap-southeast-1
databaseProvider = MySql
redisConfigured = true
redisConnected = true
searchCacheTtlSeconds = 120
apiBasePath = /api
```

Ghi chu: `/health/aws` van hien metadata cu:

```text
s3FrontendBucket = omnistay-frontend-306656769227-new
cloudFrontDomain = not-created-yet
```

Day chi la metadata chua dep trong env runtime, khong lam hong app. Sau demo nen cap nhat thanh:

```text
AWS__S3FrontendBucket=omnistay-frontend-484504929670
AWS__CloudFrontDomain=d1gm8xt8e0mar4.cloudfront.net
```

## Frontend deploy da lam

Theo thao tac deploy, frontend moi da duoc upload len bucket:

```text
omnistay-frontend-484504929670
```

Can dam bao object path tren S3 la:

```text
public/index.html
public/config.js
public/home.html
public/admin.html
src/api.js
src/session.js
src/admin.js
assets/
```

Khong duoc upload thanh dang co folder boc ngoai, vi du:

```text
omnistay-frontend-20260711-230944/public/index.html
```

CloudFront distribution da duoc invalidate:

```text
Distribution id: E25IVE7NGAHBJP
Invalidation path: /*
```

## Viec can test ngay mai

### 1. Health/API public

Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/health/aws
```

Can thay:

```text
environment = Production
databaseProvider = MySql
redisConfigured = true
redisConnected = true
apiBasePath = /api
```

Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/api/hotels/search?city=Da%20Nang&checkIn=2026-12-10&checkOut=2026-12-12&guests=2
```

Ban moi dung nen JSON co cac field moi:

```text
mainImageUrl
roomImageUrl
averageRating
reviewCount
```

### 2. UI public smoke test

Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

Test nhanh:

1. Login admin.
2. Vao Admin dashboard.
3. Kiem Users/Hotels/Bookings co load.
4. Logout.
5. Register customer moi.
6. Login customer.
7. Search `Da Nang`.
8. Vao hotel detail.
9. Tao booking.
10. Thanh toan.
11. Vao `Booking cua toi`.
12. Kiem booking hien dung trang thai.
13. Xem profile, balance bi tru.
14. Xem notifications.

### 3. Owner flow neu co thoi gian

1. Register hoac login HotelOwner.
2. Tao hotel moi.
3. Tao room type moi.
4. An/mo phong.
5. Login customer search room vua tao.
6. Booking va thanh toan.
7. Login owner kiem booking, dashboard, balance, notifications.

### 4. Admin/owner permission test

Can uu tien neu co loi demo:

- Customer khong thay System status.
- HotelOwner chi thay/sua hotel cua minh.
- HotelOwner khong thay hotel cua owner khac.
- Admin thay toan bo users/hotels/bookings.

## Neu gap loi

Backend service loi:

```bash
sudo systemctl status omnistay-api --no-pager
sudo journalctl -u omnistay-api --since "10 minutes ago" --no-pager
```

API public loi qua CloudFront:

- Kiem ALB health.
- Kiem CloudFront behavior `/api/*` tro ve ALB.
- Kiem CloudFront invalidation `/*` da Completed.

Frontend hien ban cu:

- Kiem S3 bucket `omnistay-frontend-484504929670` da co object moi.
- Kiem `public/config.js` la `API_BASE: '/api'`.
- Tao lai CloudFront invalidation `/*`.

Schema loi:

- Kiem lai cac cot:

```sql
DESCRIBE Users;
DESCRIBE Hotels;
DESCRIBE RoomTypes;
DESCRIBE Bookings;
DESCRIBE HotelReviews;
DESCRIBE Notifications;
DESCRIBE BalanceTransactions;
```

## Viec nen lam sau khi demo on

- Cap nhat Launch Template de instance moi cung tai backend artifact moi.
- Cap nhat env metadata `/health/aws` cho dung frontend bucket va CloudFront domain.
- Dua RDS password, JWT secret, admin seed password vao SSM SecureString/Secrets Manager.
- Tao/kiem CloudWatch dashboard evidence.
- Chup screenshot CloudFront, S3, ALB target healthy, RDS, Redis, UI flow.
- Cleanup chi phi neu khong can chay demo nua.
