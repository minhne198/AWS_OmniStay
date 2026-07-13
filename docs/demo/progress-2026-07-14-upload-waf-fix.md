# OmniStay upload/WAF fix progress - 2026-07-14

File nay ghi lai tien do sua loi upload anh tren CloudFront/AWS. Khong ghi password, JWT secret, access key, RDS password, token hoac thong tin nhay cam vao file nay.

## Tom tat ngan

Da xu ly chuoi loi upload anh tren public web:

- Ban dau upload anh tu may tinh bi `HTTP 403`.
- Da xac dinh request upload bi CloudFront/WAF chan truoc khi toi backend.
- Da them custom WAF allow rule cho endpoint upload anh.
- Sau khi WAF qua duoc, loi chuyen thanh `HTTP 400`.
- Da sua frontend de nen anh ve JPG truoc khi upload.
- Da sua backend de nhan them dinh dang anh va tang gioi han upload.
- Da tao artifact backend/frontend moi de deploy lai AWS.

## AWS resources lien quan

```text
CloudFront distribution id: E25IVE7NGAHBJP
CloudFront domain: d1gm8xt8e0mar4.cloudfront.net
WAF/protection pack: CreatedByCloudFront-c7a2b884
Frontend bucket: omnistay-frontend-484504929670
Backend artifact bucket: omnistay-artifacts-306656769227-new
Backend artifact object: omnistay-api.zip
EC2 app path: /opt/omnistay/api
Service name: omnistay-api
```

## Loi da thay

### 1. Upload anh bi CloudFront/WAF chan

Trieu chung tren UI:

```text
May chu tu choi thao tac nay (HTTP 403).
```

Kiem tra bang curl voi file gia 20 KB:

```text
POST https://d1gm8xt8e0mar4.cloudfront.net/api/uploads/images
Result: HTTP/1.1 403 Forbidden
Server: CloudFront
X-Cache: Error from cloudfront
Body: Request blocked.
```

Ket luan: request upload multipart/form-data bi WAF/CloudFront chan truoc backend.

### 2. Upload qua duoc WAF nhung backend tra 400

Sau khi them WAF allow rule, upload khong token tra dung:

```text
HTTP/1.1 401 Unauthorized
Server: Kestrel
```

Ket luan: request da toi backend. Loi UI `HTTP 400` con lai la do backend/du lieu upload, khong phai WAF.

## WAF thay doi da lam

Da them custom rule:

```text
Rule name: AllowOmniStayImageUpload
Action: Allow
Condition:
  URI path starts with /api/uploads/images
  AND HTTP method exactly matches POST
```

Can dam bao rule nay dung tren cac AWS managed rules:

```text
AllowOmniStayImageUpload
AWS-AWSManagedRulesAmazonIpReputationList
AWS-AWSManagedRulesCommonRuleSet
AWS-AWSManagedRulesKnownBadInputsRuleSet
```

Neu endpoint cu con duoc goi o dau do, co the them rule tuong tu:

```text
URI path starts with /api/admin/uploads/images
HTTP method exactly matches POST
Action: Allow
```

## Code thay doi chinh

### Backend

File:

```text
backend/src/HotelBooking.Api/Controllers/UploadsController.cs
```

Thay doi:

- Them route upload moi `/api/uploads/images`, van giu route cu `/api/admin/uploads/images`.
- Tang gioi han upload tu 5 MB len 20 MB.
- Nhan them dinh dang:

```text
jpg, jpeg, jfif, png, webp, gif, bmp, avif
```

- Tra loi 400 ro hon khi file qua lon hoac dinh dang khong ho tro.

### Frontend

File:

```text
frontend/src/api.js
```

Thay doi:

- Truoc khi upload, file anh duoc doc bang browser canvas.
- Anh lon duoc resize toi canh dai toi da 1600 px.
- Anh duoc nen thanh JPEG voi quality 0.84.
- GIF duoc giu nguyen de tranh mat animation.
- HEIC/HEIF bao loi ro: can doi sang JPG/PNG.
- Parser loi API nhan ca `application/problem+json`, nen UI khong con chi hien moi `HTTP 400`.

### Cache busting

Frontend public HTML da doi cache-buster:

```text
20260713-upload400fix
```

## Artifacts moi da tao

Backend:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
D:\AWS_OmniStay\artifacts\omnistay-api-20260713-upload400fix.zip
```

Frontend:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260713-upload400fix
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260713-upload400fix.zip
```

Frontend folder dung de upload len S3 la folder nay:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260713-upload400fix
```

Khi upload len S3, upload cac folder con vao root bucket:

```text
public/
src/
assets/
```

Khong upload nguyen folder boc ngoai `omnistay-frontend-20260713-upload400fix/`.

## Verification da chay

Frontend syntax:

```text
node --check frontend/src/api.js
Result: OK
```

Backend tests:

```text
dotnet test backend/tests/HotelBooking.Api.Tests/HotelBooking.Api.Tests.csproj -c Release
Result: Passed - 22/22
```

Backend artifact structure:

```text
artifacts/omnistay-api.zip
Top-level contains HotelBooking.Api.dll, appsettings.json, runtimes/, ...
```

Frontend artifact structure:

```text
artifacts/omnistay-frontend-20260713-upload400fix.zip
Top-level contains public/, src/, assets/
```

## Deploy lai AWS

### 1. Upload backend artifact

Upload:

```text
D:\AWS_OmniStay\artifacts\omnistay-api.zip
```

Len:

```text
s3://omnistay-artifacts-306656769227-new/omnistay-api.zip
```

### 2. Restart backend tren EC2

```bash
cd /opt/omnistay
sudo aws s3 cp s3://omnistay-artifacts-306656769227-new/omnistay-api.zip /opt/omnistay/omnistay-api.zip
sudo systemctl stop omnistay-api
sudo rm -rf /opt/omnistay/api/*
sudo unzip -o /opt/omnistay/omnistay-api.zip -d /opt/omnistay/api
sudo systemctl start omnistay-api
sudo systemctl status omnistay-api --no-pager
curl -i http://localhost:8080/health
```

### 3. Upload frontend

Upload cac folder con:

```text
public/
src/
assets/
```

Tu:

```text
D:\AWS_OmniStay\artifacts\omnistay-frontend-20260713-upload400fix
```

Len root bucket:

```text
omnistay-frontend-484504929670
```

### 4. CloudFront invalidation

```text
/*
```

## Test lai sau deploy

1. Mo:

```text
https://d1gm8xt8e0mar4.cloudfront.net/public/profile.html
```

2. Ctrl+F5.
3. Dang xuat/dang nhap lai neu token cu bat thuong.
4. Chon anh JPG/PNG tu may tinh.
5. Ky vong:

```text
Upload avatar thanh cong. Bam cap nhat ho so de luu.
```

6. Bam `Cap nhat ho so`.
7. Test tiep:

```text
Admin/Quan ly -> tao khach san co anh
Admin/Quan ly -> tao phong co anh
```

## Neu van loi

Neu lai `403`:

- Kiem rule `AllowOmniStayImageUpload` nam tren cac AWS managed rules.
- Kiem rule co path `/api/uploads/images` va action `Allow`.
- Kiem them rule cho `/api/admin/uploads/images` neu frontend cu van goi endpoint cu.

Neu lai `400`:

- Thu anh JPG/PNG nho hon 20 MB.
- Kiem response body trong DevTools Network de xem message chi tiet.
- Kiem log backend:

```bash
sudo journalctl -u omnistay-api --since "10 minutes ago" --no-pager
```

Neu upload thanh cong nhung anh khong hien:

- Mo URL anh tra ve, vi du:

```text
https://d1gm8xt8e0mar4.cloudfront.net/api/uploads/images/<file-name>.jpg
```

- Neu 404 sau khi restart/replace EC2, day la gioi han hien tai: uploaded file dang luu tren local filesystem cua EC2. Ve dai han nen chuyen upload anh sang S3/object storage.

## Viec nen lam sau

- Chuyen uploaded images sang S3 thay vi local disk tren EC2.
- Cap nhat Launch Template/user data neu backend artifact nay can thanh ban mac dinh cho instance moi.
- Bat WAF sampled requests/logging de debug nhanh rule nao block.
- Sau demo, tinh lai AWS WAF pricing vi man hinh dang hien CloudFront flat-rate plan Pro.
