/********************************************************************************************
    PROYECTO EDUCATIVO: SISTEMA DE LOGIN CON ROLES Y CONTRASEÑAS ENCRIPTADAS (SHA2_256)
    -----------------------------------------------------------------------------------
    OBJETIVO:

    Este script crea una base de datos llamada "LoginRolesDB_cif" que implementa un sistema
    de autenticación básico con tres roles principales: Administrador, Supervisor y Vendedor.
    Cada usuario tiene una contraseña encriptada mediante el algoritmo SHA2_256, garantizando
    que las credenciales no se almacenen en texto plano.

    ESTRUCTURA:

    - Tabla Roles: Contiene los tipos de roles disponibles en el sistema.
    - Tabla Usuarios: Almacena los datos de usuario, su contraseña en formato hash y el rol asignado.
    - Relación: Cada usuario está vinculado a un rol mediante una clave foránea (IdRol).

    SEGURIDAD:

    - Se utiliza la función HASHBYTES con el algoritmo SHA2_256 para generar un hash irreversible.
    - Las contraseñas no pueden ser desencriptadas, solo verificadas mediante comparación de hash.
    - Este método es adecuado para sistemas educativos, demostrativos o de pruebas técnicas.

    VALIDACIÓN DE LOGIN:

    - El sistema compara el usuario y el hash de la contraseña ingresada con los registros almacenados.
    - Si coinciden, se obtiene el rol correspondiente y se redirige al panel adecuado.

    AUTOR: Juancito Peña
    FECHA: 08/07/2026
********************************************************************************************/


-- CREAR BASE DE DATOS PRINCIPAL PARA EL SISTEMA DE LOGIN
CREATE DATABASE LoginRolesDB_cif;
GO

-- SELECCIONAR LA BASE DE DATOS PARA TRABAJAR
USE LoginRolesDB_cif;
GO

-- CREAR TABLA DE ROLES
-- Esta tabla define los tipos de roles disponibles en el sistema.
-- Cada rol tiene un identificador único (Id) y un nombre descriptivo (NombreRol).
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

-- CREAR TABLA DE USUARIOS
-- Esta tabla almacena los datos de los usuarios del sistema.
-- La columna Password guarda el hash SHA2_256 de la contraseña, no el texto original.
-- IdRol establece la relación con la tabla Roles mediante clave foránea.
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario VARCHAR(50) NOT NULL,
    Password VARCHAR(64) NOT NULL, -- HASH SHA2_256
    IdRol INT FOREIGN KEY REFERENCES Roles(Id)
);

-- INSERTAR ROLES PREDEFINIDOS
-- Se agregan tres roles básicos para el sistema: Admin, Supervisor y Vendedor.
INSERT INTO Roles (NombreRol) VALUES ('Admin'), ('Supervisor'), ('Vendedor');

select * from roles

-- INSERTAR USUARIOS CON CONTRASEÑAS ENCRIPTADAS
-- Se usa la función HASHBYTES con el algoritmo SHA2_256 para generar el hash.
-- CONVERT(VARCHAR(64), ..., 2) transforma el resultado binario en texto hexadecimal.
INSERT INTO Usuarios (Usuario, Password, IdRol) VALUES
('AdminUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 1),
('SuperUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'super123'), 2), 2),
('SalesUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'sales123'), 2), 3);
GO

-- CONSULTAR TODOS LOS USUARIOS REGISTRADOS
-- Muestra los datos almacenados en la tabla Usuarios.
SELECT * FROM Usuarios;

-- CONSULTAR TODOS LOS ROLES DISPONIBLES
-- Permite verificar los roles creados en la tabla Roles.
SELECT * FROM Roles;

-- CONSULTAR USUARIOS JUNTO A SUS ROLES
-- Realiza un INNER JOIN entre Usuarios y Roles para mostrar el nombre del rol asignado.
SELECT u.Usuario, u.Password, r.NombreRol
FROM Usuarios u
INNER JOIN Roles r ON u.IdRol = r.Id;
GO

-- VALIDAR LOGIN DE UN USUARIO (EJEMPLO: ADMINUSER)
-- Se declaran variables para simular el ingreso de credenciales.
DECLARE @Usuario VARCHAR(50) = 'AdminUser';
DECLARE @Password VARCHAR(50) = 'admin123';

-- Se compara el usuario y el hash de la contraseña ingresada con los datos almacenados.
-- Si coinciden, se devuelve el nombre del usuario y su rol correspondiente.
SELECT u.Usuario, r.NombreRol
FROM Usuarios u
INNER JOIN Roles r ON u.IdRol = r.Id
WHERE u.Usuario = @Usuario
AND u.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2);
GO


-- Ejecuta esto y mira el resultado en la columna "HashCalculado"
SELECT 
    Usuario, 
    Password AS PasswordGuardado,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2) AS HashCalculado
FROM Usuarios 
WHERE Usuario = 'AdminUser';


--SCRIPT PARA NUEVA VERSION CON SP:


-- CREAR USUARIO
CREATE PROCEDURE sp_InsertUsuario
    @Usuario VARCHAR(50),
    @Password VARCHAR(50),
    @IdRol INT
AS
BEGIN
    INSERT INTO Usuarios (Usuario, Password, IdRol)
    VALUES (@Usuario, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), @IdRol);
END
GO

-- LEER USUARIO POR NOMBRE
CREATE PROCEDURE sp_GetUsuario
    @Usuario VARCHAR(50)
AS
BEGIN
    SELECT U.Id, U.Usuario, R.NombreRol
    FROM Usuarios U
    INNER JOIN Roles R ON U.IdRol = R.Id
    WHERE U.Usuario = @Usuario;
END
GO

-- ACTUALIZAR USUARIO
CREATE PROCEDURE sp_UpdateUsuario
    @Id INT,
    @Usuario VARCHAR(50),
    @Password VARCHAR(50),
    @IdRol INT
AS
BEGIN
    UPDATE Usuarios
    SET Usuario = @Usuario,
        Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2),
        IdRol = @IdRol
    WHERE Id = @Id;
END
GO

-- ELIMINAR USUARIO
CREATE PROCEDURE sp_DeleteUsuario
    @Id INT
AS
BEGIN
    DELETE FROM Usuarios WHERE Id = @Id;
END
GO

