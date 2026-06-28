<<<<<<< HEAD
# Apollo Spoilers 🏎️

A production-grade AI-powered ecommerce platform for car spoilers and automotive exterior styling accessories. Built with ASP.NET Core 9 (Clean Architecture), Angular 21, and a RAG-powered AI shopping assistant named **Aasra**.

---

## ✨ Features

### Storefront (Customer)
- **Product catalog** with search, filtering (category, car brand, car model, price), sorting, and pagination
- **Product detail** pages with image galleries, specs, compatibility info, and customer reviews
- **Cart** with quantity management and stock validation
- **Wishlist** (save for later)
- **Checkout** → order placement with shipping address (no payment gateway)
- **Order history** with status tracking
- **JWT auth** with refresh tokens: register, login, logout, profile

### Admin
- Dashboard with recent **orders** + status management (Pending → Confirmed → Shipped → Delivered)
- **Product** listing view
- Order status updates

### AI Assistant — "Aasra" 🤖
A floating chat widget powered by **Retrieval-Augmented Generation (RAG)**:
- Embeds the product catalog into **Qdrant** vector database
- On each user query: generates embedding → searches Qdrant for relevant products → assembles context → LLM generates a grounded answer
- Recommends products, checks car compatibility, helps with search — all grounded in real catalog data
- Cites sources (clickable product links)
- Built with **Microsoft Semantic Kernel** + an OpenAI-compatible LLM (default: local **Ollama** with `llama3`)

---

## 🏗️ Architecture

### Backend — Clean Architecture (4 layers)

```
backend/
├─ ApolloSpoilers.sln
└─ src/
   ├─ ApolloSpoilers.Domain          ← Entities, enums, interfaces, specs (no dependencies)
   ├─ ApolloSpoilers.Application     ← DTOs, services, AutoMapper, validators, Result<T>
   ├─ ApolloSpoilers.Infrastructure  ← EF Core, Identity, JWT, Qdrant, Semantic Kernel
   └─ ApolloSpoilers.Api             ← Controllers, middleware, DI composition root
```

**Dependency direction:** `Domain ← Application ← Infrastructure ← Api` (no circular refs).

**Patterns & standards:**
- Repository + Unit of Work
- Specification pattern (declarative query composition)
- DTO + AutoMapper
- FluentValidation
- Result<T> pattern (service → controller)
- Global exception middleware → consistent `ErrorResponse` shape
- Serilog structured logging (console + file)
- Swagger + ASP.NET API versioning (`/api/v1/...`)
- Async/await throughout

### Frontend — Angular 21

```
apollo-spoilers/src/app/
├─ core/             ← services (auth, catalog, cart, wishlist, orders, chat), guards, interceptors, models
├─ shared/aasra-chat/← floating AI chat widget
├─ features/
│  ├─ products/      ← product list (with filters) + detail
│  ├─ auth/          ← login + register
│  ├─ cart/          ← cart page
│  ├─ wishlist/      ← wishlist page
│  ├─ checkout/      ← checkout form
│  ├─ orders/        ← order history
│  └─ admin/         ← admin dashboard
└─ app.*             ← root shell with Material toolbar + router outlet
```

- Standalone components, lazy-loaded routes
- Angular Material 21 UI
- Signal-based state (cart count, auth, chat)
- JWT interceptor with transparent refresh-on-401
- Reactive forms with validation

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 9, ASP.NET Core, EF Core 9, ASP.NET Identity |
| Database | SQL Server 2022 |
| AI / RAG | Microsoft Semantic Kernel 1.x, Qdrant vector DB, OpenAI-compatible LLM (Ollama default) |
| Frontend | Angular 21, Angular Material 21, Signals |
| Deployment | Docker, docker-compose, GitHub Actions CI |

---

## 🚀 Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (includes Docker Compose)
- `.env` file — copy `.env.example` to `.env` and review/edit secrets

### One-command startup
```bash
# from repo root
cp .env.example .env
docker compose up --build
```

This brings up **5 containers**:

| Service | Port | Purpose |
|---|---|---|
| `sqlserver` | 1433 | SQL Server 2022 (DB auto-created & seeded on API start) |
| `qdrant` | 6333 / 6334 | Vector database for Aasra RAG |
| `ollama` | 11434 | Local LLM server (needs model pull — see below) |
| `api` | 8080 | ASP.NET Core API + Swagger |
| `web` | 4200 | Angular app (nginx-served) |

### Pull Ollama models (required for Aasra to actually answer)
After `docker compose up`, run once:
```bash
docker exec -it apollo-ollama ollama pull nomic-embed-text   # embeddings (small, ~270 MB)
```

> **Which model for the chat LLM?** This depends on your host RAM — see the
> **Low-RAM / hybrid setup** note just below before pulling a chat model.

### ⚠️ Low-RAM hosts (≤ ~4 GB) — use the Groq hybrid
The default chat model `llama3` (**Meta-Llama-3-8B-Instruct**) needs ~5+ GB of
RAM on its own. On a host with ~3.7 GB total RAM the `apollo-ollama` container
gets OOM-killed before it can finish loading. The fix is the **hybrid** setup
that's already wired into `.env`:

- **Embeddings stay local** in Ollama — `nomic-embed-text` is tiny (~270 MB)
  and fits comfortably.
- **Chat LLM is routed to Groq's free tier** — `llama-3.3-70b-versatile` runs
  on Groq's servers, so it consumes **0 GB of host RAM** while giving better
  answers than the local 8B model.

To switch on (values are pre-set in `.env`, just add your key):

```env
Ai__Llm__BaseUrl=https://api.groq.com/openai/v1
Ai__Llm__ApiKey=YOUR_GROQ_API_KEY_HERE      # get one free at https://console.groq.com/keys
Ai__Llm__Model=llama-3.3-70b-versatile

Ai__Embedding__BaseUrl=http://ollama:11434/v1   # unchanged — still local
Ai__Embedding__Model=nomic-embed-text
```

Then restart the stack (`docker compose down && docker compose up -d --build`)
and pull **only** the embedding model as shown above. **Do not** run
`ollama pull llama3` on a low-RAM host — that's what triggers the OOM.

| | Default (local) | Low-RAM hybrid |
|---|---|---|
| Chat LLM | `llama3` (8B) local → ~5+ GB | `llama-3.3-70b` on Groq → 0 GB local |
| Embeddings | `nomic-embed-text` local | unchanged |
| Ollama RAM | crashes (OOM) | ~400 MB, stable |

### Default admin account (seeded)
- **Email:** `admin@apollospoilers.com`
- **Password:** `Admin#123`

### Access the apps
- **Frontend:** http://localhost:4200
- **API Swagger:** http://localhost:8080/swagger
- **Qdrant dashboard:** http://localhost:6333/dashboard

---

## 💻 Local Development (without Docker)

### Backend
```bash
cd backend
dotnet restore
dotnet build
# Update connection string in src/ApolloSpoilers.Api/appsettings.json if needed
dotnet ef database update --project src/ApolloSpoilers.Infrastructure --startup-project src/ApolloSpoilers.Api
dotnet run --project src/ApolloSpoilers.Api
```
The API auto-migrates and seeds on startup, so `dotnet run` alone is enough for a fresh DB.

### Frontend
```bash
cd apollo-spoilers
npm install
npm start    # ng serve → http://localhost:4200
```

Point the frontend at your API via `src/environments/environment.ts` (`apiUrl`).

### Database schema (EF Code First)
14 tables auto-created:

| Table | Purpose |
|---|---|
| Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens | ASP.NET Identity |
| Categories | Product categories (self-referencing for sub-categories) |
| Products | Spoilers with price, car fitment, material, etc. |
| ProductImages | Multiple images per product, primary flag |
| Inventories | 1:1 stock record per product |
| Carts, CartItems | Per-user shopping cart |
| Wishlists, WishlistItems | Per-user wishlist |
| Orders, OrderItems | Orders with shipping address, status enum |
| Reviews | Product ratings (1-5) + comments, approval flow |
| ChatSessions, ChatMessages | Aasra conversation persistence |
| AiKnowledgeChunks | SQL pointer to Qdrant vector points |

---

## 🤖 Aasra RAG Pipeline

```
User Query
    ↓
[1] Semantic Kernel — embed query (nomic-embed-text)
    ↓
[2] Qdrant — ANN search → top-K product chunks
    ↓
[3] Assemble prompt: system persona + retrieved context + chat history
    ↓
[4] Semantic Kernel — LLM completion (llama3 via Ollama)
    ↓
[5] Persist turn (ChatMessages) + return answer with cited sources
```

**Swapping the LLM backend** — Aasra is OpenAI-compatible. Change these env vars to use Groq (free tier) or OpenAI:

```env
# Groq example (free, fast)
Ai__Llm__BaseUrl=https://api.groq.com/openai/v1
Ai__Llm__ApiKey=your-groq-key
Ai__Llm__Model=llama-3.3-70b-versatile

Ai__Embedding__BaseUrl=https://api.groq.com/openai/v1   # or keep Ollama for embeddings
Ai__Embedding__Model=nomic-embed-text
```

> Note: if Ollama/Qdrant are unreachable, Aasra returns a clean error message instead of crashing.

---

## 🔌 API Reference (v1)

All endpoints under `/api/v1/`. Auth via `Authorization: Bearer {token}`.

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | – | Register customer |
| POST | `/auth/login` | – | Login |
| POST | `/auth/refresh` | – | Refresh token pair |
| POST | `/auth/logout` | ✅ | Invalidate refresh token |
| POST | `/auth/forgot-password` | – | Request reset (token logged in dev) |
| POST | `/auth/reset-password` | – | Reset with token |
| GET/PUT | `/auth/profile` | ✅ | View/update profile |
| GET | `/products` | – | Search/filter/paginate |
| GET | `/products/{slug}` | – | Product detail |
| GET | `/products/categories` | – | List categories |
| GET | `/products/car-brands` | – | Distinct brands |
| GET | `/products/car-models/{brand}` | – | Models for a brand |
| GET | `/products/{id}/reviews` | – | Approved reviews |
| POST | `/products/{id}/reviews` | ✅ | Submit review |
| GET/POST/PUT/DELETE | `/cart` + `/cart/items` | ✅ | Cart operations |
| GET/POST/DELETE | `/wishlist` + `/wishlist/{productId}` | ✅ | Wishlist |
| POST | `/orders` | ✅ | Place order from cart |
| GET | `/orders/my` | ✅ | Order history |
| GET | `/orders/{id}` | ✅ | Order detail |
| POST | `/chat` | ✅ | **Send message to Aasra** |
| GET | `/chat/{sessionId}/history` | ✅ | Chat history |
| — | `/admin/products/*` | Admin | Product CRUD, images, stock |
| — | `/admin/orders/*` | Admin | List orders, update status |
| — | `/admin/products/reviews/*` | Admin | Approve/reject reviews |

---

## 🐳 Production Notes

- Set `ASPNETCORE_ENVIRONMENT=Production` and a strong `Jwt:Secret` (≥32 chars).
- Replace the dev SQL password, generate fresh JWT secret.
- For Azure: the multi-stage Dockerfiles publish framework-dependent images; deploy to Azure Container Apps or App Service for Containers. Point `ConnectionStrings:DefaultConnection` at Azure SQL.
- Run `IProductIndexer.ReindexAllAsync()` once after deploy (or it auto-runs per-product on admin changes) to populate Qdrant.
- **Forgot-password** logs the reset token in development. Wire up an email sender (e.g. SendGrid) for production by replacing the dev logger call in `AuthService.ForgotPasswordAsync`.

---

## ⚠️ Known Limitations / Out of Scope

This is a deep vertical-slice build, not every module is equally polished:
- **No payment gateway** — orders are created with status `Pending`. Add Stripe/etc. later.
- **Email** — forgot-password tokens are logged, not emailed.
- **Admin UI** — order status + product list are functional; full CRUD forms (create/edit/delete products) are API-only for now (Swagger-accessible).
- **Product images** — seeded products reference placeholder paths (`/images/products/{slug}-1.jpg`); drop real images in `wwwroot/images/` or update URLs.
- **AutoMapper advisory** — NuGet flags GHSA-rvv3-g6hj-g44x against all published AutoMapper versions; it's a transitive concern with no patched release. Evaluate for your threat model before production.
- **Aasra needs a live LLM + Qdrant** to answer meaningfully. The code degrades gracefully with clean errors if those services are absent.

---

## 📂 Repository Layout

```
C:\Users\Ramit\ZCodeProject\
├─ .github/workflows/ci.yml
├─ .env.example
├─ .gitignore
├─ docker-compose.yml
├─ README.md
├─ backend/                    ← .NET 9 solution (4 projects)
└─ apollo-spoilers/            ← Angular 21 workspace
```

---

## 📜 License

MIT — for educational/portfolio use. Replace placeholder assets and review all security configuration before any production deployment.
=======
# backend
>>>>>>> origin/master
