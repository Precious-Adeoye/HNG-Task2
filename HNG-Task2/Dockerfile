# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HNG-Task2/HNG-Task2.csproj", "HNG-Task2/"]
RUN dotnet restore "./HNG-Task2/HNG-Task2.csproj"
COPY . .
WORKDIR "/src/HNG-Task2"
RUN dotnet build "./HNG-Task2.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./HNG-Task2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HNG_Task2.dll"]