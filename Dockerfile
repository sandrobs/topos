FROM node:24-alpine AS frontend-build
WORKDIR /src/frontend
COPY src/CrmIctm.Web/package.json src/CrmIctm.Web/package-lock.json ./
RUN npm ci --ignore-scripts --no-audit
COPY src/CrmIctm.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS backend-build
WORKDIR /src
COPY NuGet.Config Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/CrmIctm.Api/CrmIctm.Api.csproj src/CrmIctm.Api/
RUN dotnet restore src/CrmIctm.Api/CrmIctm.Api.csproj --configfile NuGet.Config
COPY src/CrmIctm.Api/ src/CrmIctm.Api/
COPY src/CrmIctm.Web/public/logo-igreja.jpg src/CrmIctm.Web/public/imagem-nave-igreja.png src/CrmIctm.Web/public/
RUN dotnet publish src/CrmIctm.Api/CrmIctm.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend/dist ./wwwroot
USER root
RUN mkdir -p /app/chaves && chown -R $APP_UID:$APP_UID /app/chaves
USER $APP_UID
ENTRYPOINT ["dotnet", "CrmIctm.Api.dll"]
