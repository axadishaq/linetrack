<p align="center">
  <img src="docs/images/banner.svg" alt="LineTrack: production-line tracking with .NET, Vue, WPF and PostgreSQL" width="100%">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/C%23-ASP.NET%20Core-239120?logo=csharp&logoColor=white" alt="C# ASP.NET Core">
  <img src="https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL 17">
  <img src="https://img.shields.io/badge/Vue-3-4FC08D?logo=vuedotjs&logoColor=white" alt="Vue 3">
  <img src="https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=white" alt="Vite">
  <img src="https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows&logoColor=white" alt="WPF">
  <img src="https://img.shields.io/badge/SignalR-real--time-512BD4" alt="SignalR">
  <img src="https://img.shields.io/badge/GitHub%20Actions-CI%2FCD-2088FF?logo=githubactions&logoColor=white" alt="GitHub Actions">
</p>

<p align="center">
  <b>Track every phone case through the factory line, from print to pack, in real time.</b>
</p>

---

> **Status: in progress.** I'm building LineTrack step by step and updating this README as each part works. See the [roadmap](#roadmap) and the [daily progress log](docs/progress.md).

## What it is

LineTrack is a small manufacturing tracker. Phone cases move through four stations (**Print → Cure → Quality Check → Pack**). Every scan, defect and machine stop is recorded, and supervisors see live production metrics on a dashboard.

I'm building it to learn .NET properly by making something real. It isn't meant to replace a full MES (Manufacturing Execution System). It's a focused, working system I can explain line by line.

## Features

| | Feature | What it does |
| --- | --- | --- |
| 📦 | **Production orders** | Create an order, release it, and get one unit per item with a unique serial (`LT-2026-000123`) and a QR code |
| 🔁 | **Station sequence rules** | The API refuses a scan that skips or repeats a station |
| ✅ | **Quality check** | Pass, or fail with a defect reason (misprint, scratch, bubble…) → Scrapped or Rework |
| ⏸️ | **Downtime tracking** | Operators or machines open a stop with a reason and close it later |
| 🔎 | **Traceability** | Search any serial and see its full history: who, which station, when and the outcome |
| 📈 | **Live dashboard** | Units per hour, defect rate, open stops and order progress, pushed live over SignalR |
| 🤖 | **Machine simulator** | A worker service acting as printers and ovens, streaming counts, temperature and faults |
| 🔐 | **Roles** | Operator, Supervisor and Admin with JWT sign-in |

## Architecture

<p align="center">
  <img src="docs/images/architecture.svg" alt="Three clients talk to one ASP.NET Core API, which is the only part that touches PostgreSQL" width="90%">
</p>

Three clients talk to one API. The API owns every business rule and is the only part that touches the database. The rules live in **Core** services with no database or web dependencies, so they're easy to unit-test.

### How a unit moves through the line

```mermaid
stateDiagram-v2
    direction LR
    [*] --> InProgress: order released
    InProgress --> InProgress: Print ✓ → Cure ✓
    InProgress --> Rework: QC fail (reworkable)
    Rework --> InProgress: re-scanned
    InProgress --> Scrapped: QC fail (scrap)
    InProgress --> Passed: QC pass
    Passed --> Packed: Pack ✓
    Packed --> [*]
    Scrapped --> [*]
```

### What happens on a scan

```mermaid
sequenceDiagram
    autonumber
    participant Op as Operator (WPF)
    participant API as ASP.NET Core API
    participant Core as ScanService
    participant DB as PostgreSQL
    participant Dash as Dashboard (Vue)
    Op->>API: POST /api/v1/scans (serial, station, result)
    API->>Core: validate rules
    Core->>DB: load unit + last scan
    alt rule broken
        Core-->>API: 409 Conflict (clear reason)
        API-->>Op: red result + message
    else valid
        Core->>DB: save scan, update unit
        API-->>Op: green result
        API-)Dash: SignalR ScanRecorded
    end
```

### Data model

```mermaid
erDiagram
    PRODUCTS ||--o{ PRODUCTION_ORDERS : "made as"
    PRODUCTION_ORDERS ||--o{ UNITS : contains
    UNITS ||--o{ SCAN_EVENTS : "history"
    STATIONS ||--o{ MACHINES : has
    STATIONS ||--o{ SCAN_EVENTS : "scanned at"
    MACHINES ||--o{ DOWNTIME_EVENTS : stops
    MACHINES ||--o{ MACHINE_READINGS : reports
    USERS ||--o{ SCAN_EVENTS : performs
    DEFECT_REASONS |o--o{ SCAN_EVENTS : explains
```

## Tech stack

| Layer | Technologies |
| --- | --- |
| **Backend** | C#, .NET 10, ASP.NET Core Web API (controllers), EF Core 10 + Npgsql, SignalR, BackgroundService, FluentValidation, JWT + BCrypt, Serilog, Health Checks, Scalar API docs, QRCoder |
| **Database** | PostgreSQL 17 with indexes and hand-written reporting queries |
| **Dashboard** | Vue 3 (`<script setup>`), TypeScript, Vite, Vue Router, Pinia, Axios, @microsoft/signalr, Tailwind CSS, Apache ECharts |
| **Operator app** | WPF on .NET 10, CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting + DI, SignalR client |
| **Testing** | xUnit + Shouldly, WebApplicationFactory + Testcontainers, Vitest + Vue Test Utils |
| **DevOps** | Git, GitHub Actions, Docker + Docker Compose, GitHub Container Registry, Kubernetes (kind) |
| **Quality** | .NET analyzers, `dotnet format`, ESLint + Prettier, AI code review on pull requests |

## Project structure

```text
linetrack/
├─ LineTrack.slnx
├─ Directory.Build.props        # nullable on, warnings as errors
├─ docker-compose.yml
├─ src/
│  ├─ LineTrack.Core/            # entities, rules, ScanService, MetricsService
│  ├─ LineTrack.Infrastructure/  # EF Core DbContext, migrations, SQL queries
│  ├─ LineTrack.Api/             # controllers, SignalR hub, auth, background jobs
│  ├─ LineTrack.Simulator/       # worker service acting as factory machines
│  └─ LineTrack.OperatorStation/ # WPF operator app (MVVM)
├─ tests/
│  ├─ LineTrack.Core.Tests/
│  ├─ LineTrack.Api.IntegrationTests/
│  └─ LineTrack.OperatorStation.Tests/
├─ web/                         # Vue 3 + Vite dashboard
├─ k8s/                         # Kubernetes manifests
├─ docs/                        # diagrams, screenshots, progress log, AI-usage log
└─ .github/workflows/ci.yml
```

Core depends on nothing. Infrastructure and Api depend on Core, never the other way round.

## Roadmap

**Week 1: C# foundations, API and database**
- [ ] Solution structure, Core entities, EF Core + PostgreSQL in Docker, seed data
- [ ] JWT sign-in with Operator, Supervisor and Admin roles
- [ ] Orders and units endpoints, release + serial generation
- [ ] `ScanService` with all station rules + unit tests
- [ ] API docs with Scalar

**Week 2: Dashboard and real time**
- [ ] Vue dashboard: login, orders, traceability timeline
- [ ] SignalR hub + live floor page
- [ ] Hand-written SQL reports + charts
- [ ] Integration tests with Testcontainers

**Week 3: Equipment and WPF**
- [ ] Machine simulator with faults and retry/back-off
- [ ] Downtime tracking and live metrics
- [ ] WPF operator app: scan, pass/fail, report a stop

**Week 4: DevOps and polish**
- [ ] Dockerfiles + one-command `docker compose up`
- [ ] GitHub Actions: build, test, lint, AI review, image push
- [ ] Kubernetes deployment on kind
- [ ] Screenshots, demo GIF and design notes

## Running it

Setup steps will be added here as each part works. The goal is one command, `docker compose up`, to start the database, API, simulator and dashboard with sample data.

## How I use AI

I use AI coding tools while building LineTrack, and I review everything before it goes in. I'm keeping a short log of suggestions I accepted, changed or rejected, and why, in [`docs/ai-usage-log.md`](docs/ai-usage-log.md).

## Author

**Asad Ishaq**, Full Stack Developer · [Portfolio](https://axadishaq.web.app) · [GitHub](https://github.com/axadishaq)
