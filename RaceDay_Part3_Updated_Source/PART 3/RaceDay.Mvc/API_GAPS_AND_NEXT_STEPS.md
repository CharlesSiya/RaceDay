# Part 3 integration notes

The Part 2 API was extended for the MVC client in this implementation.

## Event endpoints
- `GET /api/events` – public event list; supports `search` and `eventType` query parameters.
- `GET /api/events/{id}` – public event details, categories and route details.
- `GET /api/events/mine` – authenticated Organiser's own events and enrolment totals.
- `POST /api/events` – create an event and its route (Organiser only).
- `PUT /api/events/{id}` – update an owned event and route (Organiser only).
- `DELETE /api/events/{id}` – delete an owned event if it has no enrolments (Organiser only).

## Existing endpoints consumed by MVC
- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`
- `GET/POST/PUT/DELETE /api/events/{eventId}/categories`
- `POST /api/enrollments`, `GET /api/organiser/enrollments`, `GET /api/enrollments/mine`
- `POST /api/enrollments/{enrollmentId}/result`, `GET /api/results/mine`
- `GET/PUT /api/profile`

## Azure Blob Storage endpoints
- `POST /api/events/{eventId}/banner` – Organiser uploads an event banner.
- `POST /api/profile/photo` – authenticated user uploads a profile picture.

Both uploads are handled by the API using Azure Blob Storage. The API saves the resulting URL in `Events.BannerImageUrl` or `Users.ProfileImageUrl`. The MVC app forwards the multipart upload and never connects to Azure directly.

## Required local setup
1. Run `PART 3/Database_Part3_Images.sql` against the existing RaceDay database.
2. Configure the API's `AzureBlobStorage:ConnectionString` and `AzureBlobStorage:ContainerName` with .NET user secrets or environment variables.
3. Set the MVC `ApiSettings:BaseUrl` to the actual API URL.
4. Build and run the solution, then test both roles and the image upload flow.

This document describes implemented code paths; successful local execution, Azure upload, Docker operation and GitHub CI still need to be verified in the student's environment.
