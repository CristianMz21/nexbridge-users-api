# Nexbridge Users API

An ASP.NET Core Web API that manages users in memory using a clean, layered
architecture for learning and demonstration purposes.

## What this API provides

- Create, list, update, and delete users
- Input validation and normalized payloads
- Conflict and not-found error handling with RFC 7807 `ProblemDetails`
- Optional API key authentication via `X-Api-Key`
- Layered project structure (`Application`, `Domain`, `Infrastructure`, `Contracts`)
- Unit and integration tests with `xUnit`

## Tech stack

- .NET 10 / ASP.NET Core Web API
- Minimal hosting model with controllers
- OpenAPI in development

## Project structure

```text
Nexbridge.UsersApi
├── Application/
│   ├── Interfaces/
│   ├── Results/
│   ├── Services/
│   └── Validation/
├── Contracts/
│   └── Users/
├── Controllers/
├── Domain/
│   ├── Abstractions/
│   └── Entities/
├── Infrastructure/
│   └── Persistence/
└── Middleware/

Nexbridge.UsersApi.Tests
├── Integration/
├── Testing/
└── Unit/
```

## Endpoints

Base path: `/users`

### GET `/users`

- Returns all users
- `200 OK` with `UserResponse[]`

### GET `/users/{id}`

- Returns one user by id
- `200 OK` with `UserResponse`
- `404 Not Found` with `ProblemDetails` when missing

### POST `/users`

- Creates a new user
- Request body:

  ```json
  {
    "firstName": "Ana",
    "lastName": "Taylor",
    "email": "ana@example.com",
    "age": 34
  }
  ```

- Success: `201 Created` with created resource and `Location: /users/{id}`
- Validation errors: `400 Bad Request` with `ValidationProblemDetails`
- Duplicate email: `409 Conflict` with `ProblemDetails`

### PUT `/users/{id}`

- Updates an existing user
- Same payload shape as POST
- Success: `200 OK` with updated `UserResponse`
- Missing user: `404 Not Found`
- Duplicate email: `409 Conflict`
- Validation: `400 Bad Request`

### DELETE `/users/{id}`

- Deletes an existing user
- Success: `204 No Content`
- Missing user: `404 Not Found`

## Data and validation rules

- `firstName` and `lastName` are required and must be at most 100 characters
- `email` is required, max 254 characters, and must be valid format
- `age` must be between 1 and 120
- Inputs are normalized (`trim`, email lower-case)

## Storage model

The API uses an in-memory repository (`InMemoryUserRepository`) for now.
State is process-lifetime only and resets on restart.

## Error handling

- Expected business errors return deterministic `UserResult` outcomes from the
  application layer and map to `400`, `404`, or `409`.
- Unexpected errors are caught by global middleware and returned as a generic
  `500` `ProblemDetails` response.
- Optional API key middleware is active when `Security:ApiKey` is configured.

## Run locally

```bash
dotnet restore
dotnet run --project Nexbridge.UsersApi
```

By default, HTTPS redirection is enabled and the API serves on the configured
launch URL from `launchSettings.json`.

### Api key

Configure with app settings or environment variables:

- `Security:ApiKey` (for example `your_secret_key`)

When set, every request must include:

```text
X-Api-Key: your_secret_key
```

When unset/blank, authentication is skipped.

## Tests

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

## License

This project is licensed under the [MIT License](LICENSE).

## Creating/pushing to GitHub with `gh`

- This project is ready to be pushed to an existing GitHub repository.
- If you want to create a new repository from scratch, use:

```bash
gh repo create <owner>/<repo> --source . --public --push
```

- If the repository already exists, just push:

```bash
git push
```
