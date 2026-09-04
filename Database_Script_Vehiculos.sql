-- =========================================================================
-- SISTEMA DISTRIBUIDO DE GESTIÓN DE CATEGORÍAS Y VEHÍCULOS (MICROSERVICIOS)
-- Base de Datos: VehiculosDB
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'VehiculosDB')
BEGIN
    CREATE DATABASE VehiculosDB;
    PRINT 'Base de datos VehiculosDB creada exitosamente.';
END
GO

USE VehiculosDB;
GO

-- 1. Tabla Categorias (Manejada por CategoriasMicroservicio.Api)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categorias')
BEGIN
    CREATE TABLE Categorias (
        IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(250) NULL,
        Estado BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Categorias creada exitosamente.';
END
GO

-- 2. Tabla Vehiculos (Manejada por VehiculosMicroservicio.Api)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehiculos')
BEGIN
    CREATE TABLE Vehiculos (
        IdVehiculo INT IDENTITY(1,1) PRIMARY KEY,
        IdCategoria INT NOT NULL,
        Marca VARCHAR(100) NOT NULL,
        Modelo VARCHAR(100) NOT NULL,
        Precio DECIMAL(12,2) NOT NULL DEFAULT 0.00,
        Stock INT NOT NULL DEFAULT 0,
        Estado BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Vehiculos creada exitosamente.';
END
GO

-- 3. Datos Iniciales de Prueba
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'SUV')
BEGIN
    INSERT INTO Categorias (Nombre, Descripcion, Estado)
    VALUES ('SUV', 'Vehículos utilitarios deportivos familiares', 1);

    DECLARE @CatId INT = SCOPE_IDENTITY();

    INSERT INTO Vehiculos (IdCategoria, Marca, Modelo, Precio, Stock, Estado)
    VALUES (@CatId, 'Toyota', 'RAV4 2026', 38500.00, 5, 1);
END
GO

SELECT * FROM Categorias;
SELECT * FROM Vehiculos;
GO
