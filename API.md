# Employee Management API

Versioned with the application; current API base path: `/api`. The API uses ASP.NET Core controllers, JWT bearer authentication, and `application/problem+json` errors.

## Contents

- [Authentication](#authentication)
- [Conventions](#conventions)
- [Employees](#employees)
- [Departments](#departments)
- [User Management](#user-management)
- [Audit And Health](#audit-and-health)
- [Status Codes](#status-codes)
- [OpenAPI](#openapi)

## Authentication

`POST /api/auth/login` and `GET /health` are anonymous. All other endpoints require a bearer token, except the development sample route which requires authentication but no specific role.

```http
POST /api/auth/login
Content-Type: application/json

{
	"email": "admin@example.com",
	"password": "<password>"
}
```

Use the returned `token` value in subsequent requests:

```http
Authorization: Bearer <token>
```

Public registration is disabled. Accounts are provisioned by Admin/HR through [User Management](#user-management). Passwords must be at least eight characters and include an uppercase letter, a lowercase letter, a number, and a symbol.

## Conventions

- Request and response bodies use JSON.
- Resource IDs are positive integers.
- Employee email and department name uniqueness is case-insensitive.
- Employee create/update requests validate dates, salary, contact fields, and department assignment.
- Error bodies use ProblemDetails: `type`, `title`, `status`, `detail`, and `instance`. Validation responses also include an `errors` dictionary.
- The Employee role can read the employee directory and employee details; Admin/HR control employee mutations.

## Employees

| Method | Route | Access | Description |
|---|---|---|---|
| `GET` | `/api/employees` | Admin, HR, Employee | List employees. |
| `GET` | `/api/employees/search` | Admin, HR, Employee | Search, filter, and paginate the directory. |
| `GET` | `/api/employees/statistics` | Admin, HR | Read total, active, inactive, and average salary statistics. |
| `GET` | `/api/employees/{id}` | Admin, HR, Employee | Read one employee. |
| `POST` | `/api/employees` | Admin, HR | Create an employee record. |
| `PUT` | `/api/employees/{id}` | Admin, HR | Update an employee record. |
| `DELETE` | `/api/employees/{id}` | Admin, HR | Delete an employee record. |

### Search Parameters

`GET /api/employees/search` accepts these query parameters:

| Parameter | Type | Constraints | Default |
|---|---|---|---|
| `search` | string | Up to 100 characters; matches name or email. | none |
| `departmentId` | integer | Must be positive. | none |
| `isActive` | boolean | `true` or `false`. | none |
| `page` | integer | 1-based, 1 to 1,000,000. | `1` |
| `pageSize` | integer | 1 to 100. | `20` |

Example: `GET /api/employees/search?search=ada&departmentId=2&isActive=true&page=1&pageSize=20`.

The response contains `items`, `page`, `pageSize`, and `totalCount`.

## Departments

| Method | Route | Access | Description |
|---|---|---|---|
| `GET` | `/api/departments` | Admin, HR | List departments with employee counts. |
| `GET` | `/api/departments/{id}` | Admin, HR | Read department details. |
| `GET` | `/api/departments/{id}/employees` | Admin, HR | List employees assigned to a department. |
| `POST` | `/api/departments` | Admin | Create a department. |
| `PUT` | `/api/departments/{id}` | Admin | Update a department. |
| `DELETE` | `/api/departments/{id}` | Admin | Delete a department if it has no employees. |

Department names are limited to 100 characters; descriptions are limited to 500 characters.

## User Management

| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/api/usermanagement/users` | Admin, HR | Create an account. Admin may assign HR or Employee; HR may assign Employee only. |
| `GET` | `/api/usermanagement/me` | Any authenticated user | Read the caller's own profile. |
| `PUT` | `/api/usermanagement/me` | Any authenticated user | Update the caller's first name, last name, and phone number. Email is read-only. |
| `POST` | `/api/usermanagement/me/change-password` | Any authenticated user | Change the caller's password with the current password. |
| `POST` | `/api/usermanagement/assign-role?email={email}&role={role}` | Admin | Replace a user's current role with one role. |

Example account-creation body:

```json
{
	"firstName": "Ghaith",
	"lastName": "Alhajaj",
	"email": "Ghaith@example.com",
	"password": "<temporary strong password>",
	"role": "Employee"
}
```

## Audit And Health

| Method | Route | Access | Description |
|---|---|---|---|
| `GET` | `/api/audit?limit={1-200}` | Admin | Read the most recent successful state-changing API events. Request bodies and credentials are not recorded. |
| `GET` | `/health` | Anonymous | Check database connectivity; returns `503` if unavailable. |
| `GET` | `/WeatherForecast` | Authenticated | Development sample endpoint. |

## Status Codes

| Status | Meaning |
|---:|---|
| `200` | Request succeeded. |
| `201` | Resource created. |
| `204` | Request succeeded without a response body, such as a password change. |
| `400` | Invalid request or validation failure. |
| `401` | Missing or invalid authentication. |
| `403` | Authenticated caller lacks the required role. |
| `404` | Resource not found. |
| `409` | Duplicate email/name or other resource conflict. |
| `500` | Unexpected server error. |
| `503` | Health check cannot reach the database. |

## OpenAPI

In Development, the OpenAPI document is available at `/openapi/v1.json`. For application setup and deployment guidance, see the [project README](README.md).
