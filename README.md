# Nexbridge Users API

Nexbridge Users API is a compact ASP.NET Core Web API that demonstrates a clean,
layered architecture for user management operations.

The API exposes CRUD endpoints for users, validates input, applies deterministic
business-result mapping, and returns standardized error responses with
`ProblemDetails`.

## Table of Contents

- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [API Documentation (Swagger)](#api-documentation-swagger)
- [Validation Rules](#validation-rules)
- [Error Handling](#error-handling)
- [Testing](#testing)
- [HTTP Client Examples](#http-client-examples)
- [Repository Maintenance](#repository-maintenance)
- [License](#license)

## Quick Start

### Prerequisites

- .NET SDK 10.0
- Git
- A REST client (browser extension, Postman, curl, or VS Code REST Client)

### Run the API

```bash
dotnet restore
dotnet run --project Nexbridge.UsersApi
```

When using HTTPS profiles, the API is available from the URL configured in
`Properties/launchSettings.json`.

## Architecture

The solution uses clear separation of concerns across well-defined layers:

```text
Nexbridge.UsersApi
├── Application/            Business logic and use cases
│   ├── Interfaces/
│   ├── Results/
│   ├── Services/
│   └── Validation/
├── Contracts/
│   └── Users/             API request/response contracts
├── Controllers/            HTTP surface area
├── Domain/                 Core abstractions and entities
│   ├── Abstractions/
│   └── Entities/
├── Infrastructure/
│   └── Persistence/       Repository implementation
└── Middleware/             Cross-cutting request behavior

Nexbridge.UsersApi.Tests
├── Integration/
├── Testing/
└── Unit/
```

Key design choices:

- Business rules are encapsulated in `Application` services.
- Persistence is isolated behind domain abstractions.
- The API boundary uses typed contracts for request and response payloads.
- Unexpected exceptions are handled by centralized middleware.

## Configuration

The API is safe to run without API key authentication by default.
To enable API key protection, set:

- `Security:ApiKey` in `appsettings.json`
- or with environment variable `Security__ApiKey`

When configured, every request must include:

```text
X-Api-Key: your_secret_key
```

If the key is missing or invalid, the API returns a `401 Unauthorized`
`ProblemDetails` response.

## API Reference

Base path: `/users`.

| Method | Endpoint | Description | Success Response |
|---|---|---|---|
| `GET` | `/users` | Retrieve all users | `200 OK` (`UserResponse[]`) |
| `GET` | `/users/{id}` | Retrieve one user by identifier | `200 OK` (`UserResponse`) |
| `POST` | `/users` | Create a new user | `201 Created` (`UserResponse`) |
| `PUT` | `/users/{id}` | Replace an existing user | `200 OK` (`UserResponse`) |
| `DELETE` | `/users/{id}` | Remove a user | `204 No Content` |

### Example payload

```json
{
  "firstName": "Ana",
  "lastName": "Taylor",
  "email": "ana@example.com",
  "age": 34
}
```

### Typical status codes

- `400 Bad Request` – validation errors
- `404 Not Found` – user does not exist
- `409 Conflict` – duplicate email
- `500 Internal Server Error` – unexpected failure

## API Documentation (Swagger)

Interactive documentation is available with Swagger UI in Development:

- Swagger JSON: `http://localhost:<port>/swagger/v1/swagger.json`
- Swagger UI: `http://localhost:<port>/swagger`

For this repository default local endpoint naming, `<port>` is the HTTP port shown
when running `dotnet run --project Nexbridge.UsersApi`.

## Validation Rules

- `firstName` and `lastName` are required and limited to 100 characters.
- `email` is required, limited to 254 characters, and must be valid.
- `age` must be between `1` and `120`.
- Input values are normalized (`trim` + email lowercase) before processing.

## Error Handling

Business outcomes are represented by `UserResult` and mapped as:

- `InvalidInput` → `400` with `ValidationProblemDetails`
- `NotFound` → `404` with `ProblemDetails`
- `EmailConflict` / update conflict → `409` with `ProblemDetails`
- Unexpected exceptions → `500` with generic `ProblemDetails`

## Testing

Run the complete test suite from the repository root:

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

## HTTP Client Examples

Use the included file:

- `Nexbridge.UsersApi/Nexbridge.UsersApi.http`

It contains ready-to-run examples for:

- List users
- Create user
- Get user by id
- Update user
- Delete user
- Validation and conflict scenarios
- API key scenarios

## Repository Maintenance

- Build artifacts are excluded via `.gitignore` (`bin/`, `obj/`, `.vs/`, etc.).
- The repository is organized for incremental learning and clean API experimentation.

## License

This project is licensed under the [MIT License](LICENSE).
