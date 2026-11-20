# Build and publish the MVC application.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RaceDay.Mvc.csproj", "./"]
RUN dotnet restore "RaceDay.Mvc.csproj"
COPY . .
RUN dotnet publish "RaceDay.Mvc.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

# Run using the smaller ASP.NET Core runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
