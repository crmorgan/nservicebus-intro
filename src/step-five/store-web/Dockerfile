FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY store-web/store-web.csproj store-web/
RUN dotnet restore store-web/store-web.csproj
COPY . .
WORKDIR /src/store-web
RUN dotnet build store-web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish store-web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "store-web.dll"]
