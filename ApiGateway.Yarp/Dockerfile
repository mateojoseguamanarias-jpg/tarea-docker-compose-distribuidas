FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ApiGateway.Yarp.csproj", "./"]
RUN dotnet restore "./ApiGateway.Yarp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ApiGateway.Yarp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.Yarp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.Yarp.dll"]
