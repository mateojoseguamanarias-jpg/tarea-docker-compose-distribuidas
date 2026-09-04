FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CategoriasMicroservicio.Api.csproj", "./"]
RUN dotnet restore "./CategoriasMicroservicio.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CategoriasMicroservicio.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CategoriasMicroservicio.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CategoriasMicroservicio.Api.dll"]
