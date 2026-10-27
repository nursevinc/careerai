# ---------- Derleme aşaması ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce sadece proje dosyaları: bağımlılık katmanı, kaynak kod değişince yeniden indirilmez
COPY nuget.config ./
COPY CareerAI.Domain/CareerAI.Domain.csproj CareerAI.Domain/
COPY CareerAI.Application/CareerAI.Application.csproj CareerAI.Application/
COPY CareerAI.Infrastructure/CareerAI.Infrastructure.csproj CareerAI.Infrastructure/
COPY CareerAI.API/CareerAI.API.csproj CareerAI.API/
RUN dotnet restore CareerAI.API/CareerAI.API.csproj

COPY CareerAI.Domain/ CareerAI.Domain/
COPY CareerAI.Application/ CareerAI.Application/
COPY CareerAI.Infrastructure/ CareerAI.Infrastructure/
COPY CareerAI.API/ CareerAI.API/
RUN dotnet publish CareerAI.API/CareerAI.API.csproj -c Release -o /app/publish --no-restore

# ---------- Çalışma aşaması: küçük imaj, SDK yok ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# CV dosyaları için klasör; uygulama root değil, yetkisiz kullanıcıyla çalışır
RUN mkdir -p /app/uploads && chown $APP_UID /app/uploads
COPY --from=build /app/publish .
USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CareerAI.API.dll"]