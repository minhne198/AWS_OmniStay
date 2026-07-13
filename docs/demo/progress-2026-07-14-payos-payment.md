# OmniStay payOS payment progress - 2026-07-14

File nay ghi lai tien do them thanh toan that bang payOS. Khong ghi client id, api key, checksum key, access key, JWT secret, RDS password, token hoac thong tin nhay cam vao file nay.

## Tom tat ngan

Da them thanh toan that bang payOS song song voi thanh toan bang so du:

- Giu nguyen thanh toan bang so du noi bo.
- Them nut `Thanh toan truc tiep (payOS)` tren trang chi tiet booking.
- Them nut `payOS` va `So du` tren trang Booking cua toi.
- Them bang `PaymentTransactions` de luu giao dich provider ngoai.
- Them API tao checkout payOS cho booking dang cho thanh toan.
- Them webhook payOS de xac minh chu ky va chi webhook hop le moi mark booking `Paid`.
- Khi payOS thanh cong: khong tru so du khach, nhung cong doanh thu noi bo cho chu khach san de admin doi soat/chuyen tien.
- Khi huy booking da tra bang payOS: tien duoc hoan ve so du khach, khach co the rut tien ve ngan hang.

## Quyet dinh co che tien

Chon co che:

```text
Khach thanh toan vao tai khoan admin/nen tang qua payOS.
Sau khi booking hoan tat/doi soat, admin chuyen tien cho chu khach san.
```

Ly do:

- Don gian hon cho demo va MVP.
- Chi can mot merchant payOS cua admin.
- De kiem soat refund, tranh chap, doi soat va webhook.
- Sau nay neu can marketplace that co the them onboarding chu khach san/merchant rieng.

Khong chon khach tra thang cho chu khach san o giai doan nay vi se phai quan ly nhieu tai khoan nhan tien, onboard tung owner, refund phuc tap va kho kiem soat trang thai booking.

## Backend da them

Models:

```text
backend/src/HotelBooking.Api/Models/PaymentTransaction.cs
backend/src/HotelBooking.Api/Models/PaymentProviders.cs
backend/src/HotelBooking.Api/Models/PaymentTransactionStatuses.cs
```

Cap nhat:

```text
backend/src/HotelBooking.Api/Models/Booking.cs
backend/src/HotelBooking.Api/Models/PaymentStatuses.cs
backend/src/HotelBooking.Api/Data/HotelBookingDbContext.cs
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

Services:

```text
backend/src/HotelBooking.Api/Services/PayOsOptions.cs
backend/src/HotelBooking.Api/Services/PayOsPaymentClient.cs
backend/src/HotelBooking.Api/Services/EfCoreHotelBookingService.cs
backend/src/HotelBooking.Api/Services/CachedHotelBookingService.cs
backend/src/HotelBooking.Api/Services/IHotelBookingService.cs
```

Controllers/contracts:

```text
backend/src/HotelBooking.Api/Contracts/PaymentContracts.cs
backend/src/HotelBooking.Api/Controllers/BookingsController.cs
backend/src/HotelBooking.Api/Controllers/PayOsWebhookController.cs
```

Config:

```json
"PayOS": {
  "BaseUrl": "https://api-merchant.payos.vn",
  "ClientId": "",
  "ApiKey": "",
  "ChecksumKey": "",
  "PartnerCode": ""
}
```

Tren AWS/Production phai set bang environment variables/SSM, khong commit secret:

```text
PayOS__BaseUrl=https://api-merchant.payos.vn
PayOS__ClientId=<from payOS>
PayOS__ApiKey=<from payOS>
PayOS__ChecksumKey=<from payOS>
PayOS__PartnerCode=<optional>
```

## API moi

Tao link payOS:

```text
POST /api/bookings/{bookingCode}/payments/payos
Authorization: Bearer <JWT>
Body:
{
  "returnUrl": "https://.../public/booking-confirmation.html?code=...&payment=payos-return",
  "cancelUrl": "https://.../public/booking-confirmation.html?code=...&payment=payos-cancel"
}
```

Webhook payOS:

```text
POST /api/payments/payos/webhook
```

Webhook se:

- Kiem `signature` bang `PayOS__ChecksumKey`.
- Tim `PaymentTransaction` theo `orderCode`.
- Kiem amount khop booking.
- Chi mark booking `Paid` khi provider code la `00`.
- Idempotent neu webhook gui lai.

## Frontend da them

Files:

```text
frontend/src/api.js
frontend/src/booking-confirmation.js
frontend/src/booking-lookup.js
frontend/src/session.js
frontend/public/booking-confirmation.html
frontend/public/booking-lookup.html
frontend/public/index.html
```

Cache buster moi cho 2 trang booking:

```text
20260714-payos
```

UI hien 2 cach thanh toan:

```text
Thanh toan truc tiep (payOS)
Thanh toan bang so du
```

## Database production can cap nhat

Neu RDS da ton tai, can chay SQL tao bang:

```sql
CREATE TABLE IF NOT EXISTS PaymentTransactions (
    Id INT NOT NULL AUTO_INCREMENT,
    BookingId INT NOT NULL,
    Provider VARCHAR(30) NOT NULL,
    OrderCode BIGINT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Currency VARCHAR(10) NOT NULL,
    Status VARCHAR(30) NOT NULL,
    Description VARCHAR(100) NOT NULL,
    PaymentLinkId VARCHAR(100) NOT NULL,
    CheckoutUrl VARCHAR(1000) NOT NULL,
    QrCode VARCHAR(1000) NOT NULL,
    ProviderReference VARCHAR(100) NOT NULL,
    FailureReason VARCHAR(1000) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    PaidAt DATETIME(6) NULL,
    CancelledAt DATETIME(6) NULL,
    CONSTRAINT PK_PaymentTransactions PRIMARY KEY (Id),
    CONSTRAINT UX_PaymentTransactions_OrderCode UNIQUE (OrderCode),
    CONSTRAINT FK_PaymentTransactions_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES Bookings (Id) ON DELETE CASCADE
);

CREATE INDEX IX_PaymentTransactions_BookingId_Provider_Status ON PaymentTransactions (BookingId, Provider, Status);
```

## Verification da chay

Backend build:

```text
dotnet build backend/src/HotelBooking.Api/HotelBooking.Api.csproj -c Release
Result: OK
```

Backend tests:

```text
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj -c Release
Result: Passed - 23/23
```

Frontend syntax:

```text
node --check frontend/src/api.js
node --check frontend/src/booking-confirmation.js
node --check frontend/src/booking-lookup.js
node --check frontend/src/session.js
Result: OK
```

## Artifacts moi da tao

Backend:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260714-payos.zip
```

Frontend:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-payos
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-payos.zip
```

Frontend folder dung de upload len S3 la:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-payos
```

Upload cac folder con vao root bucket:

```text
public/
src/
assets/
```

## Viec can lam tiep truoc khi demo that

1. Tao/xac thuc tai khoan payOS cho admin/nen tang.
2. Lay `ClientId`, `ApiKey`, `ChecksumKey` tu kenh thanh toan payOS.
3. Set env vars tren EC2/service `omnistay-api`.
4. Chay SQL tao/cap nhat `PaymentTransactions`, `WithdrawalRequests` va cot ngan hang tren RDS production.
5. Deploy backend artifact moi.
6. Upload frontend moi len S3 va invalidate CloudFront.
7. Cau hinh webhook URL tren payOS:

```text
https://d1gm8xt8e0mar4.cloudfront.net/api/payments/payos/webhook
```

8. Test booking moi:

```text
Thanh toan bang so du -> khach bi tru so du, owner duoc cong doanh thu.
Thanh toan payOS -> redirect sang payOS, webhook ve thi booking Paid, khach khong bi tru so du, owner duoc cong doanh thu noi bo.
Huy booking payOS da paid -> tien hoan ve so du khach va PaymentStatus = Refunded.
```

## Ghi chu rui ro

- Chua them API hoan tien payOS tu dong. Refund tien that can admin xu ly thu cong truoc.
- Uploaded images van dang luu local EC2 neu chua chuyen sang S3.
- Can bat log/sampled request neu CloudFront/WAF chan webhook payOS.

## Hotfix payOS redirect - 2026-07-14 02:23

Loi da thay khi bam huy hoac thanh toan xong tren man hinh QR payOS:

```text
booking-confirmation.html?code=00&payment=payos-cancel&...
UI hien: Booking was not found.
```

Nguyen nhan: payOS redirect ve co query param rieng `code=00`, trung ten voi query param `code=BK...` cua OmniStay. Frontend doc `code=00` thanh ma booking nen goi `/api/bookings/00`.

Da sua:

- Frontend return/cancel URL doi sang `bookingCode=BK...` thay vi `code=BK...`.
- Trang confirmation van doc duoc link cu `code=BK...`.
- Neu payOS redirect khong co `bookingCode` nhung co `orderCode`, frontend fallback goi API moi de tim booking theo payOS order code.
- Backend them endpoint:

```text
GET /api/bookings/payments/payos/{orderCode}
```

Artifacts hotfix:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260714-payos-returnfix.zip
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-payos-returnfix
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-payos-returnfix.zip
```

Frontend cache buster moi:

```text
20260714-payos-returnfix
```

## Wallet, bank account, top-up and withdrawal - 2026-07-14 02:46

Da them tinh nang vi cho tat ca tai khoan: Admin, chu khach san va khach.

Chuc nang moi:

- Trang ca nhan co phan tai khoan ngan hang lien ket: ten ngan hang, so tai khoan, chu tai khoan.
- Nap tien bang payOS: nguoi dung nhap so tien, he thong tao link/QR payOS, webhook thanh cong thi cong tien vao so du.
- Rut tien: nguoi dung nhap so tien can rut, yeu cau vao trang admin o trang thai `Pending`.
- Admin tu chuyen khoan thu cong vao tai khoan ngan hang cua nguoi dung, xong bam xac nhan trong admin.
- Khi admin xac nhan, so du nguoi dung moi bi tru. Neu admin tu choi thi so du khong bi tru.
- Hoan tien booking paid, gom ca payOS, se cong ve so du khach de khach co the rut tien.

Backend them/cap nhat:

```text
backend/src/HotelBooking.Api/Models/WithdrawalRequest.cs
backend/src/HotelBooking.Api/Models/WithdrawalRequestStatuses.cs
backend/src/HotelBooking.Api/Models/PaymentTransactionPurposes.cs
backend/src/HotelBooking.Api/Models/User.cs
backend/src/HotelBooking.Api/Models/PaymentTransaction.cs
backend/src/HotelBooking.Api/Contracts/AuthContracts.cs
backend/src/HotelBooking.Api/Contracts/PaymentContracts.cs
backend/src/HotelBooking.Api/Controllers/AuthController.cs
backend/src/HotelBooking.Api/Controllers/AccountController.cs
backend/src/HotelBooking.Api/Controllers/AdminController.cs
backend/src/HotelBooking.Api/Services/EfCoreHotelBookingService.cs
backend/src/HotelBooking.Api/Data/HotelBookingDbContext.cs
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

API moi:

```text
POST /api/auth/me/bank-account
POST /api/account/balance/top-up/payos
GET  /api/account/withdrawals/my
POST /api/account/withdrawals
GET  /api/admin/withdrawals
POST /api/admin/withdrawals/{id}/complete
POST /api/admin/withdrawals/{id}/reject
```

Frontend them/cap nhat:

```text
frontend/public/profile.html
frontend/public/admin.html
frontend/src/profile.js
frontend/src/admin.js
frontend/src/api.js
```

Cache buster moi:

```text
20260714-wallet
```

SQL incremental cho production neu RDS da co ban payOS truoc do:

```sql
ALTER TABLE Users
    ADD COLUMN BankName VARCHAR(100) NOT NULL DEFAULT '',
    ADD COLUMN BankAccountNumber VARCHAR(50) NOT NULL DEFAULT '',
    ADD COLUMN BankAccountHolder VARCHAR(200) NOT NULL DEFAULT '';

ALTER TABLE PaymentTransactions
    DROP FOREIGN KEY FK_PaymentTransactions_Bookings_BookingId;

ALTER TABLE PaymentTransactions
    MODIFY COLUMN BookingId INT NULL,
    ADD COLUMN UserId INT NULL AFTER BookingId,
    ADD COLUMN Purpose VARCHAR(30) NOT NULL DEFAULT 'BookingPayment' AFTER Provider;

ALTER TABLE PaymentTransactions
    ADD CONSTRAINT FK_PaymentTransactions_Bookings_BookingId
        FOREIGN KEY (BookingId) REFERENCES Bookings (Id) ON DELETE SET NULL,
    ADD CONSTRAINT FK_PaymentTransactions_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE SET NULL;

CREATE INDEX IX_PaymentTransactions_UserId_Provider_Purpose_Status
    ON PaymentTransactions (UserId, Provider, Purpose, Status);

CREATE TABLE IF NOT EXISTS WithdrawalRequests (
    Id INT NOT NULL AUTO_INCREMENT,
    UserId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Status VARCHAR(30) NOT NULL,
    BankName VARCHAR(100) NOT NULL,
    BankAccountNumber VARCHAR(50) NOT NULL,
    BankAccountHolder VARCHAR(200) NOT NULL,
    Note VARCHAR(1000) NOT NULL,
    AdminNote VARCHAR(1000) NOT NULL,
    RequestedAt DATETIME(6) NOT NULL,
    CompletedAt DATETIME(6) NULL,
    CompletedByAdminId INT NULL,
    CONSTRAINT PK_WithdrawalRequests PRIMARY KEY (Id),
    CONSTRAINT FK_WithdrawalRequests_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_WithdrawalRequests_Users_CompletedByAdminId FOREIGN KEY (CompletedByAdminId) REFERENCES Users (Id) ON DELETE SET NULL
);

CREATE INDEX IX_WithdrawalRequests_Status_RequestedAt ON WithdrawalRequests (Status, RequestedAt);
CREATE INDEX IX_WithdrawalRequests_UserId_RequestedAt ON WithdrawalRequests (UserId, RequestedAt);
```

Neu production chua tung tao `PaymentTransactions`, dung schema moi trong:

```text
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

Verification moi:

```text
node --check frontend/src/api.js
node --check frontend/src/profile.js
node --check frontend/src/admin.js
Result: OK

dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj
Result: Passed - 24/24

dotnet publish backend/src/HotelBooking.Api/HotelBooking.Api.csproj -c Release
Result: OK
```

Artifacts moi:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260714-wallet.zip
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet.zip
```

## Hotfix room minimum price - 2026-07-14 02:55

Sua form tao/sua phong trong admin de nhap duoc gia `2.000 VND`:

- Frontend `roomPrice` doi `min=2000`, `step=1000` thay vi `min=0`, `step=10000`.
- Backend `UpsertRoomTypeRequest.PricePerNight` doi validation toi thieu tu `0` sang `2000`.
- Cache buster admin moi: `20260714-wallet-price2000`.

Verification:

```text
node --check frontend/src/admin.js
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj
Result: Passed - 24/24
```

Artifacts:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260714-wallet-price2000.zip
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet-price2000
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet-price2000.zip
```
