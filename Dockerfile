# Stage 1: Build .NET 10 Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore
COPY backend/MiniAts.Api/MiniAts.Api.csproj backend/MiniAts.Api/
RUN dotnet restore backend/MiniAts.Api/MiniAts.Api.csproj

# Copy source code and publish
COPY backend/MiniAts.Api/ backend/MiniAts.Api/
WORKDIR /src/backend/MiniAts.Api
RUN dotnet publish MiniAts.Api.csproj -c Release -o /app/out

# Stage 2: Minimal ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose default HTTP port
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "MiniAts.Api.dll"]
