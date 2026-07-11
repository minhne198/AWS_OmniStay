# OmniStay progress - local feature QA - 2026-07-11

File này ghi lại tiến độ hiện tại của dự án sau đợt bổ sung tính năng lớn và sửa lỗi local. Không ghi password thật, JWT secret, access key, database password hoặc token vào file này.

## Tóm tắt ngắn

OmniStay hiện đã vượt mức MVP ban đầu. Bản local đã có đủ 3 vai trò chính:

- `Customer`: tìm phòng, đặt phòng, thanh toán, hủy phòng, đánh giá, xem thông báo, trang cá nhân và số dư.
- `HotelOwner`: tạo/quản lý khách sạn và phòng của chính mình, xem booking của khách, nhận tiền booking, dashboard, thông báo và trang chủ khách sạn.
- `Admin`: quản lý user, khách sạn, phòng, booking, số dư, giao dịch, dashboard, lịch sử hoạt động và system status.

Trạng thái mới nhất đã kiểm tra local:

```text
Frontend local: http://127.0.0.1:5173/public/index.html
Backend local:  http://127.0.0.1:5010/api
Health:         http://127.0.0.1:5010/health
```

Các kiểm tra đã chạy gần nhất:

```text
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj --no-restore --no-build
Kết quả: Passed 20/20

node --check frontend/src/*.js
Kết quả: không lỗi cú pháp

Smoke test API local:
- Tạo hotel owner mới
- Tạo customer mới
- Owner tạo khách sạn/phòng
- Customer đặt phòng và thanh toán
- Customer/owner nhận thông báo có dấu tiếng Việt đúng
```

## Những việc đã hoàn thành

### 1. Luồng tài khoản và phân quyền

- Đăng ký tài khoản có chọn vai trò `Customer` hoặc `HotelOwner`.
- Tài khoản mới có sẵn `100.000.000 VND` để test.
- Admin seed vẫn tồn tại cho môi trường demo, nhưng không ghi password vào tài liệu.
- Header/menu tự hiển thị theo vai trò.
- `System status` đã ẩn với `Customer` và `HotelOwner`, chỉ phù hợp cho admin.
- Chủ khách sạn chỉ quản lý khách sạn/phòng thuộc quyền sở hữu của mình.

### 2. Trang cá nhân và số dư

- Có trang cá nhân `profile.html`.
- Người dùng có thể sửa tên, avatar và đổi mật khẩu.
- Có hiển thị số dư.
- Có lịch sử giao dịch số dư của chính người dùng.
- Khi khách thanh toán booking: khách bị trừ số dư.
- Khi booking được thanh toán: chủ khách sạn được cộng doanh thu.
- Khi booking đã thanh toán bị hủy hợp lệ: khách được hoàn tiền, chủ bị trừ lại doanh thu.
- Admin chỉnh số dư hoặc nạp tiền test đều có log giao dịch.

### 3. Khách sạn và phòng

- Admin và hotel owner có thể tạo/sửa khách sạn.
- Hotel owner chỉ thấy và sửa khách sạn/phòng của mình.
- Có chọn thành phố theo danh sách tỉnh/thành có dấu.
- Có nhập ảnh bằng URL và upload file ảnh.
- Có tạo/sửa loại phòng.
- Phòng có tổng số phòng, số đã đặt, số còn trống.
- Có tính năng ẩn phòng.
- Tìm phòng lấy dữ liệu thật từ backend/database.
- Có tìm phòng theo tên khách sạn.

### 4. Booking và thanh toán

- Khách có thể tìm phòng theo thành phố, ngày nhận/trả, số khách và tên khách sạn.
- Có trang chi tiết khách sạn.
- Có trang đặt phòng.
- Có trang xác nhận booking.
- Nút thanh toán đã bỏ chữ `giả lập`, chỉ còn `Thanh toán`.
- Có trang `Booking của tôi`.
- Có hủy booking.
- Trước khi hủy booking, frontend hỏi xác nhận để tránh bấm nhầm.
- Backend chỉ cho hủy khi còn hơn 24 giờ trước ngày nhận phòng.
- Hủy booking đã thanh toán sẽ hoàn tiền cho khách, trừ lại tiền chủ khách sạn và chuyển booking sang trạng thái đã hủy.

### 5. Review và đánh giá

- Khách đã có booking thanh toán thành công có thể đánh giá khách sạn.
- Mỗi booking chỉ đánh giá được một lần.
- Có chọn số sao và nhập nhận xét.
- Trang tìm phòng có dữ liệu điểm đánh giá.
- Có sort/filter theo điểm đánh giá.
- Trang chi tiết khách sạn hiển thị review.

### 6. Thông báo

- Khách nhận thông báo khi tạo booking, thanh toán, hủy booking.
- Chủ khách sạn nhận thông báo khi có booking mới, booking được thanh toán hoặc bị hủy.
- Admin có lịch sử hoạt động.
- Trang `notifications.html` đã sửa giao diện/chữ tiếng Việt.
- Backend đã sửa các thông báo mới sinh ra có dấu.
- Frontend có lớp fallback để hiển thị các thông báo cũ không dấu thành có dấu.

### 7. Dashboard thống kê

- Admin có dashboard tổng quan.
- Hotel owner có dashboard theo phạm vi khách sạn của mình.
- Có doanh thu theo ngày/tháng.
- Có số booking.
- Có phòng được đặt nhiều nhất.
- Có tỷ lệ phòng trống/đã đặt.

### 8. Đồng bộ giao diện và tiếng Việt

- Các trang chính đã được đồng bộ lại theo header/nav mới:
  - `index.html`
  - `register.html`
  - `home.html`
  - `search.html`
  - `hotel-detail.html`
  - `booking.html`
  - `booking-confirmation.html`
  - `booking-lookup.html`
  - `profile.html`
  - `notifications.html`
  - `owner-profile.html`
  - `admin.html`
  - `system-status.html`
- Đã sửa lỗi mojibake/chữ tiếng Việt bị hỏng trên frontend.
- Đã rà các file frontend/backend chính bằng UTF-8 check.

### 9. Local dev server

- Có server frontend no-cache để tránh trình duyệt giữ file cũ:

```text
tools/no_cache_static_server.py
```

- Frontend local đang trỏ API về:

```text
frontend/public/config.js
API_BASE = http://localhost:5010/api
```

## Những việc còn chưa xong

### 1. Chưa deploy các thay đổi mới nhất lên AWS

Các tính năng mới ngày 2026-07-11 hiện đã test local, nhưng chưa được xác nhận đã deploy lại lên CloudFront/AWS.

Cần deploy lại:

- Backend API.
- Frontend static files.
- MySQL schema/data update.
- CloudFront invalidation.

Lưu ý quan trọng: backend đã thêm bảng/cột mới, nên RDS MySQL production phải được cập nhật schema trước hoặc cùng lúc với API mới:

- `Users.AvatarUrl`
- `Users.Balance`
- `Hotels.OwnerUserId`
- `RoomTypes.IsHidden`
- `Bookings.UserId`
- `Bookings.PaymentStatus`
- `Bookings.PaidAt`
- `HotelReviews`
- `Notifications`
- `BalanceTransactions`

### 2. Chưa QA thủ công toàn bộ UI theo 3 vai trò

Đã smoke test API, nhưng anh vẫn cần test bằng trình duyệt theo từng vai trò:

- Customer.
- HotelOwner.
- Admin.

Ưu tiên test trên trình duyệt vì lỗi UI thường không lộ hết qua API test.

### 3. Upload ảnh hiện lưu ở local filesystem của API

Endpoint upload ảnh hiện lưu vào:

```text
backend/src/HotelBooking.Api/uploads/images
```

Điều này ổn để test local. Khi deploy AWS nghiêm túc, nên chuyển upload ảnh sang S3 hoặc ít nhất mount storage bền vững, vì EC2/ASG có thể thay instance và mất file upload cục bộ.

### 4. Cần kiểm tra kỹ phân quyền owner

Code đã giới hạn owner chỉ quản lý hotel/room/booking thuộc khách sạn của mình. Tuy vậy vẫn nên test thủ công các case:

- Owner A không thấy hotel của Owner B.
- Owner A không sửa được phòng của Owner B.
- Owner A không xem được booking của Owner B.
- Admin vẫn xem/sửa được toàn bộ.

### 5. Cần kiểm tra responsive/mobile

Các trang đã đồng bộ giao diện desktop tương đối ổn. Cần mở thêm mobile width để xem:

- Header/nav có bị tràn không.
- Card booking/hotel có vỡ layout không.
- Admin dashboard có đọc được không.
- Form tạo khách sạn/phòng có dễ dùng không.

### 6. Cần dọn và chuẩn hóa dữ liệu test

Trong quá trình test local có nhiều user/hotel/booking sinh ra. Trước khi quay video/demo, nên tạo lại dữ liệu sạch:

- 1 admin.
- 1 customer.
- 1 hotel owner.
- 2-3 khách sạn thật đẹp.
- Mỗi khách sạn 2-3 loại phòng.
- Một số booking ở các trạng thái khác nhau.
- Một số review có nội dung tự nhiên.

## Bước tiếp theo anh nên làm

### Bước 1 - Test local theo checklist

Mở:

```text
http://127.0.0.1:5173/public/index.html
```

Test theo thứ tự này:

1. Đăng ký customer mới.
2. Đăng ký hotel owner mới.
3. Login hotel owner.
4. Tạo khách sạn.
5. Tạo phòng.
6. Ẩn/mở lại phòng.
7. Logout owner.
8. Login customer.
9. Tìm phòng theo thành phố.
10. Tìm phòng theo tên khách sạn.
11. Lọc/sort theo điểm đánh giá.
12. Vào chi tiết khách sạn.
13. Đặt phòng.
14. Thanh toán.
15. Xem số dư bị trừ.
16. Xem thông báo customer.
17. Vào booking của tôi.
18. Thử hủy booking và kiểm tra hộp xác nhận.
19. Nếu hủy thật, kiểm tra hoàn tiền.
20. Login owner kiểm tra tiền được cộng/trừ, thông báo và booking mới.
21. Login admin kiểm tra dashboard, giao dịch, hoạt động và chỉnh số dư.

Nếu gặp lỗi, ghi lại:

```text
Trang:
Vai trò:
Hành động:
Kết quả mong muốn:
Lỗi thực tế:
Ảnh chụp:
Console/Network error nếu có:
```

### Bước 2 - Gom lỗi UI còn lại và sửa một lượt

Sau khi test hết checklist, gom lỗi theo nhóm:

- Lỗi chữ/giao diện.
- Lỗi phân quyền.
- Lỗi booking/thanh toán/số dư.
- Lỗi thông báo.
- Lỗi dashboard/admin.
- Lỗi upload ảnh.

Sửa theo nhóm sẽ nhanh và ít làm vỡ các phần đã ổn hơn.

### Bước 3 - Chạy lại test tự động

Sau khi sửa lỗi:

```text
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj --no-restore
node --check frontend/src/*.js
```

Nên thêm test backend cho các case rủi ro:

- Owner không sửa được hotel của owner khác.
- Hủy booking trước/sau mốc 24 giờ.
- Hoàn tiền/trừ tiền chủ khi hủy booking đã thanh toán.
- Mỗi booking chỉ review một lần.
- Admin chỉnh số dư có ghi transaction log.

### Bước 4 - Chuẩn bị deploy AWS

Trước khi deploy:

- Backup hoặc snapshot RDS nếu cần.
- Cập nhật schema MySQL production.
- Build/publish backend artifact mới.
- Upload artifact API.
- Deploy backend lên EC2/ASG.
- Upload frontend mới lên S3.
- Tạo CloudFront invalidation `/*`.

### Bước 5 - Test lại trên CloudFront

Sau deploy, test lại public URL:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/index.html
```

Checklist CloudFront:

- Login/register.
- Search.
- Booking.
- Payment.
- Cancel/refund.
- Notifications.
- Profile.
- Admin.
- Owner dashboard.
- Upload ảnh.
- System status chỉ admin thấy.

### Bước 6 - Làm evidence demo

Chụp ảnh/quay video:

- UI customer flow.
- UI owner flow.
- UI admin flow.
- CloudFront distribution.
- S3 bucket frontend.
- ALB target healthy.
- EC2/ASG.
- RDS.
- Redis/Valkey.
- CloudWatch dashboard.
- Redis cache HIT/MISS nếu cần bảo vệ phần caching.

## Lệnh local hữu ích

Chạy backend:

```bash
dotnet run --project backend/src/HotelBooking.Api/HotelBooking.Api.csproj --urls http://127.0.0.1:5010
```

Chạy frontend no-cache:

```bash
python tools/no_cache_static_server.py
```

Mở app:

```text
http://127.0.0.1:5173/public/index.html
```

Test backend:

```bash
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj --no-restore
```

Kiểm tra JS:

```bash
node --check frontend/src/api.js
node --check frontend/src/session.js
node --check frontend/src/admin.js
node --check frontend/src/booking-confirmation.js
node --check frontend/src/booking-lookup.js
node --check frontend/src/notifications.js
```

## Ghi chú cho người tiếp tục dự án

Nếu chỉ có ít thời gian, đừng deploy AWS ngay. Hãy test local hết checklist trước. Khi local đã ổn, mới deploy một lần lên AWS để tránh mất thời gian upload/invalidation nhiều lần.

Ưu tiên sửa các lỗi ảnh hưởng demo:

1. Booking/thanh toán/số dư.
2. Phân quyền owner/admin/customer.
3. Giao diện và tiếng Việt.
4. Thông báo.
5. Dashboard/evidence.
