# TRABAJO AUTÓNOMO (AA) - APLICACIONES DISTRIBUIDAS
## Arquitectura Distribuida Segura con Microservicios, OAuth/JWT, RabbitMQ y Despliegue en Azure

**Asignatura:** Aplicaciones Distribuidas  
**Tema Asignado:** Sistema de Gestión de Categorías y Vehículos  
**Estudiante:** Mateo Guamán  
**Fecha:** Septiembre 2026  

---

## 1. Descripción General del Proyecto
Este proyecto implementa una **arquitectura de microservicios distribuida, desacoplada y altamente escalable**, asegurada mediante un **servicio independiente de autenticación y autorización basado en JSON Web Tokens (OAuth/JWT)** y comunicación asíncrona orientada a eventos mediante **RabbitMQ**.

La solución completa está contenedorizada mediante **Docker / Docker Compose** y desplegada en la nube utilizando los servicios de **Microsoft Azure** (Azure Container Apps, Azure Container Registry y Azure SQL Database).

---

## 2. Diagrama de Arquitectura

```
                        +----------------------------+
                        |      Cliente / Postman     |
                        +--------------+-------------+
                                       |
                                       v
                        +----------------------------+
                        |     API Gateway (YARP)     |
                        |      Puerto: 5005 / ACA    |
                        +---+--------------------+---+
                            |                    |
            +---------------+                    +---------------+
            |                                                    |
            v                                                    v
+-----------------------+                            +-----------------------+
|  Microservicio 1      |      Evento RabbitMQ       |  Microservicio 2      |
|  CategoriasMicro.Api  | -------------------------> |  VehiculosMicro.Api   |
|  Puerto: 5001 / ACA   |                            |  Puerto: 5003 / ACA   |
+-----------+-----------+                            +-----------+-----------+
            |                                                    |
            +--------------------+         +---------------------+
                                 |         |
                                 v         v
                        +----------------------------+
                        |     Azure SQL Database     |
                        |       (VehiculosDB)        |
                        +----------------------------+
                                       ^
                                       | Valida Token
+-----------------------+              |
| Servicio OAuth/JWT    | -------------+
| OAuthJwt.Api          | (Emite JWT con Issuer, Audience y Secret Key)
| Puerto: 5000 / ACA    |
+-----------------------+
```

---

## 3. Descripción de los Componentes

1. **OAuthJwt.Api (Servicio Independiente de Autenticación):**
   * Emite y gestiona tokens de acceso JWT.
   * Maneja parámetros de configuración criptográfica: `Issuer`, `Audience`, `Key` y `Expiration`.
   * Desacopla la lógica de seguridad y credenciales de los microservicios de negocio.
   
2. **CategoriasMicroservicio.Api (Microservicio 1):**
   * Gestiona el CRUD de categorías de vehículos.
   * Protegido mediante middleware `JwtBearer` (requiere token válido).
   * Actúa como **Publisher (Emisor)** de eventos en RabbitMQ cada vez que se crea una nueva categoría.

3. **VehiculosMicroservicio.Api (Microservicio 2):**
   * Gestiona el inventario, características y precios de los vehículos.
   * Protegido mediante middleware `JwtBearer`.
   * Actúa como **Consumer (Receptor)** de eventos en RabbitMQ mediante un `BackgroundService` (`RabbitMQEventConsumer`) para sincronización en tiempo real.

4. **RabbitMQ Message Broker:**
   * Gestiona la cola `categoria_creada` para la comunicación asíncrona entre microservicios.

5. **ApiGateway.Yarp (Reverse Proxy):**
   * Punto de entrada único para clientes externos.
   * Enruta tráfico inteligente hacia `OAuthJwt.Api`, `CategoriasMicroservicio.Api` y `VehiculosMicroservicio.Api`.

---

## 4. Instrucciones para Ejecución Local con Docker Compose

### Requisitos Previos:
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en ejecución.
* [.NET 10 SDK](https://dotnet.microsoft.com/download) (opcional para desarrollo local).

### Pasos de ejecución:
1. Clonar el repositorio y ubicarse en la raíz del proyecto:
   ```bash
   cd SistemaDistribuidos_Vehiculos
   ```

2. Levantar la arquitectura completa con Docker Compose:
   ```bash
   docker compose up --build
   ```

3. Puertos expuestos en entorno local:
   * **API Gateway (YARP):** `http://localhost:5005`
   * **OAuth/JWT API:** `http://localhost:5000` (Swagger: `http://localhost:5000/index.html`)
   * **Categorías API:** `http://localhost:5001` (Swagger: `http://localhost:5001/swagger`)
   * **Vehículos API:** `http://localhost:5003` (Swagger: `http://localhost:5003/swagger`)
   * **RabbitMQ Management Dashboard:** `http://localhost:15672` (User: `admin` / Pass: `admin123`)

---

## 5. Procedimiento para Obtener un Token JWT y Probar el Sistema

### 5.1 Credenciales de Prueba
| Usuario | Contraseña | Rol Asignado |
| :--- | :--- | :--- |
| `admin` | `Admin123*` | `Administrador` |
| `operador` | `Operador123*` | `Operador` |
| `docente` | `Docente2026*` | `Evaluador` |

---

### 5.2 Obtención del Token (POST /api/auth/login)
**Petición cURL:**
```bash
curl -X POST "http://localhost:5005/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123*"
  }'
```

**Respuesta Exitosa (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "username": "admin",
  "role": "Administrador",
  "issuedAt": "2026-09-03T19:50:00Z",
  "expiresAt": "2026-09-03T20:50:00Z"
}
```

---

### 5.3 Demostración de Seguridad JWT

#### ❌ Caso 1: Petición Rechazada (Sin Token / Token Inválido)
```bash
curl -i -X GET "http://localhost:5005/api/Vehiculo"
```
**Respuesta esperada:**
```http
HTTP/1.1 401 Unauthorized
```

#### ✅ Caso 2: Petición Autorizada (Con Bearer Token)
```bash
curl -i -X GET "http://localhost:5005/api/Vehiculo" \
  -H "Authorization: Bearer <PEGAR_TOKEN_AQUI>"
```
**Respuesta esperada:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "idVehiculo": 1,
    "idCategoria": 1,
    "marca": "Toyota",
    "modelo": "RAV4 2026",
    "precio": 38500.00,
    "stock": 5,
    "estado": true
  }
]
```

---

## 6. Listado de Endpoints Principales

| Servicio | Método | Ruta a través del Gateway | Descripción | Requiere JWT |
| :--- | :--- | :--- | :--- | :---: |
| **OAuth/JWT** | `POST` | `/api/Auth/login` | Autenticación y generación de JWT | No |
| **OAuth/JWT** | `GET` | `/api/Auth/status` | Verificación de parámetros criptográficos | No |
| **Categorías** | `GET` | `/api/Categoria` | Listar todas las categorías | **Sí** |
| **Categorías** | `GET` | `/api/Categoria/{id}` | Obtener categoría por ID | **Sí** |
| **Categorías** | `POST` | `/api/Categoria` | Crear categoría (Dispara evento RabbitMQ) | **Sí** |
| **Categorías** | `PUT` | `/api/Categoria/{id}` | Actualizar categoría existente | **Sí** |
| **Categorías** | `DELETE`| `/api/Categoria/{id}` | Eliminar categoría | **Sí** |
| **Vehículos** | `GET` | `/api/Vehiculo` | Listar todos los vehículos | **Sí** |
| **Vehículos** | `GET` | `/api/Vehiculo/{id}` | Obtener vehículo por ID | **Sí** |
| **Vehículos** | `GET` | `/api/Vehiculo/categoria/{id}`| Filtrar vehículos por categoría | **Sí** |
| **Vehículos** | `POST` | `/api/Vehiculo` | Registrar nuevo vehículo | **Sí** |
| **Vehículos** | `PUT` | `/api/Vehiculo/{id}` | Actualizar vehículo | **Sí** |
| **Vehículos** | `DELETE`| `/api/Vehiculo/{id}` | Eliminar vehículo | **Sí** |

---

## 7. Servicios Desplegados en Azure (URLs Públicas)

> **Servicios activos y en ejecución en Microsoft Azure:**

* 🌐 **API Gateway (Ingreso Principal):** [https://apigateway-yarp.bluepond-fbb1e37f.southcentralus.azurecontainerapps.io](https://apigateway-yarp.bluepond-fbb1e37f.southcentralus.azurecontainerapps.io)
* 🔐 **Microservicio OAuth/JWT:** [https://oauthjwt-api.bluepond-fbb1e37f.southcentralus.azurecontainerapps.io](https://oauthjwt-api.bluepond-fbb1e37f.southcentralus.azurecontainerapps.io)
* 🗄️ **Azure SQL Server:** `sqlvehiculos1788531395.database.windows.net` (Base de datos: `VehiculosDB`)
* 📦 **Azure Container Registry (ACR):** `acrvehiculos13052.azurecr.io`
* 🐰 **RabbitMQ Message Broker:** `rabbitmq-service.internal.bluepond-fbb1e37f.southcentralus.azurecontainerapps.io`

---

## 8. Limpieza de Recursos de Azure (Control de Costos)

Para evitar consumos posteriores a la fecha de revisión (13/09/2026), se eliminan todos los recursos con un solo comando:

```bash
az group delete --name rg-distribuidos-vehiculos --yes --no-wait
```
