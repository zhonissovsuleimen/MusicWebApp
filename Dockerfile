# Multi-stage Dockerfile for .NET 8 Razor Pages app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Render will map traffic to the exposed port; 8080 is a common choice
EXPOSE 8080
# Bind Kestrel to 0.0.0.0 on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy csproj first to leverage Docker layer caching for restore
COPY ["MusicWebApp/MusicWebApp.csproj", "MusicWebApp/"]
RUN dotnet restore "MusicWebApp/MusicWebApp.csproj"
# Copy the rest of the source
COPY . .
WORKDIR "/src/MusicWebApp"
RUN dotnet build "MusicWebApp.csproj" -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "MusicWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicWebApp.dll"]
