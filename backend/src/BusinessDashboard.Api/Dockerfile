FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend ./backend
RUN dotnet restore backend/BusinessDashboard.sln
RUN dotnet publish backend/src/BusinessDashboard.Api/BusinessDashboard.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BusinessDashboard.Api.dll"]
