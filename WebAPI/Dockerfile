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

# WeasyPrint dependencies (pinned pip build: Debian apt ships v61 with a broken
# RTL ToUnicode map — text selects as garbage in pdf.js. v70 maps glyphs right.)
RUN apt-get update && apt-get install -y \
    libcairo2 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libgdk-pixbuf-2.0-0 \
    libffi-dev \
    fontconfig \
    fonts-dejavu \
    fonts-liberation2 \
    fonts-noto-core \
    shared-mime-info \
    python3 \
    python3-pip \
    && rm -rf /var/lib/apt/lists/*

RUN pip install --break-system-packages --no-cache-dir "weasyprint==70.0" brotli

COPY --from=build /app/publish .
COPY fonts ./fonts

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebAPI.dll"]
