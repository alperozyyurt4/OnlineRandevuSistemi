# Uygulamanın çalışacağı base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projeyi kopyala ve restore et
COPY ["OnlineRandevuSistemi.Web/OnlineRandevuSistemi.Web.csproj", "OnlineRandevuSistemi.Web/"]
COPY ["OnlineRandevuSistemi.Business/OnlineRandevuSistemi.Business.csproj", "OnlineRandevuSistemi.Business/"]
COPY ["OnlineRandevuSistemi.DataAccess/OnlineRandevuSistemi.DataAccess.csproj", "OnlineRandevuSistemi.DataAccess/"]
COPY ["OnlineRandevuSistemi.Core/OnlineRandevuSistemi.Core.csproj", "OnlineRandevuSistemi.Core/"]
RUN dotnet restore "OnlineRandevuSistemi.Web/OnlineRandevuSistemi.Web.csproj"

# Tüm dosyaları kopyala
COPY . .

# Yayınla
WORKDIR "/src/OnlineRandevuSistemi.Web"
RUN dotnet publish "OnlineRandevuSistemi.Web.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OnlineRandevuSistemi.Web.dll"]
