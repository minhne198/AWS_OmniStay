# AWS OmniStay - Huong Dan Tao AWS Services Tung Buoc

Tai lieu nay dung de tao san cac dich vu AWS tren Console va chup anh bang chung cho do an **AWS OmniStay**. Muc tieu hien tai la tao resource thanh cong va chup anh man hinh tren AWS Console, chua bat buoc deploy ung dung chay end-to-end.

> **Luu y chi phi:** NAT Gateway, ALB, RDS, ElastiCache va public IPv4 co the tinh phi theo gio. Sau khi chup anh xong, lam phan **Cleanup cuoi buoi** o cuoi tai lieu.

## 0. Quy Uoc Chung

| Hang muc | Gia tri |
| --- | --- |
| Region | `Asia Pacific (Singapore) ap-southeast-1` |
| Project tag | `AWS_OmniStay` |
| Environment tag | `demo` |
| Owner tag | `minhne198` |
| VPC CIDR | `10.0.0.0/16` |

Tag nen gan cho moi resource:

| Key | Value |
| --- | --- |
| `Project` | `AWS_OmniStay` |
| `Environment` | `demo` |
| `Owner` | `minhne198` |

Quy tac chup anh:

- Khong chup lo password, secret value, access key, token.
- Tao xong dich vu nao thi chup ngay trang summary/details cua dich vu do.
- Neu co bang list resource, chup them cot Name, Status, VPC/Subnet/Security Group.
- Neu co endpoint, chi chup endpoint/port, khong chup mat khau.

## Checklist Tong Quan

- [ ] AWS Budget
- [ ] VPC
- [ ] 6 Subnets
- [ ] Internet Gateway
- [ ] Public route table
- [ ] 2 NAT Gateways
- [ ] Private app route tables
- [ ] Security Groups: `SG-ALB`, `SG-EC2`, `SG-RDS`, `SG-Redis`
- [ ] RDS MySQL
- [ ] ElastiCache Valkey/Redis
- [ ] IAM role cho EC2
- [ ] Secrets Manager va Parameter Store
- [ ] S3 buckets
- [ ] Target Group
- [ ] Application Load Balancer
- [ ] Launch Template
- [ ] Auto Scaling Group
- [ ] CloudFront OAC + Distribution
- [ ] AWS WAF
- [ ] SNS Topic
- [ ] CloudWatch Dashboard + Alarms

## 1. Tao AWS Budget

Di den **Billing and Cost Management** -> **Budgets** -> **Create budget**.

| Truong | Gia tri |
| --- | --- |
| Budget type | Monthly cost budget |
| Budget name | `omnistay-monthly-budget` |
| Budget amount | `10 USD` |
| Alert 1 | 50% |
| Alert 2 | 80% |
| Alert 3 | 100% |
| Email | Email cua anh |

Cac buoc:

1. Chon template **Monthly cost budget** neu Console co template.
2. Nhap budget amount `10 USD`.
3. Them email nhan canh bao.
4. Create budget.

Anh can chup:

- Trang Budget summary.
- Phan alert thresholds va email recipient.

## 2. Tao VPC

Di den **VPC** -> **Your VPCs** -> **Create VPC**.

| Truong | Gia tri |
| --- | --- |
| Resource to create | VPC only |
| Name tag | `omnistay-vpc` |
| IPv4 CIDR | `10.0.0.0/16` |
| Tenancy | Default |
| VPC encryption control | None |

Tags:

| Key | Value |
| --- | --- |
| `Name` | `omnistay-vpc` |
| `Project` | `AWS_OmniStay` |
| `Environment` | `demo` |
| `Owner` | `minhne198` |

Anh can chup:

- VPC details co Name va CIDR `10.0.0.0/16`.
- VPC ID de dung lai cho cac buoc tiep theo.

## 3. Tao 6 Subnets

Di den **VPC** -> **Subnets** -> **Create subnet**. Tat ca subnet deu nam trong `omnistay-vpc`.

| Subnet name | AZ | IPv4 CIDR | Vai tro |
| --- | --- | --- | --- |
| `omnistay-public-a` | `ap-southeast-1a` | `10.0.1.0/24` | Public subnet cho ALB/NAT |
| `omnistay-public-b` | `ap-southeast-1b` | `10.0.2.0/24` | Public subnet cho ALB/NAT |
| `omnistay-app-a` | `ap-southeast-1a` | `10.0.10.0/24` | Private app subnet cho EC2 |
| `omnistay-app-b` | `ap-southeast-1b` | `10.0.11.0/24` | Private app subnet cho EC2 |
| `omnistay-data-a` | `ap-southeast-1a` | `10.0.20.0/24` | Private data subnet cho RDS/Redis |
| `omnistay-data-b` | `ap-southeast-1b` | `10.0.21.0/24` | Private data subnet cho RDS/Redis |

Sau khi tao xong 2 public subnet:

1. Chon `omnistay-public-a`.
2. Actions -> **Edit subnet settings**.
3. Bat **Auto-assign public IPv4 address**.
4. Lam tuong tu voi `omnistay-public-b`.

Anh can chup:

- Danh sach 6 subnets.
- Cot VPC, AZ, CIDR.
- Public subnet settings co auto-assign public IPv4 neu Console hien.

## 4. Tao Internet Gateway

Di den **VPC** -> **Internet Gateways** -> **Create internet gateway**.

| Truong | Gia tri |
| --- | --- |
| Name tag | `omnistay-igw` |
| Attach VPC | `omnistay-vpc` |

Cac buoc:

1. Create internet gateway voi ten `omnistay-igw`.
2. Chon IGW vua tao.
3. Actions -> **Attach to VPC**.
4. Chon `omnistay-vpc`.

Anh can chup:

- Internet Gateway co state attached.
- VPC attachment la `omnistay-vpc`.

## 5. Tao Public Route Table

Di den **VPC** -> **Route tables** -> **Create route table**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-public-rt` |
| VPC | `omnistay-vpc` |

Routes:

| Destination | Target |
| --- | --- |
| `10.0.0.0/16` | local |
| `0.0.0.0/0` | `omnistay-igw` |

Subnet associations:

- `omnistay-public-a`
- `omnistay-public-b`

Cac buoc:

1. Tao route table `omnistay-public-rt`.
2. Vao tab **Routes** -> **Edit routes**.
3. Add route `0.0.0.0/0` -> Internet Gateway `omnistay-igw`.
4. Save changes.
5. Vao tab **Subnet associations** -> **Edit subnet associations**.
6. Tick `omnistay-public-a` va `omnistay-public-b`.
7. Save associations.

Anh can chup:

- Route `0.0.0.0/0 -> igw-...`.
- Subnet associations co 2 public subnets.

## 6. Tao 2 NAT Gateways

Di den **VPC** -> **NAT Gateways** -> **Create NAT gateway**.

Tao NAT Gateway thu nhat:

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-nat-a` |
| Availability mode | Zonal |
| Subnet | `omnistay-public-a` |
| Connectivity type | Public |
| Elastic IP allocation | Automatic |

Tao NAT Gateway thu hai:

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-nat-b` |
| Availability mode | Zonal |
| Subnet | `omnistay-public-b` |
| Connectivity type | Public |
| Elastic IP allocation | Automatic |

> Console moi co the mac dinh **Regional**. Neu khong thay cho chon subnet, doi sang **Zonal**.

Anh can chup:

- NAT list co `omnistay-nat-a` va `omnistay-nat-b`.
- Status `Available`.
- Subnet va Elastic IP cua tung NAT.

## 7. Tao Private App Route Tables

### Route table cho app subnet AZ-a

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-app-a-rt` |
| VPC | `omnistay-vpc` |
| Route | `0.0.0.0/0 -> omnistay-nat-a` |
| Subnet association | `omnistay-app-a` |

### Route table cho app subnet AZ-b

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-app-b-rt` |
| VPC | `omnistay-vpc` |
| Route | `0.0.0.0/0 -> omnistay-nat-b` |
| Subnet association | `omnistay-app-b` |

Anh can chup:

- `omnistay-app-a-rt` route ra NAT-a.
- `omnistay-app-b-rt` route ra NAT-b.
- Subnet associations cua tung route table.

## 8. Tao Security Groups

Di den **EC2** -> **Security Groups** -> **Create security group**.

### 8.1. SG-ALB

Basic details:

| Truong | Gia tri |
| --- | --- |
| Security group name | `SG-ALB` |
| Description | `Allow public HTTP to ALB` |
| VPC | `omnistay-vpc` |

Inbound:

| Type | Port | Source | Description |
| --- | --- | --- | --- |
| HTTP | 80 | Anywhere-IPv4 `0.0.0.0/0` | `Allow public HTTP to ALB` |

Outbound:

| Type | Destination |
| --- | --- |
| All traffic | `0.0.0.0/0` |

### 8.2. SG-EC2

Basic details:

| Truong | Gia tri |
| --- | --- |
| Security group name | `SG-EC2` |
| Description | `Allow ALB to API instances` |
| VPC | `omnistay-vpc` |

Inbound:

| Type | Port | Source | Description |
| --- | --- | --- | --- |
| Custom TCP | 8080 | Custom: `SG-ALB` | `Allow ALB to API instances` |

### 8.3. SG-RDS

Basic details:

| Truong | Gia tri |
| --- | --- |
| Security group name | `SG-RDS` |
| Description | `Allow EC2 to MySQL` |
| VPC | `omnistay-vpc` |

Inbound:

| Type | Port | Source | Description |
| --- | --- | --- | --- |
| MySQL/Aurora | 3306 | Custom: `SG-EC2` | `Allow EC2 to RDS MySQL` |

### 8.4. SG-Redis

Basic details:

| Truong | Gia tri |
| --- | --- |
| Security group name | `SG-Redis` |
| Description | `Allow EC2 to Redis` |
| VPC | `omnistay-vpc` |

Inbound:

| Type | Port | Source | Description |
| --- | --- | --- | --- |
| Custom TCP | 6379 | Custom: `SG-EC2` | `Allow EC2 to Redis cache` |

Anh can chup:

- Inbound rules cua ca 4 security groups.
- Anh chup ro `SG-RDS` va `SG-Redis` chi cho source `SG-EC2`, khong phai `0.0.0.0/0`.

## 9. Tao RDS MySQL

### 9.1. Tao DB Subnet Group

Di den **RDS** -> **Subnet groups** -> **Create DB subnet group**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-db-subnet-group` |
| Description | `Private data subnets for OmniStay RDS` |
| VPC | `omnistay-vpc` |
| AZs | 2 AZ dang dung |
| Subnets | `omnistay-data-a`, `omnistay-data-b` |

### 9.2. Tao Database

Di den **RDS** -> **Databases** -> **Create database**.

| Truong | Gia tri |
| --- | --- |
| Creation method | Standard create |
| Engine | MySQL |
| Template | Free tier neu co, neu khong chon Dev/Test |
| DB instance identifier | `omnistay-mysql` |
| Master username | `admin` |
| Master password | Tao mat khau manh, luu rieng |
| Instance class | Loai nho nhat kha dung |
| Storage | `20 GiB` |
| VPC | `omnistay-vpc` |
| DB subnet group | `omnistay-db-subnet-group` |
| Public access | No |
| Security group | `SG-RDS` |
| Initial database name | `hotel_booking` |
| Backup retention | 1-7 ngay |

Anh can chup:

- Database status `Available`.
- Connectivity & security: endpoint, port, VPC, subnet group, security group.
- Configuration: engine MySQL, instance class, storage.

## 10. Tao ElastiCache Valkey/Redis

### 10.1. Tao Cache Subnet Group

Di den **ElastiCache** -> **Subnet groups** -> **Create subnet group**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-cache-subnet-group` |
| VPC | `omnistay-vpc` |
| Subnets | `omnistay-data-a`, `omnistay-data-b` |

### 10.2. Tao Cluster

Di den **ElastiCache** -> **Create cluster**.

| Truong | Gia tri |
| --- | --- |
| Engine | Valkey hoac Redis OSS |
| Name | `omnistay-cache` |
| Cluster mode | Disabled |
| Node type | Loai nho nhat phu hop ngan sach |
| Replicas | 0 cho demo tiet kiem |
| Port | 6379 |
| VPC | `omnistay-vpc` |
| Subnet group | `omnistay-cache-subnet-group` |
| Security group | `SG-Redis` |

Anh can chup:

- Cluster status `Available`.
- Endpoint va port.
- Subnet group va security group.

## 11. Tao IAM Role Cho EC2

Di den **IAM** -> **Roles** -> **Create role**.

| Truong | Gia tri |
| --- | --- |
| Trusted entity | AWS service |
| Use case | EC2 |
| Role name | `EC2-HotelAPI-Role` |

Policies can gan:

- `AmazonSSMManagedInstanceCore`
- `CloudWatchAgentServerPolicy`
- `AmazonS3ReadOnlyAccess`

Anh can chup:

- Role summary.
- Attached policies.
- Trust relationship cho EC2.

## 12. Tao Secrets Manager Va Parameter Store

### 12.1. Secrets Manager

Di den **Secrets Manager** -> **Store a new secret**.

| Truong | Gia tri |
| --- | --- |
| Secret type | Other type of secret |
| Secret name | `omnistay/prod/hotelbookingdb` |

Key/value goi y:

| Key | Value |
| --- | --- |
| `username` | `admin` |
| `password` | RDS password, khong chup lo |
| `host` | RDS endpoint |
| `port` | `3306` |
| `database` | `hotel_booking` |

Anh can chup:

- Secret name va metadata.
- Khong mo secret value khi chup.

### 12.2. Parameter Store

Di den **Systems Manager** -> **Parameter Store** -> **Create parameter**.

Tao cac parameter:

| Name | Type | Value |
| --- | --- | --- |
| `/omnistay/prod/Database__Provider` | String | `MySql` |
| `/omnistay/prod/AWS__Region` | String | `ap-southeast-1` |
| `/omnistay/prod/Cache__SearchTtlSeconds` | String | `120` |
| `/omnistay/prod/AWS__S3FrontendBucket` | String | `<bucket-name>` |
| `/omnistay/prod/AWS__CloudFrontDomain` | String | `<cloudfront-domain>` |

Anh can chup:

- Parameter list voi prefix `/omnistay/prod`.
- Khong chup value nhay cam neu sau nay them connection string.

## 13. Tao S3 Buckets

Di den **S3** -> **Create bucket**.

### 13.1. Frontend bucket

| Truong | Gia tri |
| --- | --- |
| Bucket name | `omnistay-frontend-<account-id>` |
| Region | `ap-southeast-1` |
| Object ownership | Bucket owner enforced |
| Block Public Access | On tat ca |
| Encryption | SSE-S3 |

### 13.2. Artifact bucket

| Truong | Gia tri |
| --- | --- |
| Bucket name | `omnistay-artifacts-<account-id>` |
| Region | `ap-southeast-1` |
| Object ownership | Bucket owner enforced |
| Block Public Access | On tat ca |
| Encryption | SSE-S3 |

Anh can chup:

- Bucket list co 2 bucket.
- Bucket Properties.
- Permissions co Block Public Access ON.

## 14. Tao Target Group

Di den **EC2** -> **Target Groups** -> **Create target group**.

| Truong | Gia tri |
| --- | --- |
| Target type | Instances |
| Target group name | `omnistay-api-tg` |
| Protocol | HTTP |
| Port | 8080 |
| VPC | `omnistay-vpc` |
| Health check protocol | HTTP |
| Health check path | `/health` |
| Success code | `200` |

Neu chua tao EC2 thi chua can register target.

Anh can chup:

- Target group details.
- Health check path `/health`.

## 15. Tao Application Load Balancer

Di den **EC2** -> **Load Balancers** -> **Create load balancer** -> **Application Load Balancer**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-alb` |
| Scheme | Internet-facing |
| IP address type | IPv4 |
| VPC | `omnistay-vpc` |
| Subnets | `omnistay-public-a`, `omnistay-public-b` |
| Security group | `SG-ALB` |
| Listener | HTTP 80 |
| Default action | Forward to `omnistay-api-tg` |

Anh can chup:

- ALB state `Active`.
- DNS name.
- Listener HTTP 80.
- Security group `SG-ALB`.

## 16. Tao Launch Template

Di den **EC2** -> **Launch Templates** -> **Create launch template**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-api-template` |
| Description | `Initial API template` |
| AMI | Amazon Linux 2023 |
| Instance type | `t3.micro` hoac loai nho nhat kha dung |
| Key pair | Co the bo qua neu chua can SSH |
| Security group | `SG-EC2` |
| IAM instance profile | `EC2-HotelAPI-Role` |
| Storage | 8-20 GiB |
| User data | De trong trong giai doan chup service |

Anh can chup:

- Launch template summary.
- AMI, instance type.
- IAM role va security group.

## 17. Tao Auto Scaling Group

Di den **EC2** -> **Auto Scaling Groups** -> **Create Auto Scaling group**.

| Truong | Gia tri |
| --- | --- |
| Name | `omnistay-api-asg` |
| Launch template | `omnistay-api-template` |
| VPC | `omnistay-vpc` |
| Subnets | `omnistay-app-a`, `omnistay-app-b` |
| Target group | `omnistay-api-tg` |
| ELB health check | Enable neu Console cho chon |
| Health check grace period | 300 seconds |
| Desired capacity | 0 |
| Minimum capacity | 0 |
| Maximum capacity | 4 |
| Scaling policy | Target tracking |
| Metric | Average CPU utilization |
| Target value | 70 |

> De chup service truoc, de desired/min = 0 de chua tao EC2 va tiet kiem chi phi. Khi demo app that thi doi desired/min ve 1.

Anh can chup:

- ASG details.
- Desired/min/max capacity.
- Attached target group.
- Scaling policy CPU 70%.

## 18. Tao CloudFront OAC Va Distribution

### 18.1. Tao Distribution

Di den **CloudFront** -> **Create distribution**.

| Truong | Gia tri |
| --- | --- |
| Origin domain | S3 frontend bucket |
| Origin access | Origin Access Control settings |
| OAC name | `omnistay-frontend-oac` |
| Signing behavior | Sign requests |
| Viewer protocol policy | Redirect HTTP to HTTPS |
| Allowed methods | GET, HEAD |
| Default root object | `public/index.html` |

Sau khi tao distribution, CloudFront se goi y S3 bucket policy. Copy policy nay.

### 18.2. Gan Bucket Policy Cho S3

1. Vao S3 -> frontend bucket.
2. Permissions -> Bucket policy.
3. Paste policy CloudFront goi y.
4. Save.

### 18.3. Them ALB Origin Va Behavior `/api/*`

Trong CloudFront distribution vua tao:

| Muc | Gia tri |
| --- | --- |
| New origin domain | ALB DNS name |
| Origin name | `omnistay-alb-origin` |
| Protocol | HTTP only |
| Behavior path pattern | `/api/*` |
| Origin for behavior | `omnistay-alb-origin` |
| Cache policy | CachingDisabled neu co |

Anh can chup:

- Distribution domain name.
- Origins: S3 va ALB.
- Behaviors: default va `/api/*`.
- OAC.
- S3 bucket policy.

## 19. Tao AWS WAF Cho CloudFront

Di den **WAF & Shield** -> **Web ACLs** -> **Create web ACL**.

| Truong | Gia tri |
| --- | --- |
| Resource type | CloudFront distributions |
| Scope | Global |
| Name | `omnistay-cloudfront-waf` |
| Associated resource | CloudFront distribution |
| Default action | Allow |

Managed rule groups nen them:

- `AWSManagedRulesCommonRuleSet`
- `AWSManagedRulesKnownBadInputsRuleSet`
- `AWSManagedRulesSQLiRuleSet`
- `AmazonIpReputationList`

Anh can chup:

- Web ACL overview.
- Associated CloudFront distribution.
- Rule list.

## 20. Tao SNS Topic

Di den **SNS** -> **Topics** -> **Create topic**.

| Truong | Gia tri |
| --- | --- |
| Type | Standard |
| Name | `omnistay-alerts` |
| Subscription protocol | Email |
| Endpoint | Email cua anh |

Cac buoc:

1. Tao topic `omnistay-alerts`.
2. Create subscription.
3. Chon Protocol = Email.
4. Nhap email.
5. Mo email va confirm subscription.

Anh can chup:

- Topic summary.
- Subscription status confirmed.

## 21. Tao CloudWatch Dashboard

Di den **CloudWatch** -> **Dashboards** -> **Create dashboard**.

| Truong | Gia tri |
| --- | --- |
| Dashboard name | `OmniStay-Demo` |

Widgets nen them:

- ALB `RequestCount`.
- ALB `HTTPCode_Target_5XX_Count`.
- ASG `GroupDesiredCapacity`.
- ASG `GroupInServiceInstances`.
- EC2 `CPUUtilization`.
- RDS `CPUUtilization`.
- RDS `DatabaseConnections`.
- ElastiCache `CPUUtilization`.
- ElastiCache `CurrConnections`.

Anh can chup:

- Dashboard overview.
- Cac widget da add.

## 22. Tao CloudWatch Alarms

### 22.1. ASG CPU High

| Truong | Gia tri |
| --- | --- |
| Metric | ASG CPUUtilization |
| Resource | `omnistay-api-asg` |
| Condition | Greater than 70 |
| Notification | `omnistay-alerts` |
| Alarm name | `omnistay-asg-cpu-high` |

### 22.2. ALB 5XX High

| Truong | Gia tri |
| --- | --- |
| Metric | `HTTPCode_Target_5XX_Count` |
| Resource | `omnistay-alb` |
| Condition | Greater than 10 |
| Notification | `omnistay-alerts` |
| Alarm name | `omnistay-alb-5xx-high` |

### 22.3. RDS CPU High

| Truong | Gia tri |
| --- | --- |
| Metric | RDS `CPUUtilization` |
| Resource | `omnistay-mysql` |
| Condition | Greater than 80 |
| Notification | `omnistay-alerts` |
| Alarm name | `omnistay-rds-cpu-high` |

Anh can chup:

- Alarm list.
- Alarm details neu mentor yeu cau.

## 23. Bo Anh Cuoi Cung Nen Co

- [ ] Budget summary.
- [ ] VPC details.
- [ ] VPC resource map.
- [ ] Subnet list.
- [ ] Public route table.
- [ ] Private route tables.
- [ ] NAT Gateways available.
- [ ] `SG-ALB` inbound.
- [ ] `SG-EC2` inbound.
- [ ] `SG-RDS` inbound.
- [ ] `SG-Redis` inbound.
- [ ] RDS available + endpoint.
- [ ] ElastiCache available + endpoint.
- [ ] IAM role policies.
- [ ] Secrets Manager secret metadata.
- [ ] Parameter Store list.
- [ ] S3 frontend bucket permissions.
- [ ] S3 artifact bucket summary.
- [ ] Target Group health check `/health`.
- [ ] ALB active + DNS.
- [ ] Launch Template summary.
- [ ] ASG details + scaling policy.
- [ ] CloudFront distribution + OAC.
- [ ] CloudFront behaviors.
- [ ] WAF Web ACL.
- [ ] SNS topic/subscription.
- [ ] CloudWatch dashboard.
- [ ] CloudWatch alarms.

## 24. Cleanup Cuoi Buoi

Lam cac muc nay ngay sau khi chup anh neu chua can deploy tiep:

- [ ] ASG: set Desired `0`, Min `0`.
- [ ] Delete NAT Gateways neu chua can EC2 private outbound.
- [ ] Release Elastic IP sau khi NAT Gateway da delete.
- [ ] Delete ElastiCache neu chua dung tiep, vi ElastiCache khong co stop.
- [ ] Stop RDS neu chua deploy backend.
- [ ] Delete ALB neu chua can test backend.
- [ ] Giu VPC, Subnets, Security Groups, IAM, S3 neu con lam tiep.
- [ ] Kiem tra Billing dashboard sau buoi lab.

## 25. Ghi Chu Ve Domain

Hien tai chua co domain nen khong dua Route 53 va ACM vao phan bat buoc. Khi co domain, bo sung sau:

- Route 53 hosted zone.
- ACM certificate o `us-east-1` cho CloudFront.
- DNS validation.
- CloudFront alternate domain name.

## 26. Link Tai Lieu AWS Chinh Thuc

- VPC: <https://docs.aws.amazon.com/vpc/latest/userguide/create-vpc.html>
- S3: <https://docs.aws.amazon.com/AmazonS3/latest/userguide/GetStartedWithS3.html>
- CloudFront OAC: <https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/private-content-restricting-access-to-s3.html>
- ALB: <https://docs.aws.amazon.com/elasticloadbalancing/latest/application/create-application-load-balancer.html>
- Target Group health checks: <https://docs.aws.amazon.com/elasticloadbalancing/latest/application/target-group-health-checks.html>
- RDS: <https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_CreateDBInstance.html>
- ElastiCache: <https://docs.aws.amazon.com/AmazonElastiCache/latest/dg/Clusters.Create.html>
- Auto Scaling Group: <https://docs.aws.amazon.com/autoscaling/ec2/userguide/create-asg-launch-template.html>
- Target tracking scaling: <https://docs.aws.amazon.com/autoscaling/ec2/userguide/as-scaling-target-tracking.html>
- AWS Budgets: <https://docs.aws.amazon.com/cost-management/latest/userguide/budgets-create.html>
