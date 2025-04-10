FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
COPY appsettings.json /app/appsettings.json
RUN dotnet restore "workshop-web-app.csproj"
RUN dotnet build "workshop-web-app.csproj" -c Release -o /app/build
RUN dotnet publish "workshop-web-app.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "workshop-web-app.dll"]