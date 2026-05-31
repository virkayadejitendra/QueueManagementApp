FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/QueueManagement.Api/QueueManagement.Api.csproj src/QueueManagement.Api/
RUN dotnet restore src/QueueManagement.Api/QueueManagement.Api.csproj

COPY src/QueueManagement.Api/ src/QueueManagement.Api/
WORKDIR /src/src/QueueManagement.Api
RUN dotnet publish QueueManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

CMD ["sh", "-c", "dotnet QueueManagement.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
