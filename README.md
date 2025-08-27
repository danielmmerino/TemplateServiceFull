# TemplateService (.NET 8) — Microservicio base

Incluye:
- API .NET 8 con Swagger, EF Core, HealthChecks
- MassTransit + RabbitMQ
- OpenTelemetry (OTLP) → Jaeger
- Dockerfile y docker-compose (infra + servicio + gateway YARP)
- GitHub Actions (CI + push a GHCR)
- Config VS Code (.vscode/)

## Requisitos
- .NET 8 SDK
- Docker Desktop (WSL2 en Windows)
- VS Code + extensiones: C# Dev Kit, Docker, YAML, Thunder Client

## Levantar
```bash
docker compose up -d --build
```
- API: http://localhost:8080/swagger
- Gateway: http://localhost:7000/template/
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- Jaeger UI: http://localhost:16686

## Desarrollo local
```bash
dotnet run --project src/TemplateService.Api/TemplateService.Api.csproj
```
