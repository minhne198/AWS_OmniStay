# OmniStay feature inventory

Updated: 2026-07-14

This file summarizes the current website features after the payOS, wallet, withdrawal, refund-to-balance, and room minimum price updates.

## User roles

OmniStay has three roles:

- `Customer`: searches, books, pays, reviews, manages wallet balance, and requests withdrawals.
- `HotelOwner`: manages owned hotels/rooms, sees owned bookings/revenue, manages wallet balance, and requests withdrawals.
- `Admin`: manages all users, hotels, bookings, balances, withdrawals, activity, and system status.

## Public and authentication

### Login

- Page: `frontend/public/index.html`.
- Email/password login.
- JWT session stored in localStorage.
- Role-aware navigation after login.
- No demo credentials auto-fill on the login form.

### Register

- Page: `frontend/public/register.html`.
- Users enter full name, email, and password.
- Users can register as:
  - Customer.
  - Hotel owner.
- New accounts receive an initial demo balance of `100,000,000 VND`.
- Self-registering as Admin is blocked.

### Shared navigation

- Shared top nav across main pages.
- Shows user name and role.
- Logout button.
- Profile link.
- Notification link with unread count.
- Hotel owner sees owner/profile management links.
- Admin sees admin/system links.
- Customer and hotel owner do not see system status.

## Customer features

### Search rooms

- Page: `frontend/public/home.html`.
- Search by city.
- Select check-in and check-out dates.
- Enter guest count.
- Search by hotel or room keyword.
- Availability is calculated from real bookings in the backend.
- Hidden rooms are excluded from public search.

### Search results

- Page: `frontend/public/search.html`.
- Shows matching hotel/room cards.
- Displays image, hotel name, city, room type, price, max guests, available rooms, rating, and review count.
- Supports price/rating sorting.
- Links to hotel detail.

### Hotel detail

- Page: `frontend/public/hotel-detail.html`.
- Shows hotel information.
- Lists room types under the hotel.
- Shows available/booked status.
- Shows hotel reviews.
- Allows the customer to continue to booking.

### Booking creation

- Page: `frontend/public/booking.html`.
- Shows booking summary: hotel, room, dates, nights, guests, and total price.
- Customer enters guest information.
- Creates a real booking through the API.
- New bookings start as pending payment.

### Booking confirmation

- Page: `frontend/public/booking-confirmation.html`.
- Shows booking code, hotel, room, dates, nights, guests, total price, booking status, and payment status.
- Supports balance payment.
- Supports payOS direct payment.
- Supports canceling bookings when allowed.
- Shows review form after eligible paid bookings.
- Handles payOS return/cancel redirects without confusing payOS `code=00` with OmniStay booking codes.
- Can recover booking confirmation from payOS `orderCode`.

### My bookings

- Page: `frontend/public/booking-lookup.html`.
- Shows current user's bookings.
- Supports lookup by booking code.
- Supports paying pending bookings by:
  - Balance.
  - payOS.
- Supports canceling bookings when allowed.

### Payments

- Balance payment remains available.
- payOS direct payment is available for pending bookings.
- payOS payments go to the platform/admin merchant account.
- payOS webhook verifies checksum before marking transactions as paid.
- Booking payOS success:
  - Booking becomes paid.
  - Customer balance is not deducted.
  - Hotel owner receives internal balance credit for payout tracking.
  - Balance/payment transactions are logged.

### Cancellation and refund

- Bookings can be canceled only when the cancellation rule allows it.
- Unpaid booking cancellation marks the booking canceled.
- Paid booking cancellation:
  - Refunds customer to OmniStay balance.
  - Reverses hotel owner booking credit when applicable.
  - Marks payment status as refunded.
  - Writes balance transaction logs.
  - Sends notifications.
- Refunds for payOS-paid bookings also go to customer balance, so customers can withdraw.

### Reviews

- Customers can review hotels after eligible paid bookings.
- Review includes rating and comment.
- One review per booking.
- Reviews affect search/detail display data.

### Profile and wallet

- Page: `frontend/public/profile.html`.
- Shows account information.
- Allows full name update.
- Allows avatar update by URL/upload flow already used by the app.
- Allows password change.
- Shows current balance.
- Shows personal balance transaction history.
- Stores linked bank account information:
  - Bank name.
  - Bank account number.
  - Bank account holder.
- Supports payOS top-up:
  - User enters amount.
  - App creates payOS checkout/QR.
  - Verified webhook credits user balance.
- Supports withdrawal request:
  - User enters amount and optional note.
  - Request starts as `Pending`.
  - Admin receives notification.
  - Balance is deducted only after admin confirms manual transfer.
- Customers, hotel owners, and admins all have wallet/bank fields.

### Notifications

- Page: `frontend/public/notifications.html`.
- Shows notifications.
- Mark one notification as read.
- Mark all notifications as read.
- Customers receive notifications for booking, payment, cancellation, refund, top-up, and withdrawal events.

## Hotel owner features

### Owner registration and scope

- Users can register as hotel owners.
- Hotel owners can create hotels.
- Hotel owners only manage data for hotels they own.
- Owner scope is enforced by `Hotels.OwnerUserId`.

### Manage hotels

- Page: `frontend/public/admin.html` in owner scope.
- Create hotels.
- Edit owned hotels.
- Select city from the app city list.
- Use image URL or image upload.
- View owned hotels.

### Manage rooms

- Create room types for owned hotels.
- Edit room types.
- Fields include name, description, max guests, price per night, total rooms, image, and hidden status.
- Minimum room price is `2,000 VND`.
- The admin/owner room price input supports entering `2000`.
- View booked rooms and available rooms.
- Hide/show rooms.
- Hidden rooms do not appear in customer search.

### Owner bookings

- View bookings for owned hotels.
- See customer, hotel, room, stay dates, total price, booking status, and payment status.
- Owner cannot see unrelated hotels/bookings.

### Owner dashboard

- Revenue summary in owner scope.
- Booking count.
- Most booked room.
- Total rooms, booked rooms, and occupancy.
- Revenue by day/month for owned hotels.

### Owner profile

- Page: `frontend/public/owner-profile.html`.
- Shows owner profile.
- Shows owned hotels.
- Shows total revenue.
- Shows verification status.

### Owner wallet

- Hotel owners receive internal balance credits from booking payments.
- Owner balance is reversed when paid bookings are canceled/refunded.
- Owners can add bank information.
- Owners can request withdrawals.
- Admin manually transfers outside the app, then confirms in OmniStay to deduct the owner balance.

## Admin features

### User management

- Page: `frontend/public/admin.html`.
- View users.
- Create users.
- Edit users.
- Delete users.
- Assign roles.
- Edit avatar.
- Set/update balance.
- Create test top-ups.
- Balance edits are logged as balance transactions and admin activity.

### Hotel and room management

- Admin sees all hotels.
- Create/edit hotels.
- Create/edit rooms.
- Upload images.
- Hide/show rooms.
- Assign/view owners through backend data.
- Room minimum price is `2,000 VND`.

### Booking management

- Admin sees all bookings.
- Shows guest, hotel, room, stay dates, total price, booking status, and payment status.
- Admin has broader management visibility than customer/owner.

### Admin dashboard

- Total revenue.
- Booking count.
- Most booked room.
- Total rooms.
- Booked rooms.
- Occupancy rate.
- Revenue by day.
- Revenue by month.

### Balance transactions

- Admin can view all balance transactions.
- Transaction types include:
  - Initial balance grant.
  - Admin balance set/adjustment.
  - Test top-up.
  - Booking payment.
  - Owner booking credit.
  - Booking refund.
  - Owner booking reversal.
  - payOS top-up.
  - Withdrawal completed.

### Withdrawal management

- Admin has a `Rut tien` tab in `frontend/public/admin.html`.
- Admin sees all withdrawal requests.
- Pending withdrawal shows user email, amount, bank name, account number, account holder, note, and requested time.
- Admin manually transfers money outside OmniStay.
- Admin then clicks confirm:
  - Withdrawal becomes completed.
  - User balance is deducted.
  - Balance transaction is written.
  - User receives notification.
- Admin can reject a pending withdrawal:
  - User balance is not deducted.
  - User receives notification.

### Activity log

- Admin can view activity log.
- Activity is based on notification/activity records.
- Main activity types include:
  - Initial balance.
  - Balance edits.
  - Test top-up.
  - Booking created.
  - Payment.
  - Cancellation.
  - Review.
  - payOS top-up.
  - Withdrawal requested/completed/rejected.

### System status

- Page: `frontend/public/system-status.html`.
- Only Admin sees the link.
- Shows backend health/AWS runtime status.
- Used to inspect API, database, Redis, and AWS configuration.

## Backend/API features

### Auth APIs

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `PUT /api/auth/me`
- `POST /api/auth/me`
- `PUT /api/auth/me/password`
- `POST /api/auth/me/bank-account`
- `PUT /api/auth/me/bank-account`

### Hotel and review APIs

- `GET /api/hotels/search`
- `GET /api/hotels/{hotelId}`
- `GET /api/hotels/{hotelId}/rooms`
- `GET /api/hotels/{hotelId}/reviews`
- `POST /api/hotels/{hotelId}/reviews`

### Booking APIs

- `POST /api/bookings`
- `GET /api/bookings/{code}`
- `GET /api/bookings/my`
- `POST /api/bookings/{code}/pay`
- `DELETE /api/bookings/{code}`
- `POST /api/bookings/{bookingCode}/payments/payos`
- `GET /api/bookings/payments/payos/{orderCode}`

### payOS APIs

- `POST /api/payments/payos/webhook`
- Webhook signature is verified with `PayOS__ChecksumKey`.
- Booking payment and wallet top-up are separated by payment transaction purpose.

### Account/wallet APIs

- `GET /api/account/balance-transactions/my`
- `POST /api/account/balance/top-up/payos`
- `GET /api/account/withdrawals/my`
- `POST /api/account/withdrawals`
- `GET /api/account/owner-profile`

### Admin APIs

- Users.
- Hotels.
- Room types.
- Bookings.
- Dashboard.
- Balance transactions.
- Activity.
- Image upload.
- Withdrawals:
  - `GET /api/admin/withdrawals`
  - `POST /api/admin/withdrawals/{id}/complete`
  - `POST /api/admin/withdrawals/{id}/reject`

### Notifications APIs

- `GET /api/notifications/my`
- Mark read.
- Mark all read.

### Health APIs

- `/health`
- `/health/aws`

## Data model

Main tables/models:

- `Users`
- `Hotels`
- `RoomTypes`
- `Bookings`
- `HotelReviews`
- `Notifications`
- `BalanceTransactions`
- `PaymentTransactions`
- `WithdrawalRequests`

Important user fields:

- `Balance`
- `AvatarUrl`
- `BankName`
- `BankAccountNumber`
- `BankAccountHolder`

Important payment fields:

- `Provider`
- `Purpose`
- `OrderCode`
- `Status`
- `BookingId`
- `UserId`

## Auth and authorization

- JWT auth.
- Role-based authorization.
- Customer/owner/admin scope.
- Owner data scope is based on `Hotels.OwnerUserId`.
- Admin-only APIs are protected.

## Cache and database

- Local/test development uses EF Core in-memory database.
- Production supports MySQL through EF Core.
- Search cache supports Redis/Valkey.
- MySQL schema is maintained in:

```text
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

## Image upload

- Multipart image upload is available.
- Supported extensions:
  - jpg
  - jpeg
  - png
  - webp
  - gif
- File limit: 5 MB.
- Current storage is local API server storage.
- This is not yet durable enough for production Auto Scaling without shared storage/S3 image handling.

## AWS/demo deployment

### Frontend

- Static frontend files live in:
  - `frontend/public`
  - `frontend/src`
  - `frontend/assets`
- Production frontend is hosted from private S3 behind CloudFront.
- CloudFront default root object points to `public/index.html`.

### Backend

- ASP.NET Core API runs on EC2.
- The service is `omnistay-api`.
- Backend artifact zip is published as `omnistay-api.zip`.
- Health checks use `/health`.

### Database/cache

- RDS MySQL stores persistent data.
- ElastiCache Redis/Valkey can cache search results.

### Current deploy artifacts

Latest local artifacts for the 2026-07-14 wallet/payOS/price2000 build:

```text
artifacts/omnistay-api.zip
artifacts/omnistay-api-20260714-wallet-price2000.zip
artifacts/omnistay-frontend-20260714-wallet-price2000
artifacts/omnistay-frontend-20260714-wallet-price2000.zip
```

## Frontend pages

```text
frontend/public/index.html
frontend/public/register.html
frontend/public/home.html
frontend/public/search.html
frontend/public/hotel-detail.html
frontend/public/booking.html
frontend/public/booking-confirmation.html
frontend/public/booking-lookup.html
frontend/public/profile.html
frontend/public/notifications.html
frontend/public/owner-profile.html
frontend/public/admin.html
frontend/public/system-status.html
```

## Current limitations

- payOS refund is not automated through payOS API yet; cancellation refunds go to OmniStay balance.
- Admin/manual payout is still required for withdrawals and owner settlement.
- Production database must be migrated before running the new API build.
- Local uploaded images are not durable across multiple EC2 instances.
- More mobile/responsive QA is still useful.
- More cross-owner authorization QA is still useful.
