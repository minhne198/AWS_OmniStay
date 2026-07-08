# 📋 KẾ HOẠCH DỰ ÁN TỐT NGHIỆP / ĐỒ ÁN

# Hệ thống Đặt phòng Khách sạn Du lịch chịu tải cao trên AWS

## *(High-Availability Hotel Booking System on AWS)*

> **Mục tiêu của tài liệu này:** Người đọc sau khi xem xong tài liệu này sẽ hiểu rõ hoàn toàn: hệ thống web này làm gì, sử dụng công nghệ gì, các thành phần AWS hoạt động như thế nào với nhau, cách viết code theo từng bước, và cách demo ấn tượng trong buổi báo cáo.

## Mục lục

1. [Tổng quan Dự án](#1-t%E1%BB%95ng-quan-d%E1%BB%B1-%C3%A1n)

2. [Tại sao dùng AWS? Vấn đề thực tế cần giải quyết](#2-t%E1%BA%A1i-sao-d%C3%B9ng-aws)

3. [Ngăn xếp Công nghệ (Tech Stack)](#3-ng%C4%83n-x%E1%BA%BFp-c%C3%B4ng-ngh%E1%BB%87)

4. [Sơ đồ Kiến trúc Hệ thống (Architecture)](#4-s%C6%A1-%C4%91%E1%BB%93-ki%E1%BA%BFn-tr%C3%BAc)

5. [Mô hình Cơ sở dữ liệu (Database Schema)](#5-m%C3%B4-h%C3%ACnh-c%C6%A1-s%E1%BB%9F-d%E1%BB%AF-li%E1%BB%87u)

6. [Luồng Hoạt động Chi tiết (Flow Diagrams)](#6-lu%E1%BB%93ng-ho%E1%BA%A1t-%C4%91%E1%BB%99ng)

7. [Hướng dẫn Thiết lập Môi trường (Setup Guide)](#7-thi%E1%BA%BFt-l%E1%BA%ADp-m%C3%B4i-tr%C6%B0%E1%BB%9Dng)

8. [Các bước Triển khai (Step-by-step)](#8-c%C3%A1c-b%C6%B0%E1%BB%9Bc-tri%E1%BB%83n-khai)

9. [Cấu trúc Code (Project Structure)](#9-c%E1%BA%A5u-tr%C3%BAc-code)

10. [Kịch bản Demo Báo cáo](#10-k%E1%BB%8Bch-b%E1%BA%A3n-demo)

11. [Chi phí và Lưu ý quan trọng](#11-chi-ph%C3%AD-v%C3%A0-l%C6%B0u-%C3%BD)

12. [A — Lịch trình thực hiện (Timeline)](#a--l%E1%BB%8Bch-tr%C3%ACnh-th%E1%BB%B1c-hi%E1%BB%87n)

13. [B — Chi tiết kỹ thuật từng phần](#b--chi-ti%E1%BA%BFt-k%E1%BB%B9-thu%E1%BA%ADt-t%E1%BB%ABng-ph%E1%BA%A7n)

14. [C — Những điểm còn thiếu & bổ sung](#c--nh%E1%BB%AFng-%C4%91i%E1%BB%83m-c%C3%B2n-thi%E1%BA%BFu--b%E1%BB%95-sung)

15. [D — Checklist tuần cuối & dự phòng sự cố](#d--checklist-tu%E1%BA%A7n-cu%E1%BB%91i--d%E1%BB%B1-ph%C3%B2ng-s%E1%BB%B1-c%E1%BB%91)

## 1. Tổng quan Dự án

### Mô tả sản phẩm

Đây là một **ứng dụng web đặt phòng khách sạn du lịch trực tuyến**, tương tự như booking.com hay agoda.com nhưng ở quy mô nhỏ hơn để phục vụ mục đích học tập.

**Người dùng cuối có thể:**

* Truy cập trang web và tìm kiếm phòng khách sạn theo điểm đến, ngày nhận phòng, ngày trả phòng, số lượng khách.

* Xem danh sách khách sạn có sẵn với hình ảnh, mô tả, giá tiền.

* Xem chi tiết từng phòng (ảnh, tiện ích, chính sách hủy phòng).

* Đặt phòng và nhận mã xác nhận đặt phòng.

* Xem và quản lý lịch sử đặt phòng của mình.

**Quản trị viên (Admin) có thể:**

* Thêm/sửa/xóa thông tin khách sạn, phòng, giá tiền.

* Xem danh sách tất cả các đơn đặt phòng.

* Xem thống kê doanh thu theo ngày/tháng.

### Mục tiêu kỹ thuật của đề tài

Đây là điểm quan trọng nhất, giúp phân biệt đề tài này với một ứng dụng web thông thường. Đề tài này tập trung chứng minh được **3 tính chất quan trọng** của hạ tầng điện toán đám mây:

| # | Tính chất                                 | Ý nghĩa thực tế                                                                                                  | Cách chứng minh trong đề tài                                               |
| - | ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| 1 | **High Availability (HA) - Sẵn sàng cao** | Website không bị sập ngay cả khi một máy chủ hoặc một vùng dữ liệu bị lỗi                                        | Chủ động tắt 1 máy chủ EC2 trong lúc đang chạy → Website vẫn hoạt động     |
| 2 | **Auto Scaling - Tự động co giãn**        | Hệ thống tự thêm máy chủ khi quá tải, tự giảm khi ít người dùng để tiết kiệm chi phí                             | Giả lập 500 người truy cập cùng lúc → Quan sát EC2 tự động tăng số lượng   |
| 3 | **Caching - Bộ nhớ đệm**                  | Các trang kết quả tìm kiếm phổ biến được phục vụ từ bộ nhớ tốc độ cao, không tốn công truy vấn database liên tục | Đo thời gian phản hồi khi không có cache (~100-200ms) vs có cache (~2-5ms) |

## 2. Tại sao dùng AWS?

### Bài toán thực tế

Hãy tưởng tượng bạn xây dựng trang web đặt phòng khách sạn và chạy trên 1 máy chủ duy nhất (server vật lý hoặc máy tính cá nhân). Điều gì xảy ra trong 3 tình huống phổ biến dưới đây:

```text
Tình huống 1: Tắt mùa cao điểm
   Tháng bình thường: 100 người/ngày  → Thuê 1 máy chủ: Lãng phí quá công suất
   Dịp Tết/Hè: 10.000 người/ngày    → Máy chủ bị quá tải, website lag hoặc sập

Tình huống 2: Máy chủ gặp sự cố
   Máy chủ cứng hỏng, mất điện đột ngột → Website ngừng hoạt động 100%
   Khách hàng không thể đặt phòng → Mất doanh thu và uy tín

Tình huống 3: Database bị quá tải
   10.000 người cùng tìm kiếm "phòng tại Đà Nẵng 20/4" → Database nhận 10.000 queries giống hệt nhau
   → Database quá tải, phản hồi rất chậm hoặc crash
```

**AWS giải quyết cả 3 vấn đề trên** mà không cần quản trị viên thức đêm:

```text
Giải pháp 1: Auto Scaling Group
   Tháng bình thường: ASG duy trì 1 máy chủ EC2   → Chi phí thấp
   Dịp Tết: CPU vượt 70% → ASG tự thêm 2-3 EC2    → Tải được phân bổ đều

Giải pháp 2: Nhiều máy chủ + Load Balancer
   Chạy 2 EC2 ở 2 vùng địa lý khác nhau (AZ)
   Nếu EC2 ở AZ-1 chết → ALB tự chuyển 100% traffic sang EC2 ở AZ-2

Giải pháp 3: ElastiCache Redis
   Kết quả tìm kiếm "phòng Đà Nẵng 20/4" được lưu vào Redis 5 phút
   10.000 request tiếp theo → Lấy từ Redis (< 2ms), không đụng database
```

## 3. Ngăn xếp Công nghệ

### 3.1 Frontend (Giao diện người dùng)

| Công nghệ                | Phiên bản | Vai trò                                                           | Lý do chọn                                                    |
| ------------------------ | --------- | ----------------------------------------------------------------- | ------------------------------------------------------------- |
| **HTML5 / CSS3**         | -         | Cấu trúc và giao diện trang web                                   | Nền tảng chuẩn của web                                        |
| **JavaScript (Vanilla)** | ES2022+   | Xử lý logic tương tác phía client                                 | Không cần framework phức tạp cho đề tài                       |
| **Bootstrap**            | 5.3       | Responsive design, giao diện đẹp nhanh                            | Tăng tốc độ phát triển UI                                     |
| **Axios**                | 1.x       | Gửi HTTP request từ trình duyệt đến .NET API                      | Dễ dùng hơn fetch API thuần                                   |
| **Amazon S3**            | -         | Lưu trữ và phục vụ tất cả các file tĩnh (HTML, CSS, JS, ảnh)      | Chi phí rất thấp, tự động mở rộng                             |
| **Amazon CloudFront**    | -         | CDN phân phối file tĩnh từ S3 đến người dùng với độ trễ thấp nhất | Người dùng ở Hà Nội hay HCM đều nhận file từ máy chủ gần nhất |

### 3.2 Backend (API Server)

| Công nghệ                            | Phiên bản    | Vai trò                                                | Lý do chọn                                                   |
| ------------------------------------ | ------------ | ------------------------------------------------------ | ------------------------------------------------------------ |
| **ASP.NET Core Web API**             | .NET 8 (LTS) | Framework xây dựng RESTful API                         | Hiệu năng cao, chạy tốt trên Linux, miễn phí, AWS hỗ trợ tốt |
| **Entity Framework Core**            | 8.x          | ORM - Ánh xạ đối tượng C# với bảng Database            | Viết code C# thay vì SQL thuần, dễ bảo trì                   |
| **Pomelo.EntityFrameworkCore.MySql** | 8.x          | Driver kết nối EF Core với MySQL (RDS)                 | Tương thích tốt nhất với AWS RDS MySQL                       |
| **StackExchange.Redis**              | 2.x          | Thư viện kết nối với Amazon ElastiCache Redis          | Thư viện chuẩn, được AWS khuyến nghị                         |
| **AWSSDK.S3**                        | 3.x          | SDK chính thức của AWS để thao tác với S3 (upload ảnh) | Chính thức từ AWS, đầy đủ tính năng                          |
| **Swashbuckle (Swagger)**            | 6.x          | Tự động sinh tài liệu API và UI thử nghiệm             | Dễ test API mà không cần Postman                             |

### 3.3 Hạ tầng AWS (Infrastructure)

> **Lưu ý:** Đây là phần cốt lõi của đề tài. Người làm cần hiểu rõ từng dịch vụ AWS bên dưới, vai trò của nó là gì, tại sao cần nó, và nó liên kết với các thành phần khác như thế nào.

| Dịch vụ AWS                         | Loại         | Vai trò trong hệ thống                                                          | Cấu hình sử dụng                                                       |
| ----------------------------------- | ------------ | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| **Amazon VPC**                      | Mạng         | Tạo mạng riêng tư ảo, cô lập hoàn toàn với internet                             | 1 VPC, dải mạng `10.0.0.0/16`                                          |
| **Subnets**                         | Mạng         | Phân chia mạng VPC thành các vùng có mục đích khác nhau                         | 2 Public Subnet (ALB, NAT GW) + 4 Private Subnet (EC2, RDS, Redis)    |
| **Security Groups**                 | Bảo mật mạng | Tường lửa ảo, kiểm soát traffic vào/ra theo port và nguồn gốc                   | SG cho ALB, SG cho EC2, SG cho RDS, SG cho Redis                       |
| **Internet Gateway**                | Mạng         | Cổng ra vào internet của toàn bộ VPC                                            | 1 IGW gắn vào VPC                                                      |
| **NAT Gateway**                     | Mạng         | Cho phép EC2 trong Private Subnet kết nối outbound internet (tải package, gửi metrics) | 2 NAT GW ở 2 Public Subnet (1 mỗi AZ). **Lưu ý:** tốn $0.059/giờ     |
| **Amazon Route 53**                 | DNS          | Phân giải tên miền, định tuyến traffic đến CloudFront                            | 1 Hosted Zone (tùy chọn, có thể dùng CloudFront domain mặc định)       |
| **Application Load Balancer (ALB)** | Phân tải     | Nhận toàn bộ request từ internet và phân phối đều đến các EC2                   | 1 ALB ở Public Subnet, lắng nghe cổng 80                               |
| **Amazon EC2**                      | Compute      | Máy chủ ảo chạy ứng dụng ASP.NET Core                                           | `t3.micro` (Free Tier), Amazon Linux 2023, cài .NET 8                  |
| **Auto Scaling Group (ASG)**        | Compute      | Tự động thêm/bớt số lượng EC2 dựa trên tải CPU                                  | Min=1, Max=4, Target CPU=70%                                           |
| **Amazon RDS**                      | Database     | Cơ sở dữ liệu quan hệ (MySQL) lưu toàn bộ dữ liệu nghiệp vụ                     | `db.t3.micro` MySQL 8.0, Primary + Standby (Multi-AZ)                  |
| **Amazon ElastiCache (Redis)**      | Cache        | Bộ nhớ đệm tốc độ cao, lưu kết quả tìm kiếm phổ biến                            | `cache.t3.micro`, Redis 7.x, Free Tier                                 |
| **Amazon S3**                       | Storage      | Lưu trữ file tĩnh (giao diện web, ảnh khách sạn)                                | 1 Bucket, Block Public Access ON, truy cập qua CloudFront OAC          |
| **Amazon CloudFront**               | CDN          | Phân phối nội dung tĩnh từ S3 đến người dùng, forward API đến ALB               | 1 Distribution, Origin: S3 bucket (OAC) + ALB                          |
| **AWS WAF**                         | Bảo mật      | Web Application Firewall gắn vào CloudFront, chống SQL Injection, XSS, DDoS     | Gắn vào CloudFront Distribution                                        |
| **IAM Roles & Policies**            | Bảo mật      | Cấp quyền cho EC2 truy cập S3, CloudWatch mà không cần Access Key (Global)      | IAM Role gắn vào EC2 Instance Profile                                  |
| **AWS KMS**                         | Bảo mật      | Quản lý khóa mã hóa, mã hóa dữ liệu RDS at rest                                | 1 KMS Key cho RDS encryption                                           |
| **AWS Secrets Manager**             | Bảo mật      | Lưu trữ an toàn DB password, JWT secret, hỗ trợ rotation                        | Lưu connection string, JWT secret (hoặc dùng SSM Parameter Store)       |
| **Amazon CloudWatch**               | Giám sát     | Thu thập metrics (CPU, request count, latency) và tạo Dashboard                 | Thiết lập Alarm khi CPU > 70% để trigger ASG + gửi SNS                 |
| **Amazon SNS**                      | Thông báo    | Gửi cảnh báo email khi CloudWatch Alarm kích hoạt (CPU cao, lỗi 5xx)            | 1 SNS Topic, subscribe Admin email                                      |
| **AWS CDK**                         | IaC          | Định nghĩa toàn bộ hạ tầng dưới dạng code, deploy bằng 1 lệnh                   | AWS CDK (TypeScript)                                                    |

### 3.4 Công cụ phát triển

| Công cụ                          | Mục đích                                                                    |
| -------------------------------- | --------------------------------------------------------------------------- |
| **Visual Studio 2022 Community** | IDE chính để code ASP.NET Core (miễn phí)                                   |
| **VS Code**                      | Editor để code frontend HTML/JS                                             |
| **AWS CLI**                      | Gõ lệnh AWS từ máy tính cá nhân (upload S3, kiểm tra EC2...)                |
| **DBeaver**                      | Công cụ xem và quản lý database MySQL/RDS bằng giao diện đồ họa             |
| **Postman**                      | Test API trước khi kết nối frontend                                         |
| **Git + GitHub**                 | Quản lý phiên bản code                                                      |
| **Locust** (Python)              | Công cụ giả lập hàng trăm người dùng truy cập cùng lúc để test Auto Scaling |

## 4. Sơ đồ Kiến trúc

### 4.1 Sơ đồ tổng thể

```text
┌─ BÊN NGOÀI AWS ──────────────────────────────────────────────────────────────┐
│  👤 User / Admin (Web Browser)                           📬 Admin Email     │
└───────────┬──────────────────────────────────────────────────────▲──────────┘
            │ ① HTTPS Request                          ⑪ Send Alert│
            ▼                                                      │
┌─ AWS Cloud ──────────────────────────────────────────────────────────────────┐
│                                                                              │
│  🔑 IAM Roles & Policies (Global Service)                                   │
│                                                                              │
│  ┌─ Region: ap-southeast-1 (Singapore) ────────────────────────────────────┐│
│  │                                                                          ││
│  │  Route 53 ──②──→ CloudFront ──③ Serve Frontend──→ S3 (OAC Private)     ││
│  │                    ├── WAF (nét đứt: lọc request)                        ││
│  │                    └──④ Forward API──→ ALB (Public Subnet)              ││
│  │                                                                          ││
│  │  ┌─ VPC: 10.0.0.0/16 ──────────────────────────────────────────────────┐││
│  │  │                                                                      │││
│  │  │  Public Subnet AZ-1 (10.0.1.0/24)    Public Subnet AZ-2 (10.0.2.0) │││
│  │  │  [Internet Gateway]  [NAT GW]  [ALB]  [NAT GW]                      │││
│  │  │                                                                      │││
│  │  │  ╔═══ Auto Scaling Group (Min=1, Max=4, Target CPU=70%) ══════════╗ │││
│  │  │  ║                        ⑤ Route to Backend (:8080)               ║ │││
│  │  │  ║  Private App AZ-1 (10.0.10.0/24)  Private App AZ-2 (10.0.11.0) ║ │││
│  │  │  ║  [EC2 ASP.NET Core 8 / t3.micro]  [EC2 ASP.NET Core 8 / t3.micro]║│││
│  │  │  ╚════════════════════════════════════════════════════════════════╝ │││
│  │  │       │⑥ Check Cache    │⑦ Query DB     │⑧ Upload/Get Images → S3  │││
│  │  │       ▼                  ▼                                          │││
│  │  │  Private DB AZ-1 (10.0.20.0/24)      Private DB AZ-2 (10.0.21.0)   │││
│  │  │  [RDS MySQL 8.0 Primary] [Redis 7.x] [RDS MySQL 8.0 Standby]       │││
│  │  │        └───────────── ⑨ Sync Replication ────────────┘              │││
│  │  └──────────────────────────────────────────────────────────────────────┘││
│  │                                                                          ││
│  │  ┌─ Security (Regional) ──┐  ┌─ Monitoring & Alerting ────────────────┐││
│  │  │ 🔒 KMS   🔐 Secrets Mgr│  │ CloudWatch ──⑩ Alarm──→ SNS ──⑪──→ Email│|
│  │  └────────────────────────┘  │         Auto Scale Trigger ──→ ASG     │││
│  │                               └───────────────────────────────────────┘││
│  └──────────────────────────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Giải thích luồng request từ trình duyệt vào hệ thống

**Khi khách hàng mở trang web (load giao diện):**

```text
Trình duyệt → Route 53 (phân giải DNS) → CloudFront (CDN)
→ AWS WAF kiểm tra request (chặn SQL injection, XSS, DDoS)
→ CloudFront kiểm tra cache CDN
  ├── Có cache: Trả về ngay từ máy chủ CDN gần nhất (cực nhanh)
  └── Không có cache: Lấy từ S3 qua OAC (S3 bucket private) → Lưu cache CDN → Trả về
```

**Khi khách hàng tìm kiếm phòng (API call):**

```text
JavaScript gọi API → Route 53 → CloudFront
→ AWS WAF lọc request (chặn tấn công)
→ CloudFront forward /api/* đến ALB
→ ALB → Chọn EC2 có tải thấp nhất (Round Robin)
→ EC2 (.NET API) xử lý request
→ ⑥ .NET kiểm tra Redis Cache trước
  ├── Cache HIT: Trả về JSON kết quả ngay (< 5ms)
  └── Cache MISS: ⑦ Query RDS MySQL → Lưu kết quả vào Redis (TTL=5min) → Trả về JSON
→ JavaScript nhận JSON → Render danh sách phòng lên màn hình
```

**Khi Auto Scaling kích hoạt:**

```text
CloudWatch giám sát CPU của các EC2 trong ASG
CPU trung bình > 70% trong 2 phút liên tiếp
→ CloudWatch Alarm → Trigger ASG Scale Out Policy
→ ASG khởi động thêm 1 EC2 mới từ Launch Template
→ EC2 mới chạy User Data Script: cài .NET Runtime, tải code, start service
→ EC2 mới đăng ký vào Target Group của ALB
→ ALB bắt đầu phân phối traffic đến EC2 mới (~3-5 phút tổng)
```

## 5. Mô hình Cơ sở dữ liệu

### 5.1 Danh sách bảng (Tables)

```sql
-- ===== DATABASE: hotel_booking =====

-- Bảng khách sạn
CREATE TABLE Hotels (
    HotelId       INT PRIMARY KEY AUTO_INCREMENT,
    Name          VARCHAR(200) NOT NULL,         -- Tên khách sạn
    City          VARCHAR(100) NOT NULL,         -- Thành phố (Đà Nẵng, Hà Nội, HCM...)
    Address       VARCHAR(500) NOT NULL,         -- Địa chỉ đầy đủ
    Description   TEXT,                          -- Mô tả khách sạn
    StarRating    TINYINT NOT NULL DEFAULT 3,    -- Số sao (1-5)
    MainImageUrl  VARCHAR(500),                  -- URL ảnh đại diện (lưu trên S3)
    CreatedAt     DATETIME DEFAULT NOW()
);

-- Bảng loại phòng (mỗi khách sạn có nhiều loại phòng)
CREATE TABLE RoomTypes (
    RoomTypeId    INT PRIMARY KEY AUTO_INCREMENT,
    HotelId       INT NOT NULL,                  -- Thuộc khách sạn nào (FK)
    Name          VARCHAR(100) NOT NULL,         -- Tên loại phòng (Standard, Deluxe, Suite)
    Description   TEXT,                          -- Mô tả loại phòng
    MaxGuests     TINYINT NOT NULL DEFAULT 2,    -- Số khách tối đa
    PricePerNight DECIMAL(10,2) NOT NULL,        -- Giá mỗi đêm (VNĐ)
    TotalRooms    INT NOT NULL DEFAULT 10,       -- Tổng số phòng loại này
    ImageUrl      VARCHAR(500),                  -- URL ảnh phòng (lưu trên S3)
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);

-- Bảng người dùng
CREATE TABLE Users (
    UserId        INT PRIMARY KEY AUTO_INCREMENT,
    FullName      VARCHAR(200) NOT NULL,
    Email         VARCHAR(200) UNIQUE NOT NULL,
    PasswordHash  VARCHAR(500) NOT NULL,         -- Mật khẩu đã được hash (bcrypt)
    Phone         VARCHAR(20),
    Role          ENUM('Customer', 'Admin') DEFAULT 'Customer',
    CreatedAt     DATETIME DEFAULT NOW()
);

-- Bảng đặt phòng
CREATE TABLE Bookings (
    BookingId     INT PRIMARY KEY AUTO_INCREMENT,
    BookingCode   VARCHAR(20) UNIQUE NOT NULL,   -- Mã đặt phòng (vd: BK20240501-001)
    UserId        INT NOT NULL,                  -- Ai đặt (FK)
    RoomTypeId    INT NOT NULL,                  -- Đặt loại phòng nào (FK)
    CheckInDate   DATE NOT NULL,                 -- Ngày nhận phòng
    CheckOutDate  DATE NOT NULL,                 -- Ngày trả phòng
    NumberOfGuests TINYINT NOT NULL,
    TotalPrice    DECIMAL(12,2) NOT NULL,        -- Tổng tiền = số đêm * giá/đêm
    Status        ENUM('Pending','Confirmed','Cancelled','Completed') DEFAULT 'Confirmed',
    Notes         TEXT,                          -- Ghi chú thêm của khách
    CreatedAt     DATETIME DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId)
);
```

### 5.2 Quan hệ giữa các bảng (ERD)

```text
Hotels ──┤< RoomTypes ──┤< Bookings >├── Users
(1)       (nhiều)  (1)   (nhiều)
```

*1 Khách sạn có nhiều Loại phòng. 1 Loại phòng có nhiều Lượt đặt. 1 Người dùng có nhiều Lượt đặt.*

## 6. Luồng Hoạt động

### 6.1 Tính năng Tìm kiếm phòng (có Redis Caching)

```text
[Browser] Người dùng nhập: Đà Nẵng | 15/8 - 18/8 | 2 khách → Bấm "Tìm kiếm"
    │
    │ GET /api/search?city=DaNang&checkin=2024-08-15&checkout=2024-08-18&guests=2
    ▼
[ALB] Nhận request, chọn EC2 đang rảnh nhất, chuyển tiếp
    │
    ▼
[EC2 - .NET API] SearchController.Search() được gọi
    │
    │ Tạo Cache Key = "search:DaNang:20240815:20240818:2"
    ▼
[ElastiCache Redis] GET "search:DaNang:20240815:20240818:2"
    │
    ├── [Cache HIT] Trả về JSON string đã lưu sẵn
    │       │
    │       └── .NET deserialize JSON → Trả về HTTP 200 với danh sách phòng
    │           Tổng thời gian: ~3-8ms ✅
    │
    └── [Cache MISS] Redis trả về null
            │
            ▼
        [RDS MySQL] Thực thi query tìm phòng còn trống:
            SELECT rt.*, h.Name as HotelName, h.City, h.StarRating,
                   rt.TotalRooms - COALESCE(booked.Count,0) as AvailableRooms
            FROM RoomTypes rt
            JOIN Hotels h ON rt.HotelId = h.HotelId
            LEFT JOIN (
                SELECT RoomTypeId, COUNT(*) as Count FROM Bookings
                WHERE Status != 'Cancelled'
                  AND CheckInDate < '2024-08-18'
                  AND CheckOutDate > '2024-08-15'
                GROUP BY RoomTypeId
            ) booked ON rt.RoomTypeId = booked.RoomTypeId
            WHERE h.City = 'DaNang'
              AND rt.MaxGuests >= 2
              AND (rt.TotalRooms - COALESCE(booked.Count,0)) > 0
            ORDER BY rt.PricePerNight ASC;
            │
            │ Thời gian query: ~80-150ms
            ▼
        [ElastiCache Redis] SET "search:DaNang:20240815:20240818:2" = [JSON kết quả] EX 300
            │   (Lưu vào cache, hết hạn sau 300 giây = 5 phút)
            ▼
        [.NET API] Trả về HTTP 200 với danh sách phòng
            Tổng thời gian: ~100-170ms (lần đầu tiên)
            Lần thứ 2 trở đi: ~3-8ms (từ cache) ✅
```

### 6.2 Tính năng Đặt phòng (ngăn chặn Overbooking)

```text
[Browser] Người dùng bấm "Đặt phòng" → Điền form → Bấm "Xác nhận"
    │
    │ POST /api/bookings  Body: { roomTypeId: 5, checkIn: "2024-08-15", checkOut: "2024-08-18", guests: 2 }
    ▼
[EC2 - .NET API] BookingsController.Create() được gọi
    │
    │ Bước 1: Xác thực người dùng (JWT Token hợp lệ không?)
    ▼
    │ Bước 2: Bắt đầu Database Transaction (quan trọng!)
    │         BEGIN TRANSACTION;
    ▼
[RDS MySQL] Kiểm tra phòng còn trống không (SELECT FOR UPDATE - Khóa hàng lại)
    │         Câu lệnh này "khóa" dữ liệu phòng, các request đặt phòng khác
    │         phải chờ cho đến khi transaction này hoàn thành.
    │         → Ngăn chặn 2 người cùng đặt 1 phòng cuối cùng còn lại!
    │
    ├── [Còn phòng] Tiếp tục:
    │       INSERT INTO Bookings VALUES (...)  -- Tạo đơn đặt phòng
    │       COMMIT TRANSACTION;               -- Hoàn thành, mở khóa
    │       │
    │       ▼
    │   [ElastiCache Redis] Xóa Cache Key liên quan đến phòng này
    │       DEL "search:DaNang:20240815:20240818:*"
    │       (Để lần tìm kiếm tiếp theo sẽ query lại DB, phản ánh phòng vừa được đặt)
    │       │
    │       └── Trả về HTTP 201 + { bookingCode: "BK20240501-001", totalPrice: 1500000 }
    │
    └── [Hết phòng] 
            ROLLBACK TRANSACTION;
            Trả về HTTP 409 Conflict + { error: "Xin lỗi! Phòng này vừa được đặt bởi người khác." }
```

### 6.3 Luồng Auto Scaling (Tự động co giãn)

```text
[CloudWatch] Giám sát liên tục mỗi 60 giây:
    - Lấy CPU Utilization trung bình của tất cả EC2 trong ASG

Trạng thái bình thường (100 người dùng, CPU ~20%):
    ASG: 1 EC2 đang chạy
    Chi phí: Rẻ nhất có thể

Khi Locust giả lập 500 người dùng tìm kiếm cùng lúc:
    CloudWatch ghi nhận CPU = 80% (> ngưỡng 70%)
    │
    │ Sau 2 phút CPU vẫn > 70%
    ▼
    CloudWatch Alarm → Trạng thái: ALARM
    │
    ▼
    ASG nhận tín hiệu → Thực hiện Scale Out Policy
    │   "Thêm 1 EC2 mới"
    ▼
    EC2 mới được khởi động từ Launch Template
    │   User Data Script chạy tự động:
    │   1. sudo dnf install -y aspnetcore-runtime-8.0
    │   2. aws s3 cp s3://my-app-bucket/app.zip /home/ec2-user/
    │   3. unzip app.zip -d /home/ec2-user/app
    │   4. sudo systemctl start hotel-api
    │
    │ ~3-4 phút để khởi động hoàn toàn
    ▼
    ALB Health Check: Gọi GET /health mỗi 30 giây
    │   → EC2 mới trả về HTTP 200
    ▼
    ALB đưa EC2 mới vào Target Group
    │   → Bắt đầu phân phối traffic đến EC2 mới
    ▼
    CPU trung bình giảm xuống còn ~40% (2 EC2 cùng gánh tải)

Sau khi Locust dừng (CPU < 30% trong 10 phút liên tiếp):
    ASG thực hiện Scale In Policy
    │   "Xóa bớt 1 EC2"
    ▼
    ALB ngừng gửi request đến EC2 sắp bị xóa (Connection Draining)
    EC2 bị terminate sau khi xử lý hết request còn lại
    Hệ thống trở lại 1 EC2 → Tiết kiệm chi phí ✅
```

## 7. Thiết lập Môi trường

### 7.1 Yêu cầu tài khoản AWS

1. Tạo tài khoản AWS tại [aws.amazon.com](https://aws.amazon.com) (cần thẻ VISA/Mastercard - chỉ xác minh, không bị tính tiền ngay).

2. Tạo IAM User riêng cho dự án (KHÔNG dùng tài khoản root):

   * Vào IAM → Users → Create User.

   * Gán policy: `AdministratorAccess` (cho mục đích học tập).

   * Tạo Access Key → Lưu lại `Access Key ID` và `Secret Access Key`.

3. Cài AWS CLI trên máy tính cá nhân và chạy `aws configure` để nhập thông tin truy cập.

4. **Cực kỳ quan trọng:** Vào **AWS Budgets** → Tạo Budget 10$ → Bật thông báo email khi chi phí dự báo vượt 5$.

### 7.2 Cài đặt trên máy tính cá nhân

```bash
# 1. Cài .NET 8 SDK
# Tải từ: https://dotnet.microsoft.com/download/dotnet/8.0

# 2. Cài AWS CLI
# Tải từ: https://aws.amazon.com/cli/

# 3. Cài Node.js (cho AWS CDK)
# Tải từ: https://nodejs.org

# 4. Cài AWS CDK
npm install -g aws-cdk

# 5. Xác nhận cài đặt thành công
dotnet --version     # → 8.x.x
aws --version        # → aws-cli/2.x.x
cdk --version        # → 2.x.x
```

## 8. Các bước Triển khai (Step-by-step)

Toàn bộ dự án được chia thành **5 Giai đoạn (Phase)**. Mỗi giai đoạn hoàn chỉnh độc lập trước khi chuyển sang giai đoạn tiếp theo.

### Phase 1: Xây dựng Hạ tầng Mạng (VPC & Bảo mật)

**Mục tiêu:** Tạo môi trường mạng an toàn trên AWS.

**Việc cần làm:**

1. Tạo VPC mới với CIDR `10.0.0.0/16`.

2. Tạo các Subnet:

   * `10.0.1.0/24` - Public Subnet AZ-a (đặt ALB + NAT Gateway tại đây)

   * `10.0.2.0/24` - Public Subnet AZ-b (đặt ALB + NAT Gateway tại đây)

   * `10.0.10.0/24` - Private App Subnet AZ-a (đặt EC2 tại đây)

   * `10.0.11.0/24` - Private App Subnet AZ-b (đặt EC2 tại đây)

   * `10.0.20.0/24` - Private DB Subnet AZ-a (đặt RDS, Redis tại đây)

   * `10.0.21.0/24` - Private DB Subnet AZ-b (đặt RDS, Redis tại đây)

3. Tạo Internet Gateway và gắn vào VPC.

4. Cập nhật Route Table của Public Subnet: thêm route `0.0.0.0/0 → IGW`.

5. Tạo 2 NAT Gateway (cho EC2 trong Private Subnet có thể kết nối outbound internet):

   * **NAT GW AZ-a:** Đặt ở Public Subnet AZ-a, gán 1 Elastic IP.

   * **NAT GW AZ-b:** Đặt ở Public Subnet AZ-b, gán 1 Elastic IP.

   > **Lưu ý chi phí:** NAT Gateway tốn $0.059/giờ (~$42/tháng mỗi cái). Chỉ bật khi cần demo, xóa sau khi demo xong để tiết kiệm.

6. Cập nhật Route Table của Private Subnet:

   * Private Subnet AZ-a: thêm route `0.0.0.0/0 → NAT GW AZ-a`

   * Private Subnet AZ-b: thêm route `0.0.0.0/0 → NAT GW AZ-b`

   (Để EC2 trong Private Subnet có thể: tải package .NET, gửi metrics CloudWatch, tải code từ S3)

7. Tạo 4 Security Groups:

   * **SG-ALB:** Cho phép inbound port 80 từ `0.0.0.0/0` (mọi người).

   * **SG-EC2:** Cho phép inbound port 8080 chỉ từ `SG-ALB`.

   * **SG-RDS:** Cho phép inbound port 3306 chỉ từ `SG-EC2`.

   * **SG-Redis:** Cho phép inbound port 6379 chỉ từ `SG-EC2`.

8. Cấu hình Security:

   * Tạo IAM Role `EC2-HotelAPI-Role` với policies: `AmazonS3ReadOnlyAccess`, `CloudWatchAgentServerPolicy`.

   * Tạo KMS Key cho RDS encryption (bật "Encryption at rest" khi tạo RDS ở Phase 2).

   * Lưu DB password, Redis endpoint, JWT secret vào **AWS Secrets Manager** (hoặc SSM Parameter Store Standard - miễn phí).

**Kiểm tra:** Đối chiếu sơ đồ kiến trúc draw.io với những gì đã tạo trên AWS Console. Đảm bảo mũi tên NAT GW kết nối đúng từ Private Subnet ra internet.

### Phase 2: Tạo Database và Cache

**Mục tiêu:** Có database MySQL và Redis sẵn sàng để kết nối.

**Việc cần làm (RDS MySQL):**

1. Vào RDS → Create Database.

2. Chọn: Standard Create | MySQL 8.0 | Free Tier Template.

3. DB instance: `db.t3.micro` | Single-AZ.

4. Master username: `admin`, Master password: đặt mật khẩu mạnh, GHI NHỚ lại.

5. VPC: Chọn VPC vừa tạo | Subnet Group: tạo nhóm gồm 2 Private DB Subnet.

6. Security Group: Chọn `SG-RDS`.

7. Sau khi tạo xong, lấy **Endpoint** (địa chỉ kết nối), dạng: `mydb.xxxxx.ap-southeast-1.rds.amazonaws.com`.

**Việc cần làm (ElastiCache Redis):**

1. Vào ElastiCache → Create Cluster → Redis OSS.

2. Cluster mode: Disabled | Node type: `cache.t3.micro`.

3. Số Replicas: 0 (để tiết kiệm Free Tier).

4. VPC: Chọn VPC vừa tạo | Subnet Group: 2 Private DB Subnet.

5. Security Group: Chọn `SG-Redis`.

6. Sau khi tạo xong, lấy **Primary Endpoint**, dạng: `myredis.xxxxx.cache.amazonaws.com:6379`.

**Tạo Database Schema:**

* Dùng công cụ DBeaver kết nối vào RDS (kết nối từ máy tính thông qua EC2 làm Bastion host).

* Chạy file SQL tạo bảng ở Phần 5.1.

* Chạy file SQL insert dữ liệu mẫu (10 khách sạn, 30 loại phòng).

### Phase 3: Phát triển Backend API (.NET)

**Mục tiêu:** Viết xong API hoàn chỉnh, chạy được trên máy tính cá nhân.

**Cấu trúc project ASP.NET Core:**

```text
HotelBookingApi/
├── Controllers/
│   ├── HotelsController.cs      ← API tìm kiếm khách sạn
│   ├── BookingsController.cs    ← API đặt phòng / xem lịch sử
│   ├── AuthController.cs        ← API đăng ký / đăng nhập
│   └── AdminController.cs       ← API quản trị (admin only)
├── Models/
│   ├── Hotel.cs                 ← Entity class ánh xạ bảng Hotels
│   ├── RoomType.cs
│   ├── User.cs
│   └── Booking.cs
├── Services/
│   ├── SearchService.cs         ← Logic tìm kiếm + Cache Redis
│   ├── BookingService.cs        ← Logic đặt phòng + Transaction DB
│   └── S3Service.cs             ← Logic upload ảnh lên S3
├── Data/
│   └── AppDbContext.cs          ← EF Core DbContext kết nối RDS
├── Middleware/
│   └── JwtMiddleware.cs         ← Xác thực JWT Token
├── appsettings.json             ← Cấu hình (đọc từ biến môi trường)
└── Program.cs                   ← Điểm khởi động ứng dụng
```

**Các API Endpoint cần xây dựng:**

```text
[Public - Không cần đăng nhập]
GET  /api/hotels/search?city={city}&checkIn={date}&checkOut={date}&guests={n}
GET  /api/hotels/{hotelId}
GET  /api/hotels/{hotelId}/rooms

[Auth]
POST /api/auth/register    Body: { fullName, email, password }
POST /api/auth/login       Body: { email, password } → Trả về JWT Token

[Customer - Cần JWT Token]
POST /api/bookings         Body: { roomTypeId, checkIn, checkOut, guests }
GET  /api/bookings/my      Trả về danh sách đặt phòng của tôi
DELETE /api/bookings/{id}  Hủy đặt phòng

[Admin - Cần JWT Token của Admin]
GET    /api/admin/bookings          Xem tất cả đơn đặt phòng
POST   /api/admin/hotels            Thêm khách sạn mới
PUT    /api/admin/hotels/{id}       Sửa thông tin khách sạn
POST   /api/admin/hotels/{id}/image Upload ảnh khách sạn lên S3

[Health Check - Dùng cho ALB]
GET  /health  → Trả về HTTP 200 OK (ALB dùng để kiểm tra EC2 còn sống không)
```

#### AuthController.cs (code mẫu)

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { error = "Email đã tồn tại" });

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Phone = req.Phone,
            Role = "Customer"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { userId = user.UserId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Email hoặc mật khẩu không đúng" });

        var token = GenerateJwtToken(user);
        return Ok(new { token, user.FullName, user.Role });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:ExpireDays"])),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### SearchService.cs (code mẫu — có Redis caching)

```csharp
public class SearchService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;
    private readonly ILogger<SearchService> _logger;

    public SearchService(AppDbContext db, IConnectionMultiplexer redis,
        ILogger<SearchService> logger)
    {
        _db = db;
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<List<SearchResult>> Search(SearchRequest req)
    {
        var cacheKey = $"search:{req.City}:{req.CheckIn:yyyyMMdd}:{req.CheckOut:yyyyMMdd}:{req.Guests}";

        // Thử lấy từ Redis
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            _logger.LogInformation("CACHE HIT: {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<List<SearchResult>>(cached);
        }

        _logger.LogInformation("CACHE MISS: {CacheKey}", cacheKey);

        // Query database
        var results = await _db.RoomTypes
            .Where(rt => rt.Hotel.City == req.City
                && rt.MaxGuests >= req.Guests
                && rt.TotalRooms >
                    _db.Bookings.Count(b => b.RoomTypeId == rt.RoomTypeId
                        && b.Status != "Cancelled"
                        && b.CheckInDate < req.CheckOut
                        && b.CheckOutDate > req.CheckIn))
            .Select(rt => new SearchResult { ... })
            .OrderBy(r => r.PricePerNight)
            .ToListAsync();

        // Lưu vào Redis trong 5 phút
        var json = JsonSerializer.Serialize(results);
        await _redis.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(5));

        return results;
    }
}
```

#### BookingService.cs (code mẫu — chống Overbooking)

```csharp
public class BookingService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;

    public async Task<BookingResult> CreateBooking(CreateBookingRequest req, int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            // Kiểm tra phòng còn trống — dùng FOR UPDATE (khóa hàng)
            var roomType = await _db.RoomTypes
                .FromSqlRaw(@"SELECT * FROM RoomTypes
                    WHERE RoomTypeId = {0}
                    FOR UPDATE", req.RoomTypeId)
                .FirstOrDefaultAsync();

            if (roomType == null)
                throw new Exception("Loại phòng không tồn tại");

            var nights = (req.CheckOut - req.CheckIn).Days;
            var totalPrice = roomType.PricePerNight * nights;
            var bookingCode = $"BK{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}";

            var booking = new Booking { ... };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Xóa cache liên quan
            var cachePattern = $"search:*:{req.CheckIn:yyyyMMdd}:{req.CheckOut:yyyyMMdd}:*";
            var server = _redis.Multiplexer.GetServer(
                _redis.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: cachePattern).ToArray();
            foreach (var key in keys)
                await _redis.KeyDeleteAsync(key);

            return new BookingResult { BookingCode = bookingCode, TotalPrice = totalPrice, Status = "Confirmed" };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new Exception("Xin lỗi! Phòng này vừa được đặt bởi người khác.");
        }
    }
}
```

### Phase 4: Xây dựng Frontend và Triển khai lên S3/CloudFront

**Mục tiêu:** Có giao diện web đẹp, kết nối được với .NET API.

**Các trang cần xây dựng (HTML + Bootstrap + JS):**

| Trang               | File                | Mô tả                                                |
| ------------------- | ------------------- | ---------------------------------------------------- |
| Trang chủ           | `index.html`        | Form tìm kiếm phòng (chọn thành phố, ngày, số khách) |
| Kết quả tìm kiếm    | `search.html`       | Danh sách khách sạn/phòng phù hợp với filter         |
| Chi tiết khách sạn  | `hotel.html`        | Ảnh, mô tả, danh sách loại phòng với giá             |
| Đặt phòng           | `booking.html`      | Form điền thông tin, xác nhận đặt phòng              |
| Xác nhận            | `confirmation.html` | Hiển thị mã đặt phòng thành công                     |
| Đăng nhập / Đăng ký | `auth.html`         | Form đăng nhập và đăng ký tài khoản                  |
| Lịch sử đặt phòng   | `my-bookings.html`  | Danh sách đơn đặt phòng của tôi                      |
| Admin Dashboard     | `admin/index.html`  | Quản trị khách sạn, xem tất cả đơn đặt phòng         |

**api.js — class gọi API tập trung:**

```javascript
const API_BASE = 'http://' + window.location.hostname + ':8080/api';

const api = {
    async get(endpoint) {
        const res = await fetch(API_BASE + endpoint, {
            headers: this.getHeaders()
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || 'Lỗi không xác định');
        }
        return res.json();
    },

    async post(endpoint, data) {
        const res = await fetch(API_BASE + endpoint, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify(data)
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || 'Lỗi không xác định');
        }
        return res.json();
    },

    getHeaders() {
        const headers = { 'Content-Type': 'application/json' };
        const token = localStorage.getItem('token');
        if (token) headers['Authorization'] = 'Bearer ' + token;
        return headers;
    }
};
```

**Deploy Frontend lên S3 & CloudFront (bảo mật bằng OAC và WAF):**

```bash
# Tạo S3 Bucket (Block All Public Access mặc định là ON)
aws s3 mb s3://hotel-booking-frontend-2024 --region ap-southeast-1

# Upload toàn bộ file frontend
aws s3 sync ./frontend/ s3://hotel-booking-frontend-2024 --delete

# Các bước tiếp theo (Làm trên AWS Console):
# 1. Tạo CloudFront Distribution trỏ về S3 bucket (REST endpoint)
# 2. Origin Access: Chọn Origin Access Control (OAC) để bảo mật bucket
# 3. Copy Bucket Policy do CloudFront cấp và dán vào S3 Bucket Permissions
# 4. Gắn AWS WAF Web ACL vào CloudFront để chặn SQL Injection, XSS
```

### Phase 5: Triển khai Backend lên EC2 + Cấu hình ALB & Auto Scaling

**Mục tiêu:** .NET API chạy trên EC2, phân tải qua ALB, tự động co giãn với ASG.

**Bước 1: Tạo Launch Template**

1. Tạo 1 EC2 thủ công (loại `t3.micro`, Amazon Linux 2023).

2. SSH vào và chạy các lệnh cài đặt .NET Runtime 8.

3. Tạo file `/etc/systemd/system/hotel-api.service` để chạy .NET API như dịch vụ nền.

4. Tạo Launch Template từ instance này.

**Script cài đặt .NET trên Amazon Linux 2023 (User Data):**

```bash
#!/bin/bash
# User Data Script cho EC2 Launch Template
dnf update -y
dnf install -y aspnetcore-runtime-8.0
dnf install -y aws-cli

mkdir -p /home/ec2-user/app
aws s3 cp s3://hotel-booking-api-bucket/app.zip /home/ec2-user/app/
unzip -o /home/ec2-user/app/app.zip -d /home/ec2-user/app/
rm /home/ec2-user/app/app.zip

cat > /etc/systemd/system/hotel-api.service << 'SERVICEEOF'
[Unit]
Description=Hotel Booking API
After=network.target

[Service]
WorkingDirectory=/home/ec2-user/app
ExecStart=/usr/bin/dotnet /home/ec2-user/app/HotelBookingApi.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hotel-api
User=ec2-user
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection=Server=YOUR_RDS_ENDPOINT;Database=hotel_booking;Uid=admin;Pwd=YOUR_PASSWORD;
Environment=Redis__ConnectionString=YOUR_REDIS_ENDPOINT:6379
Environment=Jwt__Secret=YOUR_SUPER_SECRET_KEY_32_CHAR_MIN

[Install]
WantedBy=multi-user.target
SERVICEEOF

systemctl enable hotel-api
systemctl start hotel-api
```

**Bước 2: Tạo Application Load Balancer**

```bash
aws elbv2 create-target-group \
    --name hotel-api-tg \
    --protocol HTTP \
    --port 8080 \
    --vpc-id vpc-xxxxxxxx \
    --health-check-path /health \
    --health-check-interval-seconds 30 \
    --healthy-threshold-count 2

aws elbv2 create-load-balancer \
    --name hotel-api-alb \
    --subnets subnet-public-a subnet-public-b \
    --security-groups sg-alb-id \
    --scheme internet-facing

aws elbv2 create-listener \
    --load-balancer-arn arn:aws:elasticloadbalancing:... \
    --protocol HTTP \
    --port 80 \
    --default-actions Type=forward,TargetGroupArn=...
```

**Bước 3: Tạo Auto Scaling Group**

```bash
aws ec2 create-launch-template \
    --launch-template-name hotel-api-lt \
    --launch-template-data '{
        "ImageId": "ami-xxxxxxxx",
        "InstanceType": "t3.micro",
        "SecurityGroupIds": ["sg-ec2-id"],
        "UserData": "'$(base64 -w0 user-data.sh)'",
        "IamInstanceProfile": {"Name": "EC2-S3-Access-Role"}
    }'

aws autoscaling create-auto-scaling-group \
    --auto-scaling-group-name hotel-api-asg \
    --launch-template LaunchTemplateName=hotel-api-lt,Version='$Default' \
    --target-group-arns arn:aws:elasticloadbalancing:... \
    --vpc-zone-identifier "subnet-private-a,subnet-private-b" \
    --min-size 1 --max-size 4 --desired-capacity 1 \
    --health-check-type ELB --health-check-grace-period 300

aws autoscaling put-scaling-policy \
    --auto-scaling-group-name hotel-api-asg \
    --policy-name cpu-target-policy \
    --policy-type TargetTrackingScaling \
    --target-tracking-configuration '{
        "TargetValue": 70.0,
        "PredefinedMetricSpecification": {
            "PredefinedMetricType": "ASGAverageCPUUtilization"
        },
        "ScaleInCooldown": 300,
        "ScaleOutCooldown": 120
    }'
```

### Phase 6: Cấu hình Giám sát và Cảnh báo (Monitoring & Alerting)

**Mục tiêu:** Hệ thống tự động gửi email báo cáo khi quá tải hoặc có lỗi.

**Việc cần làm:**

1. Tạo SNS Topic:
   * Vào Amazon SNS → Create topic → Type: Standard.
   * Name: `hotel-api-alerts`.
   * Create Subscription: Chọn Protocol là Email, nhập email của bạn, check mail và click "Confirm subscription".

2. Tạo CloudWatch Alarms:
   * **Báo động Tải cao:** Monitor metric `CPUUtilization` của Auto Scaling Group. Nếu `CPU > 70%` liên tục 2 phút → Gửi thông báo tới SNS Topic `hotel-api-alerts`.
   * **Báo động Lỗi ALB:** Monitor metric `HTTPCode_Target_5XX_Count` của ALB. Nếu lỗi `> 10` trong 1 phút → Gửi cảnh báo.

3. Tạo CloudWatch Dashboard (Dùng để demo báo cáo đồ án):
   * Thêm các widget biểu đồ: CPU trung bình của EC2, Số lượng Request vào ALB, Số kết nối tới RDS, Redis CPU.

## 9. Cấu trúc Code

### 9.1 Cấu trúc thư mục toàn bộ dự án

```text
hotel-booking-aws/
├── src/
│   ├── backend/                  ← ASP.NET Core .NET 8 Project
│   │   ├── HotelBookingApi/
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   ├── Middleware/
│   │   │   ├── appsettings.json
│   │   │   └── Program.cs
│   │   └── HotelBookingApi.sln
│   │
│   └── frontend/                 ← HTML, CSS, JS thuần
│       ├── index.html
│       ├── search.html
│       ├── hotel.html
│       ├── booking.html
│       ├── auth.html
│       ├── my-bookings.html
│       ├── css/
│       │   └── style.css
│       ├── js/
│       │   ├── api.js            ← Các hàm gọi API (dùng Fetch)
│       │   ├── search.js
│       │   ├── booking.js
│       │   └── auth.js
│       └── admin/
│           └── index.html
│
├── infrastructure/               ← AWS CDK (TypeScript) - Hạ tầng dưới dạng code
│   ├── lib/
│   │   ├── network-stack.ts      ← VPC, Subnets, Security Groups
│   │   ├── database-stack.ts     ← RDS MySQL, ElastiCache Redis
│   │   ├── compute-stack.ts      ← EC2, ALB, Auto Scaling Group
│   │   ├── storage-stack.ts      ← S3, CloudFront
│   │   └── monitoring-stack.ts   ← CloudWatch Dashboard, Alarms
│   ├── bin/
│   │   └── app.ts
│   └── package.json
│
├── database/
│   ├── schema.sql                ← Script tạo bảng
│   └── seed-data.sql             ← Script nhập dữ liệu mẫu (khách sạn, phòng)
│
├── load-testing/
│   └── locustfile.py             ← Script Locust để test Auto Scaling
│
└── README.md
```

#### Program.cs — Cấu hình Dependency Injection

```csharp
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(3)
    ));

// Redis
var redisConn = builder.Configuration.GetSection("Redis")["ConnectionString"];
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(new ConfigurationOptions
    {
        EndPoints = { redisConn },
        AbortOnConnectFail = false,
        ConnectTimeout = 5000,
        SyncTimeout = 3000,
        RetryCount = 3
    }));

// Services
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<S3Service>();

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://*.cloudfront.net")
              .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.Run();
```

## 10. Kịch bản Demo

> **Lưu ý:** Đây là phần quyết định điểm số của đề tài. Hãy chuẩn bị và thực hành trước ít nhất 3 lần!

### 10.1 Chuẩn bị trước buổi báo cáo (1 ngày trước)

* Chạy `cdk deploy` để dựng toàn bộ hạ tầng từ đầu, đảm bảo không lỗi.

* Nhập sẵn dữ liệu mẫu: 10 khách sạn, 30 loại phòng với ảnh đẹp.

* Mở sẵn 4 tab trên trình duyệt:

  1. Tab 1: Trang web khách sạn (giao diện người dùng).

  2. Tab 2: AWS Console → EC2 → Auto Scaling Groups.

  3. Tab 3: AWS Console → CloudWatch → Dashboard (biểu đồ CPU).

  4. Tab 4: Terminal chạy lệnh Locust.

### 10.2 Kịch bản Demo (thứ tự trình bày)

**Demo 1: Giới thiệu chức năng cơ bản (~5 phút)**

```text
Mở Tab 1 - Trang web
→ Tìm kiếm phòng tại "Đà Nẵng" từ 20/8 đến 23/8, 2 khách
→ Xem danh sách kết quả
→ Bấm vào 1 khách sạn, xem chi tiết
→ Đặt phòng (Deluxe Room 2 đêm)
→ Màn hình hiện mã đặt phòng: BK20240820-001
→ Vào "Lịch sử đặt phòng" xem đơn vừa tạo
```

**Demo 2: Chứng minh Redis Caching (~3 phút)**

```text
Mở Developer Tools (F12) → Tab Network
→ Thực hiện tìm kiếm lần 1: "Đà Nẵng, 20/8-23/8"
→ Quan sát thời gian phản hồi: ~150ms (Cache MISS, query DB)
→ Thực hiện tìm kiếm CÙNG tiêu chí lần 2
→ Quan sát thời gian phản hồi: ~4ms (Cache HIT, lấy từ Redis)
→ Giải thích: "Kết quả giống nhau được lưu trong Redis 5 phút,
   nên 9.999 người tìm sau đó đều nhận kết quả trong 4ms,
   không đụng đến database."
```

**Demo 3: Auto Scaling (~8 phút - Điểm nhấn của demo)**

```text
Mở Tab 2 và Tab 3 (AWS Console) trên màn hình
→ Hiện tại: 1 EC2 đang chạy, CPU ~15%

Mở Tab 4 (Terminal), chạy:
locust -f locustfile.py --host=http://[ALB-DNS] --users=300 --spawn-rate=30 --headless

→ 300 người dùng ảo bắt đầu tìm kiếm phòng liên tục
→ Chuyển sang Tab 3 (CloudWatch), xem biểu đồ CPU
→ CPU tăng dần: 20% → 45% → 70% → 85%
→ Sau ~2 phút, CloudWatch Alarm kích hoạt (màu đỏ)
→ Chuyển sang Tab 2 (Auto Scaling Group)
   → Xem "Desired capacity" tự động tăng từ 1 → 2
   → Quan sát EC2 mới xuất hiện trong danh sách Instances
→ Sau ~4 phút, EC2 mới "In service"
→ Biểu đồ CPU trên CloudWatch giảm xuống còn ~45%

Dừng Locust (Ctrl+C)
→ CPU giảm dần, sau 10 phút ASG tự động xóa bớt EC2
→ "Hệ thống tự điều chỉnh theo tải, tiết kiệm chi phí khi ít người dùng."
```

**Demo 4: High Availability - Failover (~4 phút)**

```text
Đang có 2 EC2 chạy (sau Demo 3)
→ Vào AWS Console → EC2 Instances
→ Chọn 1 instance, bấm "Terminate" (xác nhận xóa)

Ngay lập tức quay sang trình duyệt trang web
→ Tiếp tục tìm kiếm phòng và đặt phòng bình thường
→ Website KHÔNG bị gián đoạn, không có lỗi nào

Giải thích: "ALB liên tục kiểm tra sức khỏe từng EC2.
Khi EC2 bị terminate, ALB ngay lập tức không gửi request đến đó nữa.
Toàn bộ traffic chuyển sang EC2 còn lại trong vòng dưới 30 giây.
Người dùng không cảm nhận được sự cố."
```

### 10.3 Các câu hỏi thường gặp từ Hội đồng và cách trả lời

**Q: Tại sao dùng ElastiCache Redis mà không cache ngay trong bộ nhớ của EC2 (.NET In-Memory Cache)?**

> A: In-Memory Cache chỉ tồn tại trong 1 instance EC2. Khi có 3-4 EC2 chạy song song (sau Auto Scaling), mỗi máy có cache riêng biệt → Dữ liệu không nhất quán. Người tìm trên EC2-1 thấy phòng trống, người tìm trên EC2-2 cũng thấy phòng trống, cả 2 cùng đặt 1 phòng → Overbooking. ElastiCache Redis là bộ nhớ dùng chung cho tất cả EC2, đảm bảo nhất quán.

**Q: Tại sao không dùng Serverless Lambda thay vì EC2?**

> A: Lambda tốt cho các tác vụ chạy ngắn, không cần giữ trạng thái. Ứng dụng web truyền thống như ASP.NET Core phù hợp hơn với EC2 vì: (1) Lambda có thời gian "khởi động nguội" (cold start) gây trễ; (2) Kết nối database liên tục từ Lambda sẽ tiêu tốn nhiều Connection Pool; (3) Mục tiêu của đề tài này là minh họa kiến trúc HA truyền thống, không phải Serverless.

**Q: Làm thế nào để ngăn 2 người cùng đặt 1 phòng cuối cùng còn lại?**

> A: Sử dụng Database Transaction với cơ chế khóa hàng (Row-Level Locking - SELECT FOR UPDATE). Khi người A bắt đầu đặt phòng, câu lệnh SELECT FOR UPDATE sẽ khóa các bản ghi liên quan. Nếu người B cố gắng đặt cùng phòng cùng lúc, B sẽ phải chờ cho đến khi A hoàn thành hoặc hủy bỏ transaction. Sau đó, B sẽ nhận được thông báo "Phòng đã được đặt".

## 11. Chi phí và Lưu ý

### 11.1 Chi phí ước tính

| Giai đoạn                       | Chi phí ước tính | Ghi chú                              |
| ------------------------------- | ---------------- | ------------------------------------ |
| Phát triển & Code (3-4 tuần)    | **0 $**          | Chỉ chạy local, không dùng AWS       |
| Test hạ tầng & Debug (1-2 tuần) | **~2-5 $**       | Chạy RDS, Redis, EC2 vài tiếng/ngày  |
| Demo báo cáo (1-2 ngày)         | **~1-2 $**       | Chạy đầy đủ hệ thống trong 2-4 tiếng |
| **Tổng cộng**                   | **< 10 $**       | Khoảng 250.000 VNĐ                   |

### 11.2 Checklist cuối ngày khi làm lab

Sau mỗi buổi làm việc, kiểm tra và tắt các dịch vụ sau nếu không cần dùng qua đêm:

* [ ] Stop tất cả EC2 instances (không terminate, chỉ stop để giữ cài đặt).
* [ ] Stop RDS instance (lưu ý RDS mất 3-5 phút để start lại).
* [ ] Xóa ALB nếu không cần (tạo lại nhanh hơn) - ALB tốn ~$0.025/giờ.
* [ ] **KHÔNG cần** xóa VPC, Subnet, Security Group - Những thứ này miễn phí hoàn toàn.
* [ ] **KHÔNG cần** xóa S3 bucket và CloudFront Distribution.
* [ ] Kiểm tra trang Billing trên AWS Console để xác nhận không có gì bất thường.

### 11.3 Quy tắc vàng

> **CẢNH BÁO: KHÔNG BAO GIỜ commit thông tin nhạy cảm lên GitHub: Access Key, Secret Key, mật khẩu database.** Sử dụng biến môi trường hoặc file `.env` (đã được thêm vào `.gitignore`). Bot của hacker luôn quét GitHub tìm các key AWS bị lộ và sẽ dùng tài khoản của bạn để đào Bitcoin, gây ra hóa đơn hàng nghìn USD.

## A — LỊCH TRÌNH THỰC HIỆN (9 tuần)

### Tuần 1: Chuẩn bị môi trường & Tìm hiểu AWS căn bản

**Mục tiêu: Có tài khoản AWS, hiểu VPC/Subnet/Security Group, code được local**

| Ngày | Việc cần làm                                                | Output                                       |
| ---- | ----------------------------------------------------------- | -------------------------------------------- |
| 1-2  | Tạo tài khoản AWS, IAM User + Access Key, cài AWS CLI + CDK | `aws s3 ls` chạy được                        |
| 2-3  | Cài .NET 8 SDK, VS Code/VS2022, tạo project Web API mẫu     | `dotnet run` → Swagger UI                    |
| 3-4  | Học AWS VPC: public/private subnet, IGW, route table, SG    | Vẽ được sơ đồ mạng                           |
| 4-5  | Làm theo hướng dẫn: tạo VPC thủ công trên AWS Console       | 1 VPC + 4 subnet + IGW                       |
| 5-7  | Code backend: Models + DbContext + migration đầu tiên       | `dotnet ef migrations add Initial` chạy được |

**Công cụ cần cài:**

```bash
dotnet --version            # 8.x
aws --version               # 2.x
cdk --version               # 2.x
npm --version               # 20+
dbeaver-ce                  # GUI MySQL
```

### Tuần 2: Database & Cache — triển khai lên AWS

**Mục tiêu: Có RDS MySQL + Redis trên cloud, code kết nối được**

| Ngày | Việc cần làm                                             | Output                                    |
| ---- | -------------------------------------------------------- | ----------------------------------------- |
| 1    | Tạo RDS MySQL `db.t3.micro` + Security Group             | Endpoint: `xxx.rds.amazonaws.com:3306`    |
| 2    | Tạo ElastiCache Redis `cache.t3.micro` + SG              | Endpoint: `xxx.cache.amazonaws.com:6379`  |
| 2-3  | Kết nối DBeaver → RDS (qua EC2 bastion), chạy schema.sql | 4 bảng Hotels, RoomTypes, Users, Bookings |
| 3-4  | Code hoàn chỉnh Controllers + Services                   | API search + booking chạy local → RDS     |
| 4-5  | Test Redis caching: cache hit/miss                       | Response time: ~4ms vs ~150ms             |
| 5-6  | Viết seed data: 10 khách sạn, 30 loại phòng              | Dữ liệu mẫu đẹp, có ảnh                   |
| 6-7  | Tạo README kỹ thuật database schema                      | ERD + giải thích                          |

**Cấu hình appsettings.json mẫu:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=hotel_booking;Uid=root;Pwd=123456;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "dev-secret-key-change-in-production!!",
    "Issuer": "HotelBookingApi",
    "ExpireDays": 7
  }
}
```

### Tuần 3: Backend API — hoàn chỉnh

**Mục tiêu: 100% API endpoints chạy được, có Swagger, có JWT auth**

* Viết AuthController (Register, Login, JWT generation)

* Viết HotelsController (Search + Redis caching, Detail)

* Viết BookingsController (Create — transaction, chống overbooking, My bookings, Cancel)

* Viết AdminController (CRUD hotels, view all bookings)

* Viết Middleware (Exception handling, logging)

* Cấu hình Program.cs (DI, CORS, Swagger, JWT)

### Tuần 4: Frontend + S3/CloudFront

| Ngày | Việc cần làm                                            | Output                        |
| ---- | ------------------------------------------------------- | ----------------------------- |
| 1-2  | Viết `index.html` + `css/style.css`                     | Giao diện tìm kiếm responsive |
| 2-3  | Viết `search.html` + `js/api.js` + `js/search.js`       | Tìm kiếm gọi API thật         |
| 3-4  | Viết `auth.html` + `booking.html` + `confirmation.html` | Đăng nhập + đặt phòng         |
| 4-5  | Viết `hotel.html` (chi tiết) + `my-bookings.html`       | Trang chi tiết + lịch sử      |
| 5-6  | Viết `admin/index.html` + Admin API                     | CRUD khách sạn                |
| 6-7  | Tạo S3 bucket, upload frontend, cấu hình CloudFront     | URL CloudFront hoạt động      |

### Tuần 5: EC2 + ALB — triển khai backend

**Mục tiêu: .NET API chạy trên EC2, phân tải qua ALB**

* Tạo Launch Template với User Data script

* Tạo Target Group + ALB

* Tạo IAM Role cho EC2 (S3 access, SSM, CloudWatch)

* Deploy code lên S3 artifacts bucket

* Kiểm tra health check OK → API hoạt động qua ALB DNS

### Tuần 6: Auto Scaling + Load Testing

**Mục tiêu: ASG hoạt động, Locust chạy được**

* Tạo Auto Scaling Group (Min=1, Max=4, Target CPU=70%)

* Viết locustfile.py mô phỏng 300 users

* Chạy test → Quan sát scale out/in

* Chạy test HA — terminate 1 EC2 → ALB failover

* Đo response time cache hit vs miss

### Tuần 7: CDK — Infrastructure as Code

**Mục tiêu: Toàn bộ hạ tầng AWS được code hóa**

**Cấu trúc CDK:**

```text
infrastructure/
├── bin/app.ts              ← Entry point
├── lib/
│   ├── network-stack.ts    ← VPC, Subnet, SG
│   ├── database-stack.ts   ← RDS, ElastiCache
│   ├── compute-stack.ts    ← ALB, ASG, Launch Template
│   ├── storage-stack.ts    ← S3, CloudFront
│   └── monitoring-stack.ts ← CloudWatch Dashboard, Alarms
├── test/
├── cdk.json
└── package.json
```

**Deploy:**

```bash
cd infrastructure
npm install
cdk bootstrap
cdk synth
cdk deploy --all
```

### Tuần 8: CI/CD + Monitoring — nâng cấp

**Mục tiêu: GitHub Actions tự động deploy, CloudWatch Dashboard**

* Tạo GitHub Actions workflow build + deploy (.NET publish → S3 → SSM)

* Tạo CloudWatch Dashboard (CPU, request count, cache hit/miss, ASG size)

* Tạo CloudWatch Alarm (CPU > 70%)

* Cấu hình CloudWatch Logs Agent trên EC2

* (Optional) Amazon SES gửi email xác nhận đặt phòng

* Tạo IAM Role EC2 (thay vì dùng Access Key)

### Tuần 9: Tổng duyệt + Demo thử + Bảo vệ

| Ngày | Việc cần làm                                                    |
| ---- | --------------------------------------------------------------- |
| 1    | Chạy `cdk deploy —all` từ đầu → kiểm tra toàn bộ hệ thống       |
| 2    | Nhập dữ liệu mẫu, upload ảnh đẹp, kiểm tra API                  |
| 3    | **Demo thử lần 1** — full kịch bản, ghi lại thời gian từng bước |
| 4    | Sửa lỗi phát sinh (nếu có), tối ưu kịch bản                     |
| 5    | **Demo thử lần 2** — mượt, đúng timeline                        |
| 6-7  | Tập trả lời câu hỏi hội đồng, viết script thuyết trình          |
| 8    | **Bảo vệ**                                                      |

## B — CHI TIẾT KỸ THUẬT TỪNG PHẦN

### B.1 Program.cs — Cấu hình Dependency Injection

(Xem code mẫu ở Phase 3)

### B.2 AppDbContext.cs

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Index cho tìm kiếm (performance)
        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.City)
            .HasDatabaseName("IX_Hotels_City");

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingCode)
            .IsUnique()
            .HasDatabaseName("IX_Bookings_Code");

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.RoomTypeId, b.CheckInDate, b.CheckOutDate })
            .HasDatabaseName("IX_Bookings_Room_Dates");

        // Seed data mẫu
        modelBuilder.Entity<Hotel>().HasData(
            new Hotel { HotelId = 1, Name = "Vinpearl Đà Nẵng", City = "Đà Nẵng",
                Address = "Số 1, Ngũ Hành Sơn", StarRating = 5 },
            new Hotel { HotelId = 2, Name = "Mường Thanh Hà Nội", City = "Hà Nội",
                Address = "Số 2, Cầu Giấy", StarRating = 4 },
            new Hotel { HotelId = 3, Name = "Biển Xanh Resort", City = "Nha Trang",
                Address = "Số 3, Trần Phú", StarRating = 5 }
        );
    }
}
```

### B.3 IAM Role cho EC2 (thay vì Access Key)

```bash
# Tạo IAM Role
aws iam create-role --role-name EC2-HotelBooking-Role \
    --assume-role-policy-document '{
        "Version": "2012-10-17",
        "Statement": [{
            "Effect": "Allow",
            "Principal": { "Service": "ec2.amazonaws.com" },
            "Action": "sts:AssumeRole"
        }]
    }'

# Gán policy cho phép truy cập S3, SSM, CloudWatch
aws iam attach-role-policy \
    --role-name EC2-HotelBooking-Role \
    --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess

aws iam attach-role-policy \
    --role-name EC2-HotelBooking-Role \
    --policy-arn arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore

aws iam attach-role-policy \
    --role-name EC2-HotelBooking-Role \
    --policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy
```

### B.4 RDS Backup & Multi-AZ

```bash
# Bật auto backup (giữ 7 ngày)
aws rds modify-db-instance \
    --db-instance-identifier hotel-booking-db \
    --backup-retention-period 7

# Multi-AZ (tốn gấp đôi, nhưng HA thật sự)
aws rds modify-db-instance \
    --db-instance-identifier hotel-booking-db \
    --multi-az --apply-immediately

# Tạo snapshot thủ công trước demo
aws rds create-db-snapshot \
    --db-instance-identifier hotel-booking-db \
    --db-snapshot-identifier hotel-booking-pre-demo
```

### B.5 Xử lý lỗi tập trung (Exception Middleware)

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning("App exception: {Message}", ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict");
            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(
                new { error = "Xin lỗi! Dữ liệu vừa được thay đổi bởi người khác." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(
                new { error = "Lỗi hệ thống. Vui lòng thử lại sau." });
        }
    }
}
```

### B.6 Email xác nhận qua Amazon SES (tuỳ chọn nâng cao)

```csharp
public class EmailService
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly string _fromEmail;

    public EmailService(IConfiguration config)
    {
        _ses = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast1);
        _fromEmail = config["SES:FromEmail"];
    }

    public async Task SendBookingConfirmation(string toEmail, string userName,
        string bookingCode, string hotelName, DateTime checkIn, DateTime checkOut, decimal total)
    {
        var body = $@"
            <h2>Xác nhận đặt phòng</h2>
            <p>Cảm ơn {userName} đã đặt phòng tại {hotelName}.</p>
            <p><strong>Mã đặt phòng:</strong> {bookingCode}</p>
            <p><strong>Nhận phòng:</strong> {checkIn:dd/MM/yyyy}</p>
            <p><strong>Trả phòng:</strong> {checkOut:dd/MM/yyyy}</p>
            <p><strong>Tổng tiền:</strong> {total:N0} VNĐ</p>
            <p>Vui lòng giữ mã đặt phòng để làm thủ tục nhận phòng.</p>";

        var sendRequest = new SendEmailRequest
        {
            Source = _fromEmail,
            Destination = new Destination { ToAddresses = new List<string> { toEmail } },
            Message = new Message
            {
                Subject = new Content($"Xác nhận đặt phòng #{bookingCode}"),
                Body = new Body { Html = new Content(body) }
            }
        };
        await _ses.SendEmailAsync(sendRequest);
    }
}
```

### B.7 Script Locust (load-testing/locustfile.py)

```python
from locust import HttpUser, task, between
import random

class HotelBookingUser(HttpUser):
    wait_time = between(1, 3)

    cities = ["Da Nang", "Ha Noi", "Ho Chi Minh", "Hoi An", "Nha Trang",
              "Da Lat", "Phu Quoc", "Sapa", "Hue", "Ha Long"]

    @task(3)
    def search_hotels(self):
        city = random.choice(self.cities)
        params = {
            "city": city,
            "checkIn": f"2025-{random.randint(6,8):02d}-{random.randint(1,28):02d}",
            "checkOut": f"2025-{random.randint(6,8):02d}-{random.randint(3,30):02d}",
            "guests": random.randint(1, 4)
        }
        self.client.get("/api/hotels/search", params=params)

    @task(1)
    def view_hotel_detail(self):
        hotel_id = random.randint(1, 10)
        self.client.get(f"/api/hotels/{hotel_id}")

    @task(1)
    def login(self):
        self.client.post("/api/auth/login", json={
            "email": "test@example.com",
            "password": "Test@123"
        })
```

**Chạy Locust:**

```bash
pip install locust

# Headless (300 users, tăng 30 users/giây, chạy 15 phút)
locust -f load-testing/locustfile.py \
    --host=http://hotel-api-alb-xxxx.elb.amazonaws.com \
    --users=300 --spawn-rate=30 --headless --run-time=15m

# Web UI (mở http://localhost:8089)
locust -f load-testing/locustfile.py \
    --host=http://hotel-api-alb-xxxx.elb.amazonaws.com
```

### B.8 Script khởi động nhanh trước demo (warmup.sh)

```bash
#!/bin/bash
# warmup.sh — chạy trước demo 30 phút

# 1. Start RDS
aws rds start-db-instance --db-instance-identifier hotel-booking-db

# 2. Start EC2 (qua ASG — set desired capacity = 1)
aws autoscaling set-desired-capacity \
    --auto-scaling-group-name hotel-api-asg \
    --desired-capacity 1

# 3. Đợi health check OK
echo "Đợi EC2 ready..."
aws elbv2 describe-target-health \
    --target-group-arn arn:aws:elasticloadbalancing:... \
    --query 'TargetHealthDescriptions[0].TargetHealth.State'

# 4. Warm up Redis — chạy search mẫu
for city in "Đà Nẵng" "Hà Nội" "Hồ Chí Minh" "Nha Trang"; do
    curl -s "http://ALB-DNS/api/hotels/search?city=$city&checkIn=2025-08-15&checkOut=2025-08-18&guests=2" > /dev/null
done
echo "Warmup complete — Redis cache ready!"
```

## C — NHỮNG ĐIỂM CÒN THIẾU & BỔ SUNG

### So sánh: Plan gốc vs Plan bổ sung

| Khoản             | Plan gốc          | Plan bổ sung                   |
| ----------------- | ----------------- | ------------------------------ |
| CI/CD             | ❌ Không có        | ✅ GitHub Actions → S3 → SSM    |
| Database Multi-AZ | ❌ Single-AZ       | ✅ Cung cấp option (tốn thêm $) |
| Logging           | ❌ Không           | ✅ CloudWatch Logs Agent        |
| Dashboard         | ❌ Không           | ✅ CloudWatch Dashboard         |
| IAM Role          | ❌ Dùng access key | ✅ EC2 IAM Role                 |
| Session           | ❌ Không           | ✅ JWT                          |
| Email confirm     | ❌ Không           | ✅ Amazon SES (tuỳ chọn)        |
| Backup DB         | ❌ Không           | ✅ RDS snapshot auto            |
| CDK               | ✅ CDK             | ✅ Có code mẫu từng stack       |
| Error handling    | ❌ Không           | ✅ Middleware + retry pattern   |
| Cost optimization | ⚠️ Cơ bản         | ✅ Chi tiết + auto stop script  |

## D — CHECKLIST TUẦN CUỐI & DỰ PHÒNG SỰ CỐ

### Checklist 3 ngày trước bảo vệ

* [ ] Deploy từ đầu bằng `cdk deploy —all` — xác nhận không lỗi
* [ ] Seed dữ liệu mẫu — 10+ khách sạn, 30+ loại phòng, ảnh đẹp
* [ ] Test frontend → CloudFront → ALB → EC2 → RDS — đầy đủ flow
* [ ] Test Redis caching — response time cache hit < 10ms
* [ ] Test Auto Scaling — Locust 300 users → EC2 tự scale lên 2-3
* [ ] Test HA — terminate 1 EC2 → web vẫn chạy
* [ ] Tắt EC2/RDS qua đêm để tiết kiệm chi phí
* [ ] Tạo AWS Budget alarm $5 → email
* [ ] Chuẩn bị slide + script thuyết trình
* [ ] **Demo thử full 3 lần — có bấm giờ**

### Dự phòng sự cố (Disaster Recovery)

| Sự cố                            | Nguyên nhân               | Cách xử lý                                                          |
| -------------------------------- | ------------------------- | ------------------------------------------------------------------- |
| **RDS không start được**         | Stop lâu, MySQL lỗi       | Restore từ snapshot: `aws rds restore-db-instance-from-db-snapshot` |
| **EC2 không cài được .NET**      | User Data lỗi             | Tạo AMI mới từ EC2 đã cài sẵn, update Launch Template               |
| **Quên mật khẩu DB**             | Không ghi lại             | AWS Console → RDS → Modify → Reset master password                  |
| **ALB health check fail**        | API lỗi, port sai         | SSH vào EC2 kiểm tra `systemctl status hotel-api`                   |
| **Locust không chạy**            | Python chưa cài, sai host | Kiểm tra: `pip install locust`, dùng đúng ALB DNS                   |
| **Hết Free Tier**                | Tạo instance không đúng   | Kiểm tra billing, stop ngay, dùng đúng `t3.micro`                   |
| **Demo chậm**                    | Cold start EC2/RDS        | Start trước 30 phút, warm up cache                                  |
| **Auto Scaling không hoạt động** | Alarm sai metric          | Check CloudWatch Alarm → ASG Activity History                       |
| **Frontend không gọi được API**  | CORS sai endpoint         | Kiểm tra API_BASE trong api.js, CORS config                         |

> **Cảm ơn bạn đã đọc!**\
> Tài liệu này bao gồm toàn bộ kiến thức và kế hoạch để xây dựng hệ thống đặt phòng khách sạn chịu tải cao trên AWS. Hãy thực hành từng bước, đừng bỏ qua phần demo — đó là chìa khóa để đạt điểm cao.
