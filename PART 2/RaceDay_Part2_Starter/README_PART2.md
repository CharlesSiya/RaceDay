# RaceDay Part 2 — ASP.NET Core Web API

## Prerequisites
- Visual Studio with ASP.NET and web development workload
- .NET 10 SDK
- SQL Server LocalDB
- Git

## Database setup
The API uses EF Core Code-First migrations. In Visual Studio Package Manager Console, set Default project to `RaceDay.Api`, then run:
```powershell
Add-Migration InitialRaceDay
Update-Database
```
The database connection is configured in `appsettings.json` as `RaceDayDB`.

## Run
Set `RaceDay.Api` as startup project and press F5. In Development, open the Swagger URL shown by Visual Studio.

## Authentication
- `POST /api/auth/register` accepts Participant or Organiser. For safety, Organiser registration requires the configured `OrganiserRegistrationCode`; change the placeholder in local settings before using it.
- `POST /api/auth/login` establishes an HTTP-only cookie and server-side session values.
- `POST /api/auth/logout` clears the session.
- Do not commit real secrets or production connection strings.

## Roles
- Organiser: manages their own events and categories, sees enrolments for their own events, captures results.
- Participant: enrols in categories and views their own enrolments/results.
- Both roles can browse events and categories and update their own profile.

## Endpoints
- Auth: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`
- Profile: `GET /api/profile`, `PUT /api/profile`
- Events: `GET /api/events`, `GET /api/events/{id}`, `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}`
- Categories: `GET /api/events/{eventId}/categories`, `POST /api/events/{eventId}/categories`, `PUT /api/events/{eventId}/categories/{categoryId}`, `DELETE /api/events/{eventId}/categories/{categoryId}`
- Enrolments: `POST /api/enrollments`, `GET /api/enrollments/mine`, `GET /api/organiser/enrollments`
- Results: `POST /api/enrollments/{enrollmentId}/result`, `GET /api/results/mine`

## Important
This starter maps the ERD's six tables and relationships. `EventType` is an additional Events column required by the Part 2 specification. Distance for an event is stored in its Route record and is returned with the event. Compare your existing Part 1 SQL `CREATE TABLE` definitions and approved ERD before applying migrations to a database with existing data.
