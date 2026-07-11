# OmniStay feature inventory

File này liệt kê toàn bộ tính năng web hiện có của OmniStay tính đến ngày 2026-07-11.

## Vai trò người dùng

OmniStay hiện có 3 vai trò:

- `Customer`: khách đặt phòng.
- `HotelOwner`: chủ khách sạn.
- `Admin`: quản trị hệ thống.

## Trang public và xác thực

### Đăng nhập

- Trang đăng nhập `public/index.html`.
- Đăng nhập bằng email/password.
- Lưu session bằng JWT trong localStorage.
- Điều hướng sau login theo vai trò.
- Không còn hiển thị dòng API trên màn hình login.
- Không còn auto-fill email/password mẫu trên form login.

### Đăng ký

- Trang đăng ký `public/register.html`.
- Người dùng nhập họ tên, email, mật khẩu.
- Người dùng chọn vai trò khi đăng ký:
  - Khách hàng.
  - Chủ khách sạn.
- Tài khoản mới được cấp sẵn `100.000.000 VND` để test booking.

### Header/nav theo vai trò

- Header đồng bộ trên các trang chính.
- Hiển thị tên người dùng và vai trò.
- Có nút đăng xuất.
- Có link trang cá nhân ở góc phải.
- Có link thông báo kèm số thông báo chưa đọc.
- Hotel owner có thêm link trang chủ khách sạn.
- Admin có thêm link admin/system status.
- Customer và hotel owner không thấy system status.

## Tính năng Customer

### Tìm phòng

- Trang home `public/home.html`.
- Tìm theo thành phố.
- Chọn ngày nhận phòng/trả phòng.
- Nhập số khách.
- Tìm theo tên khách sạn.
- Dữ liệu phòng lấy từ backend/database.
- Có kiểm tra số phòng còn trống theo khoảng ngày.
- Phòng bị ẩn không hiển thị cho khách.

### Kết quả tìm kiếm

- Trang kết quả `public/search.html`.
- Hiển thị danh sách khách sạn/phòng phù hợp.
- Hiển thị ảnh, tên khách sạn, thành phố, loại phòng, giá, số khách tối đa.
- Hiển thị điểm đánh giá trung bình và số review.
- Có sort/filter theo điểm đánh giá.
- Có điều hướng sang trang chi tiết khách sạn.

### Chi tiết khách sạn

- Trang `public/hotel-detail.html`.
- Hiển thị thông tin khách sạn.
- Hiển thị danh sách phòng thuộc khách sạn.
- Hiển thị trạng thái còn phòng/đã đặt.
- Hiển thị review khách sạn.
- Có nút tiếp tục đặt phòng.

### Đặt phòng

- Trang `public/booking.html`.
- Hiển thị tóm tắt khách sạn/phòng/ngày/số khách/giá.
- Khách nhập thông tin người đặt.
- Tạo booking thật qua API.
- Booking mới ở trạng thái chờ thanh toán.

### Xác nhận booking

- Trang `public/booking-confirmation.html`.
- Hiển thị mã booking.
- Hiển thị khách sạn, phòng, ngày ở, số đêm, số khách, tổng tiền.
- Hiển thị trạng thái booking và thanh toán.
- Có nút `Thanh toán`.
- Có nút `Hủy booking` nếu booking còn được hủy.
- Khi bấm hủy sẽ hỏi xác nhận trước.
- Có form đánh giá sau khi booking đã thanh toán và đủ điều kiện review.

### Booking của tôi

- Trang `public/booking-lookup.html`.
- Khách xem danh sách booking của chính mình.
- Có tra cứu booking theo mã.
- Có thanh toán booking đang pending.
- Có hủy booking nếu còn được hủy.
- Khi bấm hủy sẽ hỏi xác nhận trước.

### Thanh toán và số dư

- Thanh toán bằng luồng demo/mock payment.
- Khi thanh toán thành công:
  - Booking chuyển sang đã xác nhận/đã thanh toán.
  - Số dư customer bị trừ.
  - Số dư hotel owner được cộng.
  - Giao dịch số dư được ghi log.

### Hủy phòng và hoàn tiền

- Chỉ được hủy khi còn hơn 24 giờ trước ngày nhận phòng.
- Nếu booking chưa thanh toán: booking chuyển trạng thái đã hủy.
- Nếu booking đã thanh toán:
  - Hoàn tiền cho customer.
  - Trừ lại doanh thu của hotel owner.
  - Payment status chuyển hoàn tiền.
  - Ghi transaction log.
  - Gửi thông báo cho các bên liên quan.

### Đánh giá khách sạn

- Customer đã thanh toán booking có thể đánh giá khách sạn.
- Đánh giá gồm số sao và nhận xét.
- Mỗi booking chỉ được đánh giá một lần.
- Điểm đánh giá ảnh hưởng dữ liệu hiển thị ở search/hotel detail.

### Trang cá nhân

- Trang `public/profile.html`.
- Xem thông tin tài khoản.
- Sửa họ tên.
- Đổi avatar bằng URL.
- Đổi mật khẩu.
- Xem số dư hiện tại.
- Xem lịch sử giao dịch số dư của chính mình.

### Thông báo

- Trang `public/notifications.html`.
- Xem danh sách thông báo.
- Đánh dấu từng thông báo đã đọc.
- Đánh dấu tất cả đã đọc.
- Customer nhận thông báo khi:
  - Booking được tạo.
  - Booking thanh toán thành công.
  - Booking bị hủy.

## Tính năng Hotel Owner

### Đăng ký/làm chủ khách sạn

- Người dùng có thể đăng ký vai trò chủ khách sạn.
- Hotel owner có quyền tạo khách sạn.
- Hotel owner chỉ quản lý dữ liệu thuộc quyền sở hữu của mình.

### Quản lý khách sạn

- Dùng trang `public/admin.html` ở phạm vi owner.
- Tạo khách sạn mới.
- Sửa khách sạn đã tạo.
- Chọn thành phố từ danh sách tỉnh/thành có dấu.
- Nhập ảnh bằng URL.
- Upload file ảnh.
- Xem danh sách khách sạn của mình.

### Quản lý phòng

- Tạo loại phòng cho khách sạn của mình.
- Sửa loại phòng.
- Nhập tên phòng, mô tả, số khách tối đa, giá mỗi đêm, tổng số phòng, ảnh.
- Xem số phòng đã đặt và số phòng còn trống.
- Ẩn/mở phòng.
- Phòng ẩn không xuất hiện trong kết quả tìm kiếm của khách.

### Quản lý booking của khách

- Xem booking thuộc các khách sạn của mình.
- Thấy khách nào đã booking phòng của mình.
- Thấy trạng thái booking/thanh toán.
- Dữ liệu booking của owner được giới hạn theo `OwnerUserId`.

### Dashboard chủ khách sạn

- Xem doanh thu.
- Xem số booking.
- Xem phòng được đặt nhiều nhất.
- Xem tỷ lệ phòng trống/đã đặt.
- Xem doanh thu theo ngày/tháng trong phạm vi khách sạn của mình.

### Trang chi tiết chủ khách sạn

- Trang `public/owner-profile.html`.
- Hiển thị profile chủ khách sạn.
- Hiển thị danh sách khách sạn đang sở hữu.
- Hiển thị tổng doanh thu.
- Hiển thị trạng thái xác minh.

### Số dư và thông báo của chủ

- Khi khách thanh toán booking, owner được cộng số dư.
- Khi khách hủy booking đã thanh toán, owner bị trừ lại số dư.
- Owner có lịch sử giao dịch số dư.
- Owner nhận thông báo khi:
  - Có booking mới.
  - Booking được thanh toán.
  - Booking bị hủy.
  - Khách đánh giá khách sạn.

## Tính năng Admin

### Quản lý user

- Trang `public/admin.html`.
- Xem danh sách user.
- Tạo user.
- Sửa user.
- Xóa user.
- Gán vai trò.
- Sửa avatar.
- Chỉnh số dư.
- Nạp tiền test.
- Mỗi lần chỉnh/nạp số dư có ghi balance transaction và activity log.

### Quản lý khách sạn/phòng

- Admin thấy toàn bộ khách sạn.
- Tạo/sửa khách sạn.
- Tạo/sửa phòng.
- Upload ảnh.
- Ẩn/mở phòng.
- Gán/nhìn owner của khách sạn qua dữ liệu backend.

### Quản lý booking

- Admin xem toàn bộ booking.
- Xem thông tin khách đặt, khách sạn, phòng, ngày ở, tổng tiền, trạng thái booking/thanh toán.
- Admin có quyền rộng hơn customer/owner trong các luồng backend cần quản trị.

### Dashboard admin

- Tổng doanh thu.
- Tổng số booking.
- Phòng được đặt nhiều nhất.
- Tổng số phòng.
- Số phòng đã đặt.
- Tỷ lệ lấp đầy.
- Doanh thu theo ngày.
- Doanh thu theo tháng.

### Lịch sử giao dịch số dư

- Admin xem danh sách transaction toàn hệ thống.
- Các loại transaction hiện có:
  - Cấp số dư ban đầu.
  - Admin chỉnh số dư.
  - Nạp tiền test.
  - Trừ tiền booking.
  - Cộng doanh thu cho chủ khách sạn.
  - Hoàn tiền booking.
  - Trừ lại doanh thu booking hủy.

### Lịch sử hoạt động

- Admin xem activity log.
- Các hoạt động chính được ghi lại qua notification/activity:
  - Tạo số dư ban đầu.
  - Chỉnh số dư.
  - Nạp tiền test.
  - Booking mới.
  - Thanh toán.
  - Hủy booking.
  - Review mới.

### System status

- Trang `public/system-status.html`.
- Chỉ admin thấy link trong menu.
- Hiển thị trạng thái health/AWS runtime.
- Dùng để kiểm tra backend, database, Redis và config AWS.

## Tính năng backend/API

### API chính

- Auth:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/me`
  - `PUT /api/auth/me`
  - `PUT /api/auth/me/password`
- Hotels:
  - `GET /api/hotels/search`
  - `GET /api/hotels/{hotelId}`
  - `GET /api/hotels/{hotelId}/rooms`
  - `GET /api/hotels/{hotelId}/reviews`
  - `POST /api/hotels/{hotelId}/reviews`
- Bookings:
  - `POST /api/bookings`
  - `GET /api/bookings/{code}`
  - `GET /api/bookings/my`
  - `POST /api/bookings/{code}/pay`
  - `DELETE /api/bookings/{code}`
- Admin:
  - Users.
  - Hotels.
  - Room types.
  - Bookings.
  - Dashboard.
  - Balance transactions.
  - Activity.
  - Image upload.
- Notifications:
  - `GET /api/notifications/my`
  - Mark read.
  - Mark all read.
- Account:
  - My balance transactions.
  - Owner profile.
- Health:
  - `/health`
  - `/health/aws`

### Data model

- `Users`
- `Hotels`
- `RoomTypes`
- `Bookings`
- `HotelReviews`
- `Notifications`
- `BalanceTransactions`

### Auth và phân quyền

- JWT auth.
- Role-based authorization.
- Customer/owner/admin scope.
- Owner scope dựa trên `Hotels.OwnerUserId`.

### Cache và database

- Local development dùng in-memory store.
- Production hỗ trợ MySQL qua EF Core.
- Search cache hỗ trợ Redis/Valkey.
- Có schema MySQL trong:

```text
backend/src/HotelBooking.Api/Data/schema.mysql.sql
```

### Upload ảnh

- Upload file ảnh qua multipart/form-data.
- Hỗ trợ extension:
  - jpg
  - jpeg
  - png
  - webp
  - gif
- Giới hạn file 5 MB.
- Hiện lưu cục bộ trên API server.

## Tính năng AWS/demo

### Frontend hosting

- Frontend static files trong `frontend/public`, `frontend/src`, `frontend/assets`.
- Production đã từng deploy lên S3 private bucket.
- CloudFront dùng Origin Access Control cho S3.
- CloudFront default root object trỏ về `public/index.html`.

### Backend hosting

- ASP.NET Core API chạy trên EC2.
- Đặt sau Application Load Balancer.
- Có Auto Scaling Group.
- Health check qua `/health`.

### Database/cache

- RDS MySQL cho dữ liệu bền vững.
- ElastiCache Redis/Valkey cho cache kết quả tìm kiếm.

### Monitoring/evidence

- Có health endpoint `/health/aws`.
- Có tài liệu/runbook AWS trong `docs/runbooks`.
- Có load-testing folder cho Locust.
- Có kế hoạch CloudWatch dashboard/evidence.

## Trang frontend hiện có

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

## Ghi chú giới hạn hiện tại

- Các tính năng mới nhất đang chắc chắn ở local; cần deploy lại AWS để CloudFront có bản mới.
- Upload ảnh local chưa phải giải pháp bền vững cho production/ASG.
- Cần QA thủ công thêm trên mobile/responsive.
- Cần test kỹ phân quyền giữa nhiều hotel owner khác nhau.
- Cần cập nhật schema MySQL production trước khi chạy API mới trên AWS.
