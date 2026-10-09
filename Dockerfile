FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN mkdir -p /app/data && chown app:app /app/data
RUN mkdir -p /app/keys && chown app:app /app/keys
RUN mkdir -p /app/data/ThingImages && chown app:app /app/data/ThingImages
VOLUME /app/data
VOLUME /app/keys
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["src/src.csproj", "web/"]
RUN dotnet restore "web/src.csproj"
WORKDIR "/src/web"
COPY . .
RUN dotnet build "./src/src.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./src/src.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "src.dll"]