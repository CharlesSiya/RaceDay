# RaceDay – South African Road Events Platform

RaceDay is a web-based event management system for South African running, walking and cycling events. The repository is developed progressively across the three POE parts.

## Repository structure

- `docs/` – Part 1 ERD, API plan and database script.
- `PART 2/RaceDay_Part2/RaceDay.Api/` – ASP.NET Core REST API.
- `PART 2/RaceDay_Part2/RaceDay.slnx` – solution containing the API, tests and MVC project.
- `RaceDay.Api.Tests/` – automated tests.
- `PART 3/RaceDay.Mvc/` – ASP.NET Core MVC web application and Dockerfile.
- `PART 3/Database_Part3_Images.sql` – Part 3 database extension for image URLs.
- `.github/workflows/raceday-ci.yml` – repository-level CI workflow.

## Roles

**Organiser:** manage owned events and categories, view enrolments and capture results.

**Participant:** browse events, enrol in categories, view personal enrolments/results and update a profile.

## Build and test

Install the .NET 10 SDK, then run these commands from the repository root:

```bash
dotnet restore "PART 2/RaceDay_Part2/RaceDay.slnx"
dotnet build "PART 2/RaceDay_Part2/RaceDay.slnx" --configuration Release
dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

## Run the API and MVC

1. Configure the API SQL Server connection string in `PART 2/RaceDay_Part2/RaceDay.Api/appsettings.json` or via user secrets.
2. Run the API project from Visual Studio or with `dotnet run --project "PART 2/RaceDay_Part2/RaceDay.Api/RaceDay.Api.csproj"`.
3. Set `ApiSettings:BaseUrl` in `PART 3/RaceDay.Mvc/appsettings.json` to the actual API URL, including the trailing slash.
4. Run `dotnet run --project "PART 3/RaceDay.Mvc/RaceDay.Mvc.csproj"`.

Run `PART 3/Database_Part3_Images.sql` once against the existing RaceDay database before using banner/profile image uploads.

## Azure Blob Storage
Set `AzureBlobStorage:ConnectionString` and `AzureBlobStorage:ContainerName` on the API using .NET user secrets or environment variables. Do not commit connection strings or access keys. The MVC project uploads files to the API; it never accesses Azure directly.

## Docker
From `PART 3/RaceDay.Mvc`:

```bash
docker build -t raceday-mvc .
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5080/ raceday-mvc
```

Replace port `5080` with the actual HTTP port of the API. The API must be running and reachable from the container.

## CI/CD and presentation
GitHub Actions is configured in `.github/workflows/raceday-ci.yml`. Push the source to GitHub and confirm that the workflow completes successfully. Add a screenshot of the real green build and your actual unlisted YouTube presentation link to the Part 3 README before submitting.

## AI assistance disclosure
AI assistance was used to review and scaffold parts of the implementation. The student must understand, verify, test and adapt the code, and disclose AI use in accordance with the assignment instructions.
