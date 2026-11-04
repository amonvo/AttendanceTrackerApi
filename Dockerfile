FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore AttendanceTrackerApi.csproj --locked-mode
RUN dotnet publish AttendanceTrackerApi.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "AttendanceTrackerApi.dll"]
