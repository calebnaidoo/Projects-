# GLMS — Global Logistics Management System
## TechMove Logistics | PROG7311 / EAPD7111 | Full POE (Parts 2 + 3)

---

## Quick Start

### Local Development (without Docker)
```bash
# Terminal 1 — run the API on port 5001
cd GLMS.API
dotnet run

# Terminal 2 — run the MVC app on port 5000
cd GLMS.Web
dotnet run
```
Update `GLMS.Web/appsettings.Development.json` → `"BaseUrl": "http://localhost:5001/"`.

### Docker (all 3 containers)
```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| MVC Frontend | http://localhost:5000 |
| API + Swagger | http://localhost:5001 |
| SQL Server | localhost,1433 (sa / YourStrong@Passw0rd) |

---

## Solution Structure

| Project | Purpose |
|---------|---------|
| `GLMS.Web` | Part 2: Full MVC monolith (EF Core, Identity, file upload, currency) |
| `GLMS.API` | Part 3: REST Web API — owns DB, exposes endpoints, JWT auth |
| `GLMS.Tests` | Parts 2+3: 29 unit tests + 12 integration tests = 41 total |

---

## API Endpoints (Swagger at http://localhost:5001)

| Verb | Endpoint | Description |
|------|----------|-------------|
| POST | /api/auth/register | Register |
| POST | /api/auth/login | Login → JWT token |
| GET | /api/contracts | List (filter by date/status/search) |
| POST | /api/contracts | Create (returns 201) |
| GET | /api/contracts/{id} | Get by ID (404 if missing) |
| PUT | /api/contracts/{id} | Full update |
| PATCH | /api/contracts/{id}/status | Status-only update |
| DELETE | /api/contracts/{id} | Delete (returns 204) |
| GET | /api/clients | List clients |
| POST | /api/clients | Create client |
| GET | /api/servicerequests | List all |
| POST | /api/servicerequests | Create (422 if contract Expired/OnHold) |
| GET | /api/currency/rates | Live USD/EUR/GBP → ZAR |

---

## Run Tests
```bash
dotnet test GLMS.Tests/GLMS.Tests.csproj --verbosity normal
```
Uses InMemory database — no SQL Server required.

## GitHub Actions
Push to GitHub → Actions tab → workflow runs automatically on every commit.
