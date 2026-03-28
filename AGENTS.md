# AGENTS.md — NexusGrid Agent Roles

## Architect Agent
- Owns all architecture decisions in CLAUDE.md
- Enforces polyglot persistence boundaries (PostgreSQL for ACID, Cassandra for AP, Redis for cache)
- Reviews any cross-service communication patterns
- Ensures each service owns its own database — no shared databases

## Backend Agent
- Writes all C# service code following CLAUDE.md coding standards
- Enforces: Repository pattern, service layer, DTOs separate from entities
- All async methods use CancellationToken and end with Async suffix
- Constructor injection only — no service locator
- sealed on classes not designed for inheritance

## Database Agent
- EF Core: Code-first migrations, never raw SQL unless performance-critical
- Cassandra: Prepared statements only, query-first schema design
- Redis: StackExchange.Redis with singleton ConnectionMultiplexer
- Reviews all data access for proper async patterns

## Testing Agent
- xUnit + Moq + FluentAssertions for all tests
- Test naming: MethodName_Scenario_ExpectedResult
- Every public controller action has at least one test
- Minimum 80% coverage on service and repository layers

## DevOps Agent
- Owns Dockerfiles, docker-compose, Kubernetes manifests
- Owns Terraform modules and CI/CD pipelines
- Enforces: multi-stage Docker builds, health probes on every pod
- Reviews security: no secrets in code, least-privilege IAM

## Observability Agent
- Owns Prometheus config, Grafana dashboards, Serilog setup
- Ensures all services expose /metrics and /health endpoints
- Correlation IDs propagated via X-Correlation-Id header
- Structured JSON logging with service_name, correlation_id, level
