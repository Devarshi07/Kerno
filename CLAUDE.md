CLAUDE.md — NexusGrid
Project Overview
NexusGrid is a cloud-native microservices platform built with .NET 8, demonstrating polyglot persistence (PostgreSQL + Cassandra + Redis), Kubernetes orchestration, Terraform IaC, CI/CD pipelines, and full observability.

Architecture Decisions
Polyglot Persistence
PostgreSQL (EF Core) for transactional data — orders, users (ACID required)
Cassandra for high-write event/audit logging — query-first schema design, denormalized
Redis for API gateway caching and sliding-window rate limiting
Never use Cassandra for transactional/relational data. Never use PostgreSQL for high-write append-only logs.
Service Communication
Synchronous REST via API Gateway (YARP) for request/response
Each service owns its own database — no shared databases
Correlation IDs propagated via X-Correlation-Id header across all services
Authentication
JWT tokens issued by User Service
API Gateway validates tokens before forwarding
Role-based access: Admin, User
Coding Standards
C# Conventions
Use PascalCase for public members, _camelCase for private fields
Prefer var only when the type is obvious from the right side
Use record types for DTOs, class for domain entities
Async methods must end with Async suffix (e.g., GetOrderByIdAsync)
Always use CancellationToken in async methods
No #region blocks — if you need regions, the class is too large
Use sealed on classes that aren't designed for inheritance
Architecture Patterns
Repository Pattern for all data access — no EF Core DbContext in controllers
Service Layer between controllers and repositories for business logic
DTOs separate from entities — never return database models from API endpoints
Constructor injection only — no service locator pattern
Use IOptions<T> pattern for configuration, not raw IConfiguration
API Design
Return ActionResult<T> from all controller actions
Use proper HTTP status codes: 201 for creation, 204 for delete, 404 for not found
Consistent error response: { "error": "message", "code": "ERROR_CODE", "details": {} }
Pagination via ?page=1&pageSize=20 with response metadata
API versioning via URL path: /api/v1/orders
Error Handling
Global exception middleware per service — no try/catch in controllers
Custom exception types: NotFoundException, ValidationException, ConflictException
Log exceptions with Serilog including correlation ID and stack trace
Never swallow exceptions silently
Testing
xUnit for unit tests, Moq for mocking, FluentAssertions for assertions
Every public controller action has at least one test
Test naming: MethodName_Scenario_ExpectedResult
Integration tests use WebApplicationFactory<T> + Testcontainers
Minimum 80% code coverage on service and repository layers
Database
EF Core: Code-first migrations, never raw SQL unless performance-critical
Cassandra: Prepared statements only, never string-concatenated CQL
Redis: Use StackExchange.Redis with connection multiplexer singleton
All database calls are async
Build & Run Commands
bash
# Restore and build all services
dotnet build NexusGrid.sln

# Run tests
dotnet test NexusGrid.sln --verbosity normal

# Run specific service locally
cd src/NexusGrid.OrderService && dotnet run

# Docker compose — full stack
docker-compose up --build

# Kubernetes local deploy
minikube start
kubectl apply -f infrastructure/kubernetes/

# Terraform plan
cd infrastructure/terraform && terraform plan -var-file=environments/staging/terraform.tfvars

# Terraform apply
cd infrastructure/terraform && terraform apply -var-file=environments/staging/terraform.tfvars
Project Structure Rules
Shared code goes in NexusGrid.Shared — DTOs, extension methods, constants
Each service has its own Dockerfile — no shared Dockerfiles
Kubernetes manifests mirror the service structure
Terraform is modular — one module per infrastructure concern
Tests mirror source structure: NexusGrid.OrderService → NexusGrid.OrderService.Tests
Debugging Notes
If EF Core migrations fail, check connection string in appsettings.Development.json
Cassandra container takes ~30s to be ready — docker-compose healthcheck handles this
Redis connection errors usually mean the container isn't up yet — retry logic is in RedisConnectionFactory
YARP gateway config is in appsettings.json under ReverseProxy section
Evolution Log
v1.0 — Initial setup: coding standards, architecture decisions, project structure
v2.0 — Phase 1-3: All 3 services built (Order, User, Notification) with 38 unit tests
v3.0 — Phase 4: API Gateway with YARP, Redis rate limiting + caching (48 tests)
v4.0 — Phase 5-6: Docker + docker-compose (10 containers), Kubernetes manifests (16 files)
v5.0 — Phase 7-8: Terraform AWS modules (5), Jenkins + GitHub Actions CI/CD pipelines
v6.0 — Phase 9-10: Prometheus + Grafana observability, README, AGENTS.md, full documentation
