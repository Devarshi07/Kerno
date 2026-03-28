# NexusGrid

A production-grade cloud-native microservices platform built with .NET, demonstrating polyglot persistence, Kubernetes orchestration, Terraform IaC, CI/CD pipelines, and full observability.

## Architecture

```
                        ┌─────────────────────┐
                        │     API Gateway      │
                        │   (.NET + YARP)      │
                        │  Redis Rate Limit    │
                        │  Redis Cache         │
                        │  JWT Validation      │
                        └──────────┬───────────┘
                                   │
              ┌────────────────────┼────────────────────┐
              │                    │                     │
    ┌─────────▼────────┐ ┌────────▼─────────┐ ┌────────▼──────────┐
    │  Order Service   │ │  User Service    │ │ Notification Svc  │
    │  (.NET 10 API)   │ │  (.NET 10 API)   │ │ (.NET 10 API)     │
    │  CRUD + Status   │ │  Auth + JWT      │ │ Events + Audit    │
    └────────┬─────────┘ └────────┬─────────┘ └────────┬──────────┘
             │                    │                     │
    ┌────────▼─────────┐ ┌───────▼──────────┐ ┌────────▼──────────┐
    │   PostgreSQL     │ │   PostgreSQL     │ │    Cassandra      │
    │   (Orders DB)    │ │   (Users DB)     │ │  (Events/Audit)   │
    └──────────────────┘ └──────────────────┘ └───────────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# / .NET 10, ASP.NET Core Web API |
| Order & User DB | PostgreSQL via Entity Framework Core |
| Event/Audit DB | Apache Cassandra via DataStax C# Driver |
| Cache & Rate Limit | Redis via StackExchange.Redis |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Auth | JWT (HS256) + BCrypt password hashing |
| Containerization | Docker, docker-compose |
| Orchestration | Kubernetes (Minikube / EKS) |
| IaC | Terraform (AWS — VPC, EKS, RDS, Keyspaces, ElastiCache) |
| CI/CD | Jenkins + GitHub Actions |
| Observability | Prometheus, Grafana, Serilog |
| Testing | xUnit, Moq, FluentAssertions |

## Project Structure

```
nexusgrid/
├── src/
│   ├── NexusGrid.Gateway/              # API Gateway (YARP + Redis)
│   ├── NexusGrid.OrderService/         # Order Service (PostgreSQL)
│   ├── NexusGrid.UserService/          # User Service (PostgreSQL + JWT)
│   ├── NexusGrid.NotificationService/  # Notification Service (Cassandra)
│   └── NexusGrid.Shared/              # Shared DTOs, exceptions, extensions
├── tests/
│   ├── NexusGrid.OrderService.Tests/
│   ├── NexusGrid.UserService.Tests/
│   ├── NexusGrid.NotificationService.Tests/
│   └── NexusGrid.Gateway.Tests/
├── infrastructure/
│   ├── terraform/                      # AWS IaC (5 modules)
│   └── kubernetes/                     # K8s manifests (16 files)
├── ci/
│   ├── Jenkinsfile                     # 11-stage pipeline
│   └── .github/workflows/ci-cd.yml    # GitHub Actions (5 jobs)
├── observability/
│   ├── prometheus/                     # Metrics collection
│   ├── grafana/                        # Dashboards
│   └── logging/                        # Serilog config
├── docker-compose.yml                  # Full-stack local orchestration
├── CLAUDE.md                           # Agentic development config
└── AGENTS.md                           # Agent role definitions
```

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker & docker-compose
- (Optional) kubectl + Minikube for K8s
- (Optional) Terraform for AWS deployment

### Run Locally with Docker

```bash
docker-compose up --build
```

| Service | URL |
|---------|-----|
| Gateway (API entry) | http://localhost:5000 |
| Order Service | http://localhost:5101 |
| User Service | http://localhost:5102 |
| Notification Service | http://localhost:5103 |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3000 (admin/admin) |

### Run Tests

```bash
dotnet test NexusGrid.sln --verbosity normal
```

### Build

```bash
dotnet build NexusGrid.sln
```

## API Endpoints

### Auth (public)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register new user |
| POST | `/api/v1/auth/login` | Login, receive JWT |

### Orders (authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/orders` | List orders (paginated) |
| GET | `/api/v1/orders/{id}` | Get order by ID |
| GET | `/api/v1/orders/user/{userId}` | Get orders by user |
| POST | `/api/v1/orders` | Create order |
| PATCH | `/api/v1/orders/{id}/status` | Update order status |
| DELETE | `/api/v1/orders/{id}` | Delete order |

### Users (authenticated, Admin-only for list/delete)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/users/me` | Get current user |
| GET | `/api/v1/users` | List all users (Admin) |
| PUT | `/api/v1/users/me` | Update profile |
| DELETE | `/api/v1/users/{id}` | Delete user (Admin) |

### Notifications
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/notifications` | Create notification |
| GET | `/api/v1/notifications/user/{userId}` | Get by user |
| GET | `/api/v1/notifications/status/{status}` | Get by status |
| PATCH | `/api/v1/notifications/{userId}/{createdAt}/{id}/status` | Update status |

### Audit Events
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/audit` | Create audit event |
| GET | `/api/v1/audit/tenant/{tenantId}` | Get by tenant |

## Database Design

### PostgreSQL (ACID — Orders & Users)

Entity Framework Core with code-first migrations. Transactional data requiring strong consistency.

### Cassandra (AP — Notifications & Audit)

Query-first schema design with denormalized tables:

| Query Pattern | Table | Partition Key | Clustering |
|--------------|-------|--------------|------------|
| User's notifications | `notifications_by_user` | `user_id` | `created_at DESC` |
| Pending notifications today | `notifications_by_status` | `(status, date)` | `created_at DESC` |
| Tenant audit trail | `audit_events_by_tenant` | `(tenant_id, event_date)` | `event_time DESC` |

### Redis (Sub-ms — Gateway)

- Sliding window rate limiting via sorted sets + Lua scripts
- Response caching with configurable TTL

## Infrastructure

### Terraform Modules

| Module | Resources |
|--------|-----------|
| networking | VPC, public/private subnets, NAT, security groups |
| compute | EKS cluster + managed node group |
| database | 2x RDS PostgreSQL + Amazon Keyspaces |
| cache | ElastiCache Redis |
| iam | Cluster role, node role, per-service IRSA roles |

### Kubernetes

- 2 replicas per application service
- Health probes (readiness + liveness) on every pod
- ConfigMaps for non-sensitive config, Secrets for credentials
- Nginx Ingress routing to Gateway

## CI/CD Pipeline

**Jenkins** — 11 stages: Checkout, Build, Test, Docker Build (parallel), Push to ECR, Terraform Plan, Deploy Staging, Smoke Tests, Manual Approval, Canary Deploy to Production, Health Check.

**GitHub Actions** — 5 jobs: build-and-test, docker-build (matrix), terraform-plan (PRs), deploy-staging, deploy-production (manual approval via environment protection).

## Observability

- **Prometheus** scrapes `/metrics` from all services (via prometheus-net)
- **Grafana** dashboards: Service Health Overview + Cassandra Performance
- **Serilog** structured JSON logging with correlation IDs across services
- Metrics: request rate, p95 latency, error rate, active connections

## CLAUDE.md Evolution

This project was built using agentic development with Claude Code. The CLAUDE.md file evolved throughout development, capturing coding standards, architecture decisions, and project conventions that guided AI-assisted development across all 10 phases.

| Phase | What was built |
|-------|---------------|
| 1 | Order Service — .NET fundamentals, EF Core, Repository pattern |
| 2 | User Service — JWT auth, BCrypt, role-based access |
| 3 | Notification Service — Cassandra query-first design |
| 4 | API Gateway — YARP, Redis rate limiting + caching |
| 5 | Docker — Multi-stage Dockerfiles, docker-compose |
| 6 | Kubernetes — Deployments, services, ingress, secrets |
| 7 | Terraform — AWS modules (VPC, EKS, RDS, Keyspaces, Redis) |
| 8 | CI/CD — Jenkins + GitHub Actions pipelines |
| 9 | Observability — Prometheus, Grafana, Serilog |
| 10 | Documentation — README, AGENTS.md, evolution log |
