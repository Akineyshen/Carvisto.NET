FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Создаем директории и устанавливаем права
RUN mkdir -p /app/Data \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chmod 777 /app/Data \
    && chmod 777 /home/app/.aspnet/DataProtection-Keys

USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Carvisto.csproj", "./"]
RUN dotnet restore "Carvisto.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./Carvisto.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Carvisto.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /app/Data && \
    chown -R 1000:1000 /app/Data && \
    chmod -R 777 /app/Data

USER $APP_UID

USER $APP_UID
VOLUME /app/Data
VOLUME /home/app/.aspnet/DataProtection-Keys
ENTRYPOINT ["dotnet", "Carvisto.dll"]