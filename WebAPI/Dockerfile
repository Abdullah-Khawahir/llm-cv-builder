# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj directly (NO nested WebAPI/WebAPI.csproj)
COPY WebAPI.csproj ./

RUN dotnet restore WebAPI.csproj

# Copy everything
COPY . .

RUN dotnet publish WebAPI.csproj -c Release -o /app/publish

# ===== Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# WeasyPrint dependencies
RUN apt-get update && apt-get install -y \
    libcairo2 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libgdk-pixbuf-2.0-0 \
    libffi-dev \
    fontconfig \
    fonts-dejavu \
    shared-mime-info \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebAPI.dll"]
