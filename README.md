# AWS_OmniStay

High-availability hotel booking demo on AWS.

This repository is the implementation workspace for a graduation/project demo
that shows a hotel booking web app running with load balancing, auto scaling,
caching, monitoring, and cost controls on AWS.

## Project Goals

- Build a simple hotel booking product: search hotels, view rooms, book rooms,
  view booking history, and manage data through an admin workflow.
- Demonstrate cloud architecture concepts: high availability, EC2 Auto Scaling,
  Redis/Valkey caching, RDS-backed persistence, and CloudWatch monitoring.
- Configure AWS services through AWS Console for mentor screenshots and keep
  secrets out of source control.

## Repository Structure

```text
.
├── backend/
│   ├── src/HotelBooking.Api/          # ASP.NET Core Web API
│   └── tests/HotelBooking.Api.Tests/  # Backend automated tests
├── frontend/
│   ├── public/                        # Static HTML and public files
│   ├── src/                           # Frontend JavaScript and styles
│   └── assets/                        # Images and other static assets
├── infra/                             # Reserved for future IaC, not used in the current Console-first setup
├── load-testing/                      # Locust scenarios and reports
├── docs/
│   ├── architecture/                  # Architecture notes and diagrams
│   ├── demo/                          # Demo script and presentation notes
│   └── runbooks/                      # Deploy, rollback, and cleanup guides
└── scripts/                           # Local helper scripts
```

## Planned Architecture

- Frontend: S3 private bucket served through CloudFront Origin Access Control.
- API: ASP.NET Core Web API on EC2 instances behind an Application Load Balancer.
- Scaling: EC2 Auto Scaling Group with CPU target tracking.
- Data: Amazon RDS MySQL for persistent data and ElastiCache Redis OSS/Valkey
  for search-result caching.
- Operations: CloudWatch dashboard, alarms, and explicit cleanup steps to avoid
  unexpected AWS cost.

## Development Workflow

1. Implement and test the backend locally.
2. Build the static frontend and configure its API endpoint through `config.js`.
3. Follow `aws-console-setup-guide.html` to create AWS services in the AWS Console.
4. Follow `aws-code-config-guide.html` to connect code runtime settings to AWS services.
5. Deploy backend artifacts to EC2 and frontend assets to S3.
6. Run Locust load tests to capture cache, scaling, and availability evidence.

## Local Run

Start the backend API:

```bash
dotnet run --project backend/src/HotelBooking.Api/HotelBooking.Api.csproj --urls http://127.0.0.1:5010
```

Serve the static frontend from the repository root:

```bash
python3 -m http.server 5173 -d frontend
```

Open:

```text
http://127.0.0.1:5173/public/index.html
```

The frontend reads its backend URL from `frontend/public/config.js`. For local
development it points to `http://localhost:5010/api`.

For CloudFront production, use the sample `frontend/public/config.aws.example.js`
and set `API_BASE` to `/api`.

## AWS Documentation Files

- `services-aws.html`: summary of AWS services, roles, benefits, and rough pricing.
- `aws-console-setup-guide.html`: step-by-step AWS Console setup checklist.
- `aws-code-config-guide.html`: runtime configuration guide for backend/frontend code.

## Security Notes

- Do not commit AWS access keys, database passwords, JWT secrets, PEM files, or
  local `.env` files.
- Use IAM roles for AWS resources.
- Use SSM Parameter Store or Secrets Manager for runtime secrets.
- Create an AWS Budget before deploying paid resources.
