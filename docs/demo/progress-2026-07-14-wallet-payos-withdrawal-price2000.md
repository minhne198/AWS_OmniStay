# OmniStay wallet, payOS, withdrawal, and room price progress - 2026-07-14

This file records the current progress for the real payment, wallet, withdrawal, refund-to-balance, and minimum room price build.

Do not put secrets in this file. Do not store payOS keys, JWT secrets, RDS passwords, AWS keys, or tokens here.

## Summary

Implemented a production-facing payment and wallet flow while keeping the existing balance payment flow:

- Existing balance payment remains available.
- Booking payment can now be made through payOS.
- payOS webhook verifies signature before accepting a payment result.
- payOS return/cancel redirect no longer conflicts with OmniStay booking codes.
- All user roles now have linked bank account fields.
- Users can top up their OmniStay balance through payOS.
- Users can request withdrawals.
- Admin manually transfers money outside the app, then confirms or rejects the withdrawal in OmniStay.
- Booking refunds now return to customer OmniStay balance.
- Room creation/editing now allows a minimum price of `2,000 VND`.

## Money flow decision

Chosen MVP flow:

```text
Customer pays the platform/admin merchant account through payOS.
OmniStay records internal balance movements.
Admin manually settles withdrawals and owner payouts outside the app.
```

Why:

- One payOS merchant account is enough for demo/MVP.
- Easier reconciliation and dispute handling.
- Refunds can be represented in customer balance.
- Owner settlement can be handled manually first.
- Future marketplace onboarding can be added later if each owner needs their own merchant account.

## Backend changes

New files:

```text
backend/src/HotelBooking.Api/Controllers/PayOsWebhookController.cs
backend/src/HotelBooking.Api/Models/PaymentProviders.cs
backend/src/HotelBooking.Api/Models/PaymentTransaction.cs
backend/src/HotelBooking.Api/Models/PaymentTransactionPurposes.cs
backend/src/HotelBooking.Api/Models/PaymentTransactionStatuses.cs
backend/src/HotelBooking.Api/Models/WithdrawalRequest.cs
backend/src/HotelBooking.Api/Models/WithdrawalRequestStatuses.cs
backend/src/HotelBooking.Api/Services/PayOsOptions.cs
backend/src/HotelBooking.Api/Services/PayOsPaymentClient.cs
```

Updated backend areas:

```text
backend/src/HotelBooking.Api/Contracts/AuthContracts.cs
backend/src/HotelBooking.Api/Contracts/AdminContracts.cs
backend/src/HotelBooking.Api/Contracts/PaymentContracts.cs
backend/src/HotelBooking.Api/Controllers/AccountController.cs
backend/src/HotelBooking.Api/Controllers/AdminController.cs
backend/src/HotelBooking.Api/Controllers/AuthController.cs
backend/src/HotelBooking.Api/Controllers/BookingsController.cs
backend/src/HotelBooking.Api/Data/HotelBookingDbContext.cs
backend/src/HotelBooking.Api/Data/schema.mysql.sql
backend/src/HotelBooking.Api/Models/Booking.cs
backend/src/HotelBooking.Api/Models/PaymentStatuses.cs
backend/src/HotelBooking.Api/Models/User.cs
backend/src/HotelBooking.Api/Program.cs
backend/src/HotelBooking.Api/Services/CachedHotelBookingService.cs
backend/src/HotelBooking.Api/Services/EfCoreHotelBookingService.cs
backend/src/HotelBooking.Api/Services/IHotelBookingService.cs
backend/src/HotelBooking.Api/appsettings.json
backend/tests/HotelBooking.Api.Tests/AuthAndAdminTests.cs
```

## Frontend changes

Updated frontend areas:

```text
frontend/public/admin.html
frontend/public/booking-confirmation.html
frontend/public/booking-lookup.html
frontend/public/index.html
frontend/public/profile.html
frontend/src/admin.js
frontend/src/api.js
frontend/src/booking-confirmation.js
frontend/src/booking-lookup.js
frontend/src/profile.js
frontend/src/session.js
```

Current frontend cache buster:

```text
20260714-wallet-price2000
```

## New APIs

Bank account:

```text
POST /api/auth/me/bank-account
PUT  /api/auth/me/bank-account
```

payOS booking payment:

```text
POST /api/bookings/{bookingCode}/payments/payos
GET  /api/bookings/payments/payos/{orderCode}
POST /api/payments/payos/webhook
```

Wallet top-up:

```text
POST /api/account/balance/top-up/payos
```

Withdrawals:

```text
GET  /api/account/withdrawals/my
POST /api/account/withdrawals
GET  /api/admin/withdrawals
POST /api/admin/withdrawals/{id}/complete
POST /api/admin/withdrawals/{id}/reject
```

## Database changes

New/updated model fields:

- `Users.BankName`
- `Users.BankAccountNumber`
- `Users.BankAccountHolder`
- `PaymentTransactions.BookingId` is nullable.
- `PaymentTransactions.UserId`
- `PaymentTransactions.Purpose`
- New table `WithdrawalRequests`.

Production SQL for an existing RDS database that already has the earlier payOS table:

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

If production does not have `PaymentTransactions` yet, use the latest full schema:

```text
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

## Deployment artifacts

Latest backend artifact:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260714-wallet-price2000.zip
```

Latest frontend artifact:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet-price2000
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260714-wallet-price2000.zip
```

Frontend upload target:

```text
S3 bucket: omnistay-frontend-484504929670
Upload folder contents: public/, src/, assets/
```

Backend artifact target:

```text
S3 bucket: omnistay-artifacts-306656769227-new
Object: omnistay-api.zip
```

CloudFront distribution:

```text
E25IVE7NGAHBJP
Invalidate: /*
```

## EC2 backend deploy commands

These commands do not require `cd /opt/omnistay/api` because all paths are absolute:

```bash
aws s3 cp s3://omnistay-artifacts-306656769227-new/omnistay-api.zip /tmp/omnistay-api.zip
sudo systemctl stop omnistay-api
sudo unzip -o /tmp/omnistay-api.zip -d /opt/omnistay/api
sudo systemctl daemon-reload
sudo systemctl start omnistay-api
sudo systemctl status omnistay-api --no-pager
```

If the service is not healthy:

```bash
sudo journalctl -u omnistay-api -n 80 --no-pager
```

## Verification already run locally

Frontend syntax:

```text
node --check frontend/src/api.js
node --check frontend/src/profile.js
node --check frontend/src/admin.js
Result: OK
```

Backend tests:

```text
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj
Result: Passed - 24/24
```

Backend publish:

```text
dotnet publish backend/src/HotelBooking.Api/HotelBooking.Api.csproj -c Release
Result: OK
```

## Manual production QA checklist

After SQL, backend deploy, frontend upload, and CloudFront invalidation:

1. Log in as Admin.
2. Open `public/admin.html`.
3. Create or edit a room with price `2000`.
4. Confirm no browser validation popup appears for price `2000`.
5. Open profile page.
6. Save linked bank account.
7. Create a payOS top-up and confirm QR/link is generated.
8. Complete a top-up test and confirm balance increases after webhook.
9. Create a withdrawal request.
10. Confirm Admin sees the pending withdrawal in the `Rut tien` tab.
11. Confirm Admin completion deducts user balance only after manual confirmation.
12. Create a booking and pay by balance.
13. Create a booking and pay by payOS.
14. Cancel a paid booking and confirm refund goes to customer balance.

## Remaining risks / follow-up

- payOS refund API is not automated yet.
- Withdrawals and owner payouts are intentionally manual in this MVP.
- Local EC2 image upload is not durable enough for a multi-instance production deployment.
- More mobile QA is useful after CloudFront receives the new frontend build.
