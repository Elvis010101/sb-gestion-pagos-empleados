/* ===========================================================================
   SB.GestionPagos — Script inicial de base de datos
   ---------------------------------------------------------------------------
   Entregable exigido por la prueba técnica (p. 8): "script .sql o migraciones".
   Aquí están las dos cosas, y NO son dos fuentes de verdad distintas: todo lo
   que sigue al encabezado está GENERADO a partir de la migración
   `MigracionInicial` con:

       dotnet ef migrations script --idempotent

   Es decir, el script no se mantiene a mano. Se regenera. Si alguien editara
   este archivo en vez de la migración, el próximo `dotnet ef database update`
   dejaría la base y el script diciendo cosas distintas.

   IDEMPOTENTE: cada bloque comprueba en `__EFMigrationsHistory` si su
   migración ya se aplicó. Ejecutar el script dos veces no duplica nada ni
   falla, que es justo lo que hace falta cuando quien lo corre es un evaluador
   sobre una base en estado desconocido.

   Uso:
       sqlcmd -S localhost,1433 -U sa -P <contraseña> -i db/script-inicial.sql

   Credenciales sembradas (demostración — documentadas en el README):
       admin   / Admin123!    → rol Administrador
       usuario / Usuario123!  → rol Usuario
   =========================================================================== */

/* La creación de la base NO la genera EF: `dotnet ef` asume que la base existe
   o la crea él mismo al aplicar la migración. Como este script se entrega para
   ejecutarse a mano, tiene que poder partir de un servidor vacío. */
IF DB_ID(N'SbGestionPagos') IS NULL
BEGIN
    CREATE DATABASE [SbGestionPagos];
END;
GO

USE [SbGestionPagos];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE TABLE [Empleados] (
        [Id] int NOT NULL IDENTITY,
        [PrimerNombre] nvarchar(100) NOT NULL,
        [ApellidoPaterno] nvarchar(100) NOT NULL,
        [NumeroSeguroSocial] nvarchar(20) NOT NULL,
        [Departamento] nvarchar(100) NOT NULL,
        [Estado] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [TipoEmpleado] nvarchar(40) NOT NULL,
        [SalarioSemanal] decimal(18,2) NULL,
        [VentasBrutas] decimal(18,2) NULL,
        [TarifaComision] decimal(5,4) NULL,
        [SalarioBase] decimal(18,2) NULL,
        [SueldoPorHora] decimal(18,2) NULL,
        [HorasTrabajadas] decimal(6,2) NULL,
        CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [NombreUsuario] nvarchar(50) NOT NULL,
        [HashContrasena] nvarchar(100) NOT NULL,
        [Rol] int NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'SalarioSemanal', N'TipoEmpleado') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] ON;
    EXEC(N'INSERT INTO [Empleados] ([Id], [ApellidoPaterno], [Departamento], [Estado], [FechaCreacion], [NumeroSeguroSocial], [PrimerNombre], [SalarioSemanal], [TipoEmpleado])
    VALUES (1, N''Reyes'', N''Tecnología'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000001-1'', N''Ana'', 32500.0, N''Asalariado''),
    (2, N''Mejía'', N''Finanzas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000002-2'', N''Carlos'', 28000.0, N''Asalariado''),
    (3, N''Fernández'', N''Recursos Humanos'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000003-3'', N''Lucía'', 24500.0, N''Asalariado''),
    (4, N''Guzmán'', N''Operaciones'', 2, ''2026-01-15T12:00:00.0000000Z'', N''001-0000004-4'', N''Pedro'', 21000.0, N''Asalariado'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'SalarioSemanal', N'TipoEmpleado') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'HorasTrabajadas', N'NumeroSeguroSocial', N'PrimerNombre', N'SueldoPorHora', N'TipoEmpleado') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] ON;
    EXEC(N'INSERT INTO [Empleados] ([Id], [ApellidoPaterno], [Departamento], [Estado], [FechaCreacion], [HorasTrabajadas], [NumeroSeguroSocial], [PrimerNombre], [SueldoPorHora], [TipoEmpleado])
    VALUES (5, N''Santos'', N''Operaciones'', 1, ''2026-01-15T12:00:00.0000000Z'', 40.0, N''001-0000005-5'', N''María'', 350.0, N''PorHoras''),
    (6, N''Peña'', N''Operaciones'', 1, ''2026-01-15T12:00:00.0000000Z'', 46.5, N''001-0000006-6'', N''José'', 420.0, N''PorHoras''),
    (7, N''Jiménez'', N''Tecnología'', 1, ''2026-01-15T12:00:00.0000000Z'', 38.0, N''001-0000007-7'', N''Rosa'', 550.0, N''PorHoras''),
    (8, N''Castillo'', N''Operaciones'', 2, ''2026-01-15T12:00:00.0000000Z'', 40.0, N''001-0000008-8'', N''Miguel'', 310.0, N''PorHoras'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'HorasTrabajadas', N'NumeroSeguroSocial', N'PrimerNombre', N'SueldoPorHora', N'TipoEmpleado') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'TarifaComision', N'TipoEmpleado', N'VentasBrutas') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] ON;
    EXEC(N'INSERT INTO [Empleados] ([Id], [ApellidoPaterno], [Departamento], [Estado], [FechaCreacion], [NumeroSeguroSocial], [PrimerNombre], [TarifaComision], [TipoEmpleado], [VentasBrutas])
    VALUES (9, N''Vargas'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000009-9'', N''Elena'', 0.08, N''PorComision'', 185000.0),
    (10, N''Núñez'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000010-0'', N''Rafael'', 0.065, N''PorComision'', 240500.0),
    (11, N''Ortiz'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000011-1'', N''Carmen'', 0.1, N''PorComision'', 98750.5),
    (12, N''Polanco'', N''Ventas'', 2, ''2026-01-15T12:00:00.0000000Z'', N''001-0000012-2'', N''Andrés'', 0.075, N''PorComision'', 132000.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'TarifaComision', N'TipoEmpleado', N'VentasBrutas') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'SalarioBase', N'TarifaComision', N'TipoEmpleado', N'VentasBrutas') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] ON;
    EXEC(N'INSERT INTO [Empleados] ([Id], [ApellidoPaterno], [Departamento], [Estado], [FechaCreacion], [NumeroSeguroSocial], [PrimerNombre], [SalarioBase], [TarifaComision], [TipoEmpleado], [VentasBrutas])
    VALUES (13, N''Rosario'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000013-3'', N''Patricia'', 18000.0, 0.05, N''AsalariadoPorComision'', 210000.0),
    (14, N''Almonte'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000014-4'', N''Luis'', 20000.0, 0.045, N''AsalariadoPorComision'', 156300.0),
    (15, N''Batista'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000015-5'', N''Sofía'', 16500.0, 0.0625, N''AsalariadoPorComision'', 87400.25),
    (16, N''Encarnación'', N''Ventas'', 1, ''2026-01-15T12:00:00.0000000Z'', N''001-0000016-6'', N''Ramón'', 25000.0, 0.035, N''AsalariadoPorComision'', 305000.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApellidoPaterno', N'Departamento', N'Estado', N'FechaCreacion', N'NumeroSeguroSocial', N'PrimerNombre', N'SalarioBase', N'TarifaComision', N'TipoEmpleado', N'VentasBrutas') AND [object_id] = OBJECT_ID(N'[Empleados]'))
        SET IDENTITY_INSERT [Empleados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'HashContrasena', N'NombreUsuario', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [HashContrasena], [NombreUsuario], [Rol])
    VALUES (1, N''$2a$12$iA7cg2RZfZbgY6/shX4Py.bPfze.s9Pv.YDJ7sJHKa9ds/MBsXHVa'', N''admin'', 1),
    (2, N''$2a$12$RM7LaYWnevG9wPyWUyxQSe4C/3G3F0Ek7i.50I0zSurYpSIPlnWRa'', N''usuario'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'HashContrasena', N'NombreUsuario', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Empleados_ApellidoPaterno] ON [Empleados] ([ApellidoPaterno]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Empleados_Departamento] ON [Empleados] ([Departamento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Empleados_Estado] ON [Empleados] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Empleados_NumeroSeguroSocial] ON [Empleados] ([NumeroSeguroSocial]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260830214945_MigracionInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260830214945_MigracionInicial', N'8.0.30');
END;
GO

COMMIT;
GO

