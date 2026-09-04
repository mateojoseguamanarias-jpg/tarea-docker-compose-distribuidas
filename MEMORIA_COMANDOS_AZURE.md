# MEMORIA DE COMANDOS DE AZURE (CLI)
**Asignatura:** Aplicaciones Distribuidas - Actividad Autónoma (AA)  
**Tema:** Sistema Distribuido de Vehículos y Categorías con OAuth/JWT  
**Estudiante:** Mateo Guamán  

---

## 1. Inicio de Sesión y Verificación de Cuenta
```bash
# 1.1 Iniciar sesión interactiva en Azure
az login

# 1.2 Listar suscripciones disponibles
az account list --output table

# 1.3 Seleccionar la suscripción activa (reemplazar con su Subscription ID)
az account set --subscription "<TU_SUBSCRIPTION_ID>"

# 1.4 Verificar la suscripción establecida
az account show --output table
```

---

## 2. Creación del Grupo de Recursos (Resource Group)
```bash
# Variables de entorno base
export RESOURCE_GROUP="rg-distribuidos-vehiculos"
export LOCATION="eastus"

# 2.1 Crear el Resource Group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --output table
```

---

## 3. Creación de Azure Container Registry (ACR)
```bash
export ACR_NAME="acrvehiculosdistribuidos$RANDOM"

# 3.1 Crear el registro de contenedores (SKU Básico para optimizar costos)
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true \
  --location $LOCATION

# 3.2 Iniciar sesión en el ACR
az acr login --name $ACR_NAME

# 3.3 Obtener el Servidor de Login
az acr show --name $ACR_NAME --query loginServer --output tsv
```

---

## 4. Construcción y Publicación de Imágenes Docker en ACR
```bash
# 4.1 Construir y publicar imagen del Servicio Independiente OAuth/JWT
az acr build \
  --registry $ACR_NAME \
  --image oauthjwt-api:v1 \
  ./OAuthJwt.Api

# 4.2 Construir y publicar imagen del Microservicio de Categorías
az acr build \
  --registry $ACR_NAME \
  --image categorias-api:v1 \
  ./CategoriasMicroservicio.Api

# 4.3 Construir y publicar imagen del Microservicio de Vehículos
az acr build \
  --registry $ACR_NAME \
  --image vehiculos-api:v1 \
  ./VehiculosMicroservicio.Api

# 4.4 Construir y publicar imagen del API Gateway (YARP)
az acr build \
  --registry $ACR_NAME \
  --image apigateway-yarp:v1 \
  ./ApiGateway.Yarp

# 4.5 Listar las imágenes subidas al ACR
az acr repository list --name $ACR_NAME --output table
```

---

## 5. Creación y Configuración de Azure SQL Database
```bash
export SQL_SERVER_NAME="sql-distribuidos-vehiculos-$RANDOM"
export SQL_DB_NAME="VehiculosDB"
export SQL_ADMIN_USER="sqladmin"
export SQL_ADMIN_PASS="PassVehiculos2026!#"

# 5.1 Crear el servidor lógico de SQL
az sql server create \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --location $LOCATION \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASS

# 5.2 Habilitar regla de firewall para permitir servicios de Azure
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 5.3 Habilitar IP local para ejecutar el script SQL (opcional)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowMyClientIP \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 255.255.255.255

# 5.4 Crear la Base de Datos (Tier Básico para minimizar costos)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DB_NAME \
  --service-objective Basic \
  --edition Basic \
  --max-size 2GB
```

---

## 6. Despliegue en Azure Container Apps (ACA)
```bash
export ENVIRONMENT_NAME="env-distribuidos-vehiculos"

# 6.1 Crear el Entorno de Azure Container Apps
az containerapp env create \
  --name $ENVIRONMENT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# 6.2 Obtener credenciales del ACR
export ACR_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
export ACR_PASS=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" --output tsv)

# 6.3 Desplegar RabbitMQ en Container Apps
az containerapp create \
  --name rabbitmq-service \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT_NAME \
  --image rabbitmq:3-management \
  --target-port 5672 \
  --ingress internal \
  --env-vars RABBITMQ_DEFAULT_USER=admin RABBITMQ_DEFAULT_PASS=admin123 \
  --cpu 0.5 --memory 1.0Gi

# 6.4 Desplegar Microservicio OAuth/JWT
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
  --cpu 0.25 --memory 0.5Gi

# 6.5 Desplegar Microservicio de Categorías
export SQL_CONN="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASS};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

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
  --cpu 0.25 --memory 0.5Gi

# 6.6 Desplegar Microservicio de Vehículos
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
  --cpu 0.25 --memory 0.5Gi

# 6.7 Desplegar API Gateway (Ingreso Público)
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
  --cpu 0.25 --memory 0.5Gi
```

---

## 7. Verificación de URLs Públicas y Pruebas
```bash
# 7.1 Obtener la URL pública del API Gateway
az containerapp show \
  --name apigateway-yarp \
  --resource-group $RESOURCE_GROUP \
  --query "properties.configuration.ingress.fqdn" \
  --output tsv

# 7.2 Obtener la URL pública del Microservicio OAuth/JWT
az containerapp show \
  --name oauthjwt-api \
  --resource-group $RESOURCE_GROUP \
  --query "properties.configuration.ingress.fqdn" \
  --output tsv

# 7.3 Prueba de Login mediante cURL para obtener Token JWT
curl -X POST "https://<URL_OAUTHJWT>/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123*"}'

# 7.4 Prueba de Petición SIN token a través del API Gateway (Rechazada 401)
curl -i -X GET "https://<URL_GATEWAY>/api/Vehiculo"

# 7.5 Prueba de Petición CON token a través del API Gateway (Autorizada 200)
curl -i -X GET "https://<URL_GATEWAY>/api/Vehiculo" \
  -H "Authorization: Bearer <TOKEN_JWT>"
```

---

## 8. Eliminación y Limpieza de Recursos (Para evitar costos)
Una vez finalizada la revisión docente, ejecutar:
```bash
az group delete \
  --name $RESOURCE_GROUP \
  --yes \
  --no-wait
```
Verificar que el grupo haya sido eliminado:
```bash
az group exists --name $RESOURCE_GROUP
```
