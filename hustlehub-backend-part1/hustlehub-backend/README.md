# HustleHub+ | Secure Backend (Part 1 — Secure Foundations)

![Node.js](https://img.shields.io/badge/Node.js-18%2B-339933?logo=nodedotjs&logoColor=white)
![Express](https://img.shields.io/badge/Express-4.x-000000?logo=express&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-black?logo=jsonwebtokens)
![HTTPS](https://img.shields.io/badge/Transport-HTTPS%2FTLS-informational)
![Status](https://img.shields.io/badge/Status-Part%201%20Complete-success)

> **Module:** INSY7314 — Information Systems 3D 
> **POE Part 1:** Secure Foundations 

---

## Table of Contents

1. [Overview](#overview)
2. [Intended Users](#intended-users)
3. [Architecture](#architecture)
4. [Project Structure](#project-structure)
5. [Getting Started](#getting-started)
6. [Generating a Local SSL Certificate](#generating-a-local-ssl-certificate)
7. [Environment Variables](#environment-variables)
8. [API Reference](#api-reference)
9. [Security Implementation](#security-implementation)
10. [Testing](#testing)
11. [Design Decisions & Trade-offs](#design-decisions--trade-offs)

---

## Overview

**HustleHub+** is a secure freelance marketplace platform that lets **freelancers** advertise
services and **clients** browse and book them, while automatically recording the resulting
transactions and giving freelancers visibility into their income and estimated tax obligations.

Because the platform handles **credentials, transactional data, and financial information**,
security is treated as a first-class requirement rather than something bolted on afterwards.
Part 1 lays the foundation everything else is built on: a hardened API that can safely register
and authenticate users.

## Intended Users

| Role | Description |
|---|---|
| **Client** | Browses gigs and books freelancer services. |
| **Freelancer** | Lists gigs, fulfils bookings, and tracks income/tax estimates. |
| **Admin** *(introduced in Part 2)* | Manages the platform and oversees users/content. |

## Architecture

Part 1 implements the **backend** tier of the eventual MERN stack. The diagram below shows
where this API sits, its internal request flow, and the security boundaries around it.


flowchart TB
    subgraph Client Tier
        A[React Frontend<br/>Part 2]
        P[Postman / Newman<br/>API Test Client]
    end

    subgraph "Trust Boundary: Public Internet"
        direction TB
        A -.HTTPS/TLS.-> B
        P -.HTTPS/TLS.-> B
    end

    subgraph "HustleHub+ API — Express Application"
        B[HTTPS Listener<br/>self-signed TLS cert]
        B --> C[Security Middleware<br/>Helmet · CORS · JSON body-limit]
        C --> D[Validation Layer<br/>express-validator<br/>sanitise + allow-list rules]
        D --> E[Auth Routes<br/>/api/auth/register · /login · /profile]
        E --> F[Controllers<br/>business logic]
        F --> G[bcrypt<br/>password hashing]
        F --> H[JWT<br/>sign / verify]
        E -. protect middleware .-> H
        F --> I[(File-based User Store<br/>users.json)]
        F --> J[Global Error Handler<br/>no stack traces to client]
    end

    style B fill:#1f6feb,color:#fff
    style G fill:#c0392b,color:#fff
    style H fill:#c0392b,color:#fff
    style J fill:#8e44ad,color:#fff
```

**Security boundaries shown above:**

- **Transport boundary** — every request crosses the public internet only over HTTPS/TLS;
  plain HTTP is not served.
- **Validation boundary** — no request body reaches a controller until it has passed
  allow-list validation and sanitisation.
- **Authentication boundary** — the `protect` middleware sits directly in front of any route
  that must not be reachable without a valid JWT (`/api/auth/profile` demonstrates this today;
  every gig/booking/transaction route added in Part 2 sits behind the same gate).
- **Data boundary** — password hashes only ever leave the store as `password`; the
  `toSafeUser()` helper strips that field before any user object is serialised into an
  API response.
- **Error boundary** — the global error handler is the single exit point for failures, so
  internal detail (stack traces, file paths, library errors) can never leak to a client
  regardless of where in the code an error originated.



## Project Structure

```
hustlehub-backend/
├── certs/                      # Local TLS cert/key (generated, gitignored)
├── postman/
│   └── HustleHub-Part1.postman_collection.json
├── src/
│   ├── app.js                  # Express app: middleware stack + route mounting
│   ├── server.js                # HTTPS server bootstrap
│   ├── config/
│   │   └── env.js               # Single source of truth for environment config
│   ├── controllers/
│   │   └── authController.js    # register / login / getProfile business logic
│   ├── data/
│   │   └── users.json           # File-based user store (Part 1 only)
│   ├── middleware/
│   │   ├── authMiddleware.js    # JWT verification ("protect")
│   │   ├── errorMiddleware.js   # 404 handler + global error handler
│   │   └── validateMiddleware.js# express-validator rules + error collector
│   ├── models/
│   │   └── userModel.js         # Data access layer (find/create/sanitise user)
│   └── utils/
│       ├── AppError.js          # Operational error class
│       └── generateToken.js     # JWT signing helper
├── .env.example
├── .gitignore
├── generate-certs.sh            # Creates a local self-signed certificate
├── package.json
└── README.md
```

Routes, controllers, models, middleware, and utilities are each isolated in their own layer.
This keeps every file small and single-purpose, and means the file-based `userModel` can be


## Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) v18 or later
- npm (bundled with Node.js)
- OpenSSL (bundled with macOS/Linux; use Git Bash or WSL on Windows)
- [Postman](https://www.postman.com/) (for API testing)

### Installation

```bash
# 1. Clone the repository
git clone <your-repo-url>
cd hustlehub-backend

# 2. Install dependencies
npm install

# 3. Create your environment file
cp .env.example .env
# then open .env and set a strong, random JWT_SECRET:
node -e "console.log(require('crypto').randomBytes(64).toString('hex'))"

# 4. Generate a local SSL certificate 
npm run generate-certs

# 5. Start the server
npm run dev      # with auto-reload (nodemon)
# or
npm start         # plain node
```

You should see:

```
HustleHub+ API listening securely on https://localhost:5000
Environment: development
```

## Generating a Local SSL Certificate

The API refuses to start over plain HTTP — a valid certificate and key are required. For local
development, generate a free self-signed certificate:

```bash
npm run generate-certs
```

This runs `generate-certs.sh`, which uses OpenSSL to create `certs/key.pem` and `certs/cert.pem`,
valid for 365 days. Because the certificate is self-signed (not issued by a trusted Certificate
Authority), your browser and Postman will flag it as "not trusted" — that is expected. In
Postman, disable **Settings → General → SSL certificate verification** before sending requests.
In a real deployment, this self-signed certificate would be replaced with one issued by a
trusted CA (e.g. via Let's Encrypt).

## Environment Variables

All configuration is loaded from a single `.env` file (see `.env.example`) and centralised in
`src/config/env.js`, so there is exactly one place in the codebase that reads `process.env`.

| Variable | Description | Example |
|---|---|---|
| `PORT` | Port the HTTPS server listens on | `5000` |
| `NODE_ENV` | `development` or `production` | `development` |
| `JWT_SECRET` | Secret key used to sign/verify JWTs — must be long and random | 
| `JWT_EXPIRES_IN` | Token lifetime | `1h` |
| `BCRYPT_SALT_ROUNDS` | Cost factor for password hashing | `12` |
| `SSL_KEY_PATH` / `SSL_CERT_PATH` | Paths to the local TLS key/certificate | `./certs/key.pem` |
| `CORS_ORIGIN` | Allowed frontend origin(s) | `https://localhost:3000` |

`.env` is listed in `.gitignore` and must **never** be committed.

## API Reference

Base URL: `https://localhost:5000/api`

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| `GET` | `/health` | No | Basic liveness check |
| `POST` | `/auth/register` | No | Register a new client or freelancer |
| `POST` | `/auth/login` | No | Authenticate and receive a JWT |
| `GET` | `/auth/profile` | **Yes (JWT)** | Return the authenticated user's profile |

<details>
<summary><strong>POST /auth/register</strong></summary>

**Request body**
```json
{
  "username": "jane_doe",
  "email": "jane@example.com",
  "password": "StrongP@ss1",
  "role": "freelancer"
}
```

**Success — 201 Created**
```json
{
  "success": true,
  "message": "User registered successfully.",
  "data": {
    "user": { "id": "...", "username": "jane_doe", "email": "jane@example.com", "role": "freelancer", "createdAt": "..." },
    "token": "eyJhbGciOi..."
  }
}
```

**Failure — 409 Conflict** (duplicate email) / **400 Bad Request** (validation failure)
</details>

<details>
<summary><strong>POST /auth/login</strong></summary>

**Request body**
```json
{ "email": "jane@example.com", "password": "StrongP@ss1" }
```

**Success — 200 OK** — same shape as register.
**Failure — 401 Unauthorized** — `{ "success": false, "message": "Invalid email or password." }`
(identical message whether the email doesn't exist or the password is wrong — see
[Security Implementation](#security-implementation)).
</details>

<details>
<summary><strong>GET /auth/profile</strong> (protected)</summary>

**Header:** `Authorization: Bearer <token>`

**Success — 200 OK**
```json
{ "success": true, "message": "Profile retrieved successfully.", "data": { "user": { "id": "...", "role": "...", "email": "...", "username": "..." } } }
```

**Failure — 401 Unauthorized** — missing, malformed, expired, or invalid token.
</details>

Every response — success or failure — follows the same envelope: `{ success, message, data? }`.

## Security Implementation

### 1. Password Hashing (bcrypt)

Plain-text passwords are **never stored**. On registration, `bcrypt.hash()` derives a salted
hash (`BCRYPT_SALT_ROUNDS`, default 12) before anything touches the data store; on login,
`bcrypt.compare()` checks the supplied password against the stored hash without ever decrypting
it. bcrypt is deliberately slow and includes a per-password salt, which makes both rainbow-table
attacks and brute-force/GPU cracking impractical compared to fast hashes like plain SHA-256.

### 2. Token-Based Authentication (JWT)

On successful registration or login, the API issues a JSON Web Token signed with `JWT_SECRET`.
The payload deliberately contains **only** `id` and `role` — never the password hash or email —
because a JWT is signed, not encrypted, and its payload can be read by anyone holding the token.
Every subsequent request to a protected route (e.g. `GET /auth/profile`) must present this token
in an `Authorization: Bearer <token>` header; the `protect` middleware verifies the signature and
expiry, confirms the referenced user still exists, and only then allows the request through. This
same middleware protects every sensitive route added in Part 2.

### 3. Input Validation & Sanitisation

All request bodies pass through `express-validator` rules before reaching a controller:

- **Usernames** are restricted to 3–30 alphanumeric/underscore characters.
- **Emails** must be syntactically valid and are normalised (lower-cased, trimmed).
- **Passwords** must be 8+ characters and include upper case, lower case, a number, and a
  special character — resisting both weak-password and dictionary attacks.
- Inputs are trimmed and escaped, so HTML/script payloads (e.g. `<script>...</script>`) are
  rejected outright rather than being "cleaned" and silently accepted.

This is an **allow-list** approach: input must match what's expected, rather than trying to
blacklist "bad" characters after the fact.

### 4. HTTPS / TLS

The server only ever runs over HTTPS (`src/server.js` uses Node's `https` module, not `http`).
Without TLS, credentials and tokens sent to a plain-HTTP endpoint travel across the network in
clear text and can be read by anyone able to observe the traffic (e.g. on shared Wi-Fi). A
locally-trusted self-signed certificate is sufficient to exercise real TLS handshakes during
development; production would use a certificate from a trusted CA.

### 5. Safe, Controlled Error Handling

A single global error handler (`errorMiddleware.js`) is the only place errors are turned into
HTTP responses. Expected ("operational") errors — like a duplicate email or bad password — return
their intended message and status code. Anything unexpected (a genuine bug) is logged **in full,
server-side only**, and the client receives nothing more specific than *"Something went wrong.
Please try again later."* Stack traces, file paths, and library-specific error text never reach
the client. Login failures also use one identical message for "no such user" and "wrong password"
specifically to prevent **user enumeration** — an attacker cannot use the login endpoint to
discover which emails are registered.

### 6. Defence in Depth (bonus, hardens the surface ahead of Part 2)

- **Helmet** sets protective HTTP response headers (CSP, `X-Content-Type-Options`, HSTS, and
  hides the `X-Powered-By: Express` header that would otherwise advertise the framework).
- **CORS** is restricted to an explicit allow-list of origins rather than `*`.
- **Body-size limits** (`10kb`) on JSON/urlencoded parsing reduce exposure to oversized-payload
  abuse.

## Testing

A Postman collection is provided at `postman/HustleHub-Part1.postman_collection.json`, covering:

- Registration: success, duplicate email, missing fields, weak password, malicious/script input
- Login: success, wrong password, non-existent user
- Protected route: valid token, missing token, invalid token
- Health check and an unmatched route (confirming no stack trace leaks on a 404)

**Run via Postman UI:** import the collection, disable SSL verification (self-signed cert), run
requests top-to-bottom (Login must run after Register so the JWT variable is populated).

**Run via Newman (CLI):**
```bash
npm install -g newman
newman run postman/HustleHub-Part1.postman_collection.json --insecure
```
All 13 requests / 23 assertions pass against a freshly started server.

## Design Decisions & Trade-offs

| Decision | Rationale |
|---|---|
| File-based storage instead of a database | Explicitly permitted for Part 1; isolates all persistence in `userModel.js` so swapping in MongoDB/Mongoose for Part 2 requires no controller changes. |
| Identical error for "no user" vs "wrong password" | Prevents account enumeration via the login endpoint. |
| JWT payload limited to `id` + `role` | Minimises what's exposed if a token is intercepted; a JWT payload is readable, not encrypted. |
| Global error handler as the only response point for errors | Guarantees internal details can never leak, regardless of where an error originates. |
| Helmet/CORS/body-limits added in Part 1 | Gets ahead of Part 2's security-header requirement and hardens the attack surface from day one. |




*INSY7314 — Application Development Security, POE Part 1.*
// Developer Details 
ST10084625 - Caleb Keanu Naidoo
ST10460086 - Reece  Moodley
ST10441678 - Taytem Pillay  


