# Multi-stage Dockerfile for Digital Memory Map
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for caching restore layer
COPY ["DigitalMemoryMap.Web/DigitalMemoryMap.Web.csproj", "DigitalMemoryMap.Web/"]
COPY ["DigitalMemoryMap.BLL/DigitalMemoryMap.BLL.csproj", "DigitalMemoryMap.BLL/"]
COPY ["DigitalMemoryMap.DAL/DigitalMemoryMap.DAL.csproj", "DigitalMemoryMap.DAL/"]
RUN dotnet restore "DigitalMemoryMap.Web/DigitalMemoryMap.Web.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/DigitalMemoryMap.Web"
RUN dotnet publish "DigitalMemoryMap.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DigitalMemoryMap.Web.dll"]
