# Customer Notification Preferences API

ASP.NET Core Web API (.NET 10) for viewing and updating a customer's email/SMS notification preferences. Data is held in an EF Core in-memory database; the external notification system is represented by an `IChangeNotifier` interface with a stub implementation (no network calls).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Data is held in an EF Core in-memory database
- External notification system is represented by an `IChangeNotifier` interface with a stub implementation (no network calls).

## Commands

From the repository root:

### Build
```bash
dotnet build
```
### Test
```bash
dotnet test
```
### Run
```bash
dotnet run --project SampleApi --launch-profile https
```

The API listens on `https://localhost:7083` (plain HTTP on `http://localhost:5128` redirects to HTTPS). In Development, API documentation is available at `https://localhost:7083/docs` and the OpenAPI document at `/openapi/v1.json`.

The `curl` examples below use `-k` to accept the local development certificate; run `dotnet dev-certs https --trust` once to trust it and drop the flag.

## Seeded customers

The in-memory store is seeded at startup with these fixed IDs:

| Customer ID                            | Email | SMS   |
|----------------------------------------|-------|-------|
| `6b56b62f-0bdf-4354-a822-9b1c257c5ec0` | true  | false |
| `af285565-031a-4df2-9f82-98f101dd08fa` | false | true  |
| `abb0125e-b3ed-4a66-910b-492f431f9486` | true  | true  |
| `e2906422-3298-4b81-8a78-39544d94268e` | false | false |
| `6d4977a7-a9a4-4b72-b75d-00569f699602` | true  | false |
| `de5de5f6-0d9d-472d-98be-b45db46f84ca` | false | true  |

Data is not persisted between runs.

## API

### Get preferences

```bash
curl -ik https://localhost:7083/api/customers/6b56b62f-0bdf-4354-a822-9b1c257c5ec0/notification-preferences
```

`200 OK`

```json
{ "customerId": "6b56b62f-0bdf-4354-a822-9b1c257c5ec0", "emailEnabled": true, "smsEnabled": false }
```

Unknown customer (valid GUID that is not seeded) returns `404 Not Found`:

```bash
curl -ik https://localhost:7083/api/customers/00000000-0000-0000-0000-000000000001/notification-preferences
```

An ID that is not a valid GUID returns `400 Bad Request`:

```bash
curl -ik https://localhost:7083/api/customers/not-a-guid/notification-preferences
```

### Update preferences

```bash
curl -ik -X PUT https://localhost:7083/api/customers/6b56b62f-0bdf-4354-a822-9b1c257c5ec0/notification-preferences \
  -H "Content-Type: application/json" \
  -d '{"emailEnabled": false, "smsEnabled": true}'
```

`200 OK`

```json
{ "customerId": "6b56b62f-0bdf-4354-a822-9b1c257c5ec0", "emailEnabled": false, "smsEnabled": true }
```

Unknown customer returns `404 Not Found` and no customer is created:

```bash
curl -ik -X PUT https://localhost:7083/api/customers/00000000-0000-0000-0000-000000000001/notification-preferences \
  -H "Content-Type: application/json" \
  -d '{"emailEnabled": false, "smsEnabled": true}'
```

Malformed body (invalid JSON) returns `400 Bad Request`:

```bash
curl -ik -X PUT https://localhost:7083/api/customers/6b56b62f-0bdf-4354-a822-9b1c257c5ec0/notification-preferences \
  -H "Content-Type: application/json" \
  -d '{"emailEnabled": true,'
```

Missing property returns `400 Bad Request` with an error naming the property:

```bash
curl -ik -X PUT https://localhost:7083/api/customers/6b56b62f-0bdf-4354-a822-9b1c257c5ec0/notification-preferences \
  -H "Content-Type: application/json" \
  -d '{"emailEnabled": true}'
```

| Status | When |
|--------|------|
| `200 OK` | Preferences replaced (or unchanged); the updated values are returned |
| `400 Bad Request` | Missing, malformed or invalid body, a missing `emailEnabled`/`smsEnabled` property, or an invalid GUID |
| `404 Not Found` | Unknown customer (none is created) |
| `503 Service Unavailable` | Preferences were saved but the notification system failed |

Notification behaviour: if at least one preference changes, the notifier is called exactly once with the customer ID and the updated values. It is not called for unchanged values, validation failures or unknown customers.

## Notification failure policy

If persistence succeeds but notification throws, the API returns `503` with a problem-details response, and the updated preferences **remain saved**. This is deliberately simple. Implications: a client retrying the same PUT sees "unchanged" and will not trigger another notification, so the notification can be lost. A production design would use a transactional outbox (or similar) so notifications are retried reliably.

## Design note

The solution uses a clean-architecture-inspired layering in a single project: `Domain` (entity), `Application` (DTOs, `CustomerService`, repository abstraction), `Infrastructure` (EF Core in-memory persistence, stub notifier) and `Controllers`, wired with dependency injection. It is not strict: `IChangeNotifier` lives in the Infrastructure namespace, and separate projects would be needed to enforce boundaries.

Input validation is declarative. The request DTO uses non-nullable `required` members, so a missing property, null or malformed body is rejected during binding with a `400` before the controller runs, and omitted booleans cannot default to `false`. A non-GUID route ID is rejected by binding in the same way. The controller maps service results to HTTP responses, and `CustomerService` owns the rules, notifying only when the repository reports a real change. A notification failure after persistence returns `503`.

Tests follow Arrange/Act/Assert at three levels: controller tests (result mapping, mocked dependencies), service tests (notification rules, ordering, failure handling) and integration tests (the real pipeline via `WebApplicationFactory`, an isolated in-memory database and a fake notifier). Integration tests are tagged with acceptance criteria AC01-AC10.

Known limitations: data is lost on restart; the failure policy above can drop notifications; there is no concurrency control; framework validation messages are not customised.

For production I would add:
- **Reliable notification:** a transactional outbox with retries, on a real database with optimistic concurrency.
- **Authentication and authorisation:** JWT bearer authentication, with policies so customers access only their own preferences.
- **Rate limiting:** per-client limits on the PUT endpoint.
- **CORS:** an explicit allow-list of trusted origins.
- **Health checks:** liveness and readiness endpoints covering the database and notifier.
- **Observability:** OpenTelemetry metrics and distributed tracing, plus structured logging with correlation IDs.

## AI tools used

GitHub Copilot (in VS Code) was used to help refine some tests, generate seed data and this documentation. All output was reviewed, and verified by building the solution and running the full automated test suite.
