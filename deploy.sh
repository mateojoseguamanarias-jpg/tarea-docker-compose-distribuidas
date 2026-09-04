#!/bin/bash
set -e

echo "========================================================================="
echo "INICIANDO DESPLIEGUE AUTOMATICO DE MICROSERVICIOS EN AZURE CONTAINER APPS"
echo "========================================================================="

export RESOURCE_GROUP="rg-distribuidos-vehiculos"
export ACR_NAME="acrvehiculos13052"
export SQL_SERVER_NAME="sqlvehiculos1788531395"
export ENVIRONMENT_NAME="env-distribuidos-vehiculos"
export LOCATION="southcentralus"

echo "1. Obteniendo credenciales de ACR..."
export ACR_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
export ACR_PASS=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" --output tsv)
export SQL_CONN="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=VehiculosDB;Persist Security Info=False;User ID=sqladmin;Password=PassVehiculos2026!#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

echo "2. Creando Entorno de Container Apps ($ENVIRONMENT_NAME)..."
az containerapp env create \
  --name $ENVIRONMENT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --output table || true

echo "3. Desplegando RabbitMQ..."
az containerapp create \
  --name rabbitmq-service \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image rabbitmq:3-management \
  --target-port 5672 \
  --ingress internal \
  --env-vars RABBITMQ_DEFAULT_USER=admin RABBITMQ_DEFAULT_PASS=admin123 \
  --cpu 0.5 --memory 1.0Gi \
  --output table

echo "4. Desplegando OAuth/JWT API..."
az containerapp create \
  --name oauthjwt-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image $ACR_SERVER/oauthjwt-api:v1 \
  --registry-server $ACR_SERVER \
  --registry-username $ACR_NAME \
  --registry-password $ACR_PASS \
  --target-port 8080 \
  --ingress external \
  --env-vars \
    Jwt__Key="SuperSecretKeyForDistributedSystemsVehiculos2026!#Key" \
    Jwt__Issuer="OAuthJwtService" \
    Jwt__Audience="SistemaDistribuidosVehiculos" \
    Jwt__ExpiresInMinutes="60" \
  --cpu 0.25 --memory 0.5Gi \
  --output table

echo "5. Desplegando Categorias API..."
az containerapp create \
  --name categorias-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image $ACR_SERVER/categorias-api:v1 \
  --registry-server $ACR_SERVER \
  --registry-username $ACR_NAME \
  --registry-password $ACR_PASS \
  --target-port 8080 \
  --ingress internal \
  --env-vars \
    ConnectionStrings__CategoriasConnection="$SQL_CONN" \
    RabbitMQ__HostName="rabbitmq-service" \
    RabbitMQ__Port="5672" \
    RabbitMQ__UserName="admin" \
    RabbitMQ__Password="admin123" \
    Jwt__Key="SuperSecretKeyForDistributedSystemsVehiculos2026!#Key" \
    Jwt__Issuer="OAuthJwtService" \
    Jwt__Audience="SistemaDistribuidosVehiculos" \
  --cpu 0.25 --memory 0.5Gi \
  --output table

echo "6. Desplegando Vehiculos API..."
az containerapp create \
  --name vehiculos-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image $ACR_SERVER/vehiculos-api:v1 \
  --registry-server $ACR_SERVER \
  --registry-username $ACR_NAME \
  --registry-password $ACR_PASS \
  --target-port 8080 \
  --ingress internal \
  --env-vars \
    ConnectionStrings__VehiculosConnection="$SQL_CONN" \
    RabbitMQ__HostName="rabbitmq-service" \
    RabbitMQ__Port="5672" \
    RabbitMQ__UserName="admin" \
    RabbitMQ__Password="admin123" \
    Jwt__Key="SuperSecretKeyForDistributedSystemsVehiculos2026!#Key" \
    Jwt__Issuer="OAuthJwtService" \
    Jwt__Audience="SistemaDistribuidosVehiculos" \
  --cpu 0.25 --memory 0.5Gi \
  --output table

echo "7. Desplegando API Gateway..."
az containerapp create \
  --name apigateway-yarp \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image $ACR_SERVER/apigateway-yarp:v1 \
  --registry-server $ACR_SERVER \
  --registry-username $ACR_NAME \
  --registry-password $ACR_PASS \
  --target-port 8080 \
  --ingress external \
  --cpu 0.25 --memory 0.5Gi \
  --output table

echo "========================================================================="
echo "DESPLIEGUE FINALIZADO EXITOSAMENTE. OBTENIENDO URLS PUBLICAS:"
echo "========================================================================="
GATEWAY_URL=$(az containerapp show --name apigateway-yarp --resource-group $RESOURCE_GROUP --query "properties.configuration.ingress.fqdn" --output tsv)
AUTH_URL=$(az containerapp show --name oauthjwt-api --resource-group $RESOURCE_GROUP --query "properties.configuration.ingress.fqdn" --output tsv)

echo "URL API GATEWAY: https://$GATEWAY_URL"
echo "URL OAUTH JWT:   https://$AUTH_URL"
echo "========================================================================="
