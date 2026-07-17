# 🔐 CRUD_LOGIN_MAUI — Sistema de Autenticación y Roles con .NET MAUI + SQL Server

> 📘 **Manual paso a paso, completo y documentado**
> Guía exhaustiva para construir (o entender) un sistema de **login con roles** desarrollado en **.NET MAUI**, con conexión directa a **SQL Server** y contraseñas protegidas mediante **hashing SHA2_256**.

---

## 📑 Tabla de Contenidos

| # | Paso | Descripción |
|---|------|-------------|
| 🗄️ | [Paso 1](#️-paso-1-configurar-la-base-de-datos-sql-server) | Configurar la Base de Datos (SQL Server) |
| 🚀 | [Paso 2](#-paso-2-enrutamiento-principal-appshell) | Enrutamiento Principal (`AppShell`) |
| 🔒 | [Paso 3](#-paso-3-pantalla-de-login-mainpage) | Pantalla de Login (`MainPage`) |
| 🛠️ | [Paso 4](#️-paso-4-panel-de-control-adminpage) | Panel de Control (`AdminPage`) |
| 🎭 | [Paso 5](#-paso-5-vistas-roles) | Vista de Gestión de Roles |
| 👔 | [Paso 6](#-paso-6-vistas-limitadas-supervisor) | Vista Restringida — Supervisor |
| 🛍️ | [Paso 7](#️-paso-7-vistas-limitadas-vendedor) | Vista Restringida — Vendedor |
| ✅ | [Cierre](#-conclusión) | Conclusión y buenas prácticas |

---

## 🧭 Visión General del Proyecto

Este proyecto es un **sistema educativo de autenticación con roles** (Admin, Supervisor, Vendedor) que demuestra un flujo completo:

- 🔑 **Login seguro** validado contra SQL Server con contraseñas encriptadas (nunca en texto plano).
- 🧩 **Navegación basada en Shell**, redirigiendo dinámicamente según el rol del usuario autenticado.
- 🗂️ **CRUD completo** de usuarios y roles desde la app (Insertar, Actualizar, Eliminar, Consultar, Validar).
- 🎨 **Vistas diferenciadas por rol**, donde solo el Administrador tiene permisos de escritura sobre la base de datos.

```
┌─────────────┐      valida credenciales       ┌───────────────────┐
│  MainPage   │ ─────────────────────────────► │   SQL Server DB    │
│  (Login)    │ ◄───────────────────────────── │ LoginRolesDB_cif   │
└──────┬──────┘        devuelve NombreRol       └───────────────────┘
       │
       │ Shell.Current.GoToAsync($"{role}Page")
       ▼
┌─────────────┬─────────────────┬─────────────────┐
│  AdminPage  │ SupervisorPage  │  VendedorPage    │
│ (CRUD total)│ (solo lectura)  │  (solo lectura)  │
└─────────────┴─────────────────┴─────────────────┘
```

### 📂 Estructura del Proyecto (.NET MAUI)

A diferencia del proyecto por defecto que genera Visual Studio, nuestra estructura está purificada y orientada exclusivamente a las pantallas de los roles y la conexión a datos.

```text
📁 CRUD_LOGIN_MAUI
 ├── 📁 Dependencies            # 📦 Paquetes (Microsoft.Data.SqlClient)
 ├── 📁 Platforms               # 🤖 Código específico por plataforma (Android, iOS, Windows)
 ├── 📁 Resources               # 🎨 Recursos visuales (AppIcon, Fonts, Images, Styles)
 │
 ├── 📄 App.xaml                # 🖌️ Diccionarios de recursos y estilos globales
 ├── 📄 App.xaml.cs             # 🚀 Punto de entrada (Llama al AppShell)
 ├── 📄 AppShell.xaml           # 🗺️ Enrutador visual (Gestor de las rutas base)
 ├── 📄 AppShell.xaml.cs        # 🔗 Registro de rutas ocultas mediante C#
 │
 ├── 📄 MainPage.xaml           # 🚪 Pantalla principal de Acceso (Login)
 ├── 📄 AdminPage.xaml          # 👑 Panel del Administrador (CRUD Usuarios)
 ├── 📄 RolesPage.xaml          # 🎭 Gestión del catálogo de roles (CRUD Roles)
 ├── 📄 SupervisorPage.xaml     # 👔 Vista limitada a reportes y supervisión
 ├── 📄 VendedorPage.xaml       # 🛍️ Vista operativa para módulo de ventas
 │
 ├── 📄 MauiProgram.cs          # ⚙️ Inyector de dependencias y fuentes de MAUI
 └── 📄 CRUD_LOGIN_MAUI.csproj  # 🏗️ Archivo maestro (net9.0 y reglas de nulos)
```

### 🧰 Requisitos previos

| Herramienta | Uso |
|---|---|
| 🖥️ Visual Studio 2022 (17.8+) con carga de trabajo **.NET Multi-platform App UI development** | Crear y ejecutar el proyecto MAUI |
| 🗃️ SQL Server (Express, Developer o Standard) | Alojar la base de datos `LoginRolesDB_cif` |
| 🧩 SQL Server Management Studio (SSMS) | Ejecutar scripts y administrar la base de datos |
| 📦 Paquete NuGet `Microsoft.Data.SqlClient` | Conectar la app MAUI con SQL Server |

---

## 🗄️ PASO 1: Configurar la Base de Datos (SQL Server)

Antes de programar en .NET MAUI, necesitamos la base de datos que almacenará a los usuarios, sus contraseñas encriptadas y sus roles. Además, como la app se conectará **desde un dispositivo/emulador** (no siempre desde la misma máquina), debemos habilitar el **acceso remoto** al motor de SQL Server.

### 🪜 Pasos para preparar todo el entorno de base de datos

**1️⃣ Instalar y abrir SSMS**
1. Descarga e instala **SQL Server Management Studio (SSMS)** desde el sitio oficial de Microsoft (si aún no lo tienes).
2. Ábrelo y en la ventana **Connect to Server**, ingresa el nombre de tu instancia (por ejemplo `localhost` o `NOMBRE-PC\SQLEXPRESS`).
3. Selecciona el método de autenticación:
   - 🪟 **Windows Authentication** (recomendado para desarrollo local), o
   - 🔑 **SQL Server Authentication** (necesario si luego usarás un usuario como `JUANCITO` desde la app).
4. Haz clic en **Connect**.

**2️⃣ Habilitar el modo de autenticación mixta (necesario para el usuario `JUANCITO` de la cadena de conexión)**
1. En el **Object Explorer**, clic derecho sobre el nombre del servidor → **Properties**.
2. Ve a la pestaña **Security**.
3. Selecciona **SQL Server and Windows Authentication mode**.
4. Clic en **OK**.
5. Clic derecho sobre el servidor → **Restart** (o reinicia el servicio desde *SQL Server Configuration Manager*) para aplicar el cambio.

**3️⃣ Crear el login SQL que usará la app (ej. `JUANCITO`)**
1. En **Object Explorer**, expande **Security → Logins**.
2. Clic derecho → **New Login...**
3. En **Login name**, escribe `JUANCITO`.
4. Selecciona **SQL Server authentication** y define la contraseña (por ejemplo `123456`, la misma que aparece en la cadena de conexión del código).
5. Desmarca **Enforce password expiration** (para evitar bloqueos en desarrollo).
6. En la pestaña **Server Roles**, marca `sysadmin` (solo para entorno de práctica) o, para algo más restringido, asigna permisos `db_owner` sobre `LoginRolesDB_cif` una vez creada.
7. Clic en **OK**.

**4️⃣ Crear una nueva consulta y ejecutar el script**
1. Clic en **New Query** (o `Ctrl+N`) en la barra de herramientas de SSMS.
2. Copia **todo** el script SQL de la sección de abajo.
3. Pégalo dentro de la ventana de consulta.
4. Ejecuta con el botón **Execute** o presionando `F5`.
5. Verifica en el panel de resultados que las consultas `SELECT * FROM Usuarios` y `SELECT * FROM Roles` devuelvan datos.

**5️⃣ Habilitar el Acceso Remoto (Remote Connections)**
1. Clic derecho sobre el servidor en Object Explorer → **Properties**.
2. Ve a la pestaña **Connections**.
3. Verifica que la casilla **Allow remote connections to this server** esté marcada. ✅
4. Clic en **OK**.

**6️⃣ Habilitar el protocolo TCP/IP y fijar el puerto 1433**
1. Abre **SQL Server Configuration Manager** (búscalo en el menú Inicio de Windows).
2. Ve a **SQL Server Network Configuration → Protocols for [TU_INSTANCIA]**.
3. Haz doble clic en **TCP/IP** y en la pestaña **Protocol**, cambia **Enabled** a `Yes`.
4. Ve a la pestaña **IP Addresses**, baja hasta la sección **IPAll**.
5. En **TCP Port**, escribe `1433` (déjalo vacío en *TCP Dynamic Ports*).
6. Clic en **Apply** y luego **OK**.
7. Reinicia el servicio: en el panel izquierdo ve a **SQL Server Services**, clic derecho sobre tu instancia (ej. `SQL Server (MSSQLSERVER)`) → **Restart**.

**7️⃣ Abrir el puerto 1433 en el Firewall de Windows**
1. Abre **Firewall de Windows Defender con seguridad avanzada**.
2. Clic en **Reglas de entrada (Inbound Rules)** → **Nueva regla...**
3. Selecciona **Puerto** → **Siguiente**.
4. Elige **TCP** y especifica el puerto **1433** → **Siguiente**.
5. Selecciona **Permitir la conexión** → **Siguiente**.
6. Marca los perfiles (Dominio, Privado, Público según tu red) → **Siguiente**.
7. Asigna un nombre descriptivo, por ejemplo `SQL Server 1433` → **Finalizar**.

**8️⃣ Verificar la IP del servidor para la cadena de conexión**
1. En una terminal de Windows (`cmd`), ejecuta `ipconfig` y copia la **IPv4 Address** del equipo donde corre SQL Server (en este manual se usa `10.0.0.15` como ejemplo).
2. Asegúrate de que el emulador/dispositivo MAUI esté en la **misma red** para poder alcanzar esa IP.
3. Esa IP es la que se usará en `Server=10.0.0.15,1433;...` dentro de las cadenas de conexión de C#.

> ⚠️ **Nota de seguridad:** `SHA2_256` es un algoritmo de *hashing* unidireccional (no reversible), ideal para fines educativos. En producción se recomienda además aplicar **salting** (sal criptográfica) para mitigar ataques de tablas *rainbow*.

### 📜 Script completo de creación de la base de datos

```sql
/********************************************************************************************
    PROYECTO EDUCATIVO: SISTEMA DE LOGIN CON ROLES Y CONTRASEÑAS ENCRIPTADAS (SHA2_256)
    -----------------------------------------------------------------------------------
    OBJETIVO:
    Crear la base "LoginRolesDB_cif" que implementa autenticación básica con roles 
    (Admin, Supervisor, Vendedor) y contraseñas encriptadas (SHA2_256).
********************************************************************************************/

-- 1. CREAR BASE DE DATOS PRINCIPAL
CREATE DATABASE LoginRolesDB_cif;
GO
USE LoginRolesDB_cif;
GO

-- 2. CREAR TABLA DE ROLES
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
```

### 📊 Modelo Entidad-Relación (simplificado)

```
┌─────────────────────┐          ┌─────────────────────────┐
│        Roles        │          │        Usuarios         │
├─────────────────────┤          ├─────────────────────────┤
│ 🔑 Id        INT     │ 1     N │ 🔑 Id            INT     │
│    NombreRol VARCHAR │◄─────────│    Usuario       VARCHAR│
└─────────────────────┘          │    Password      VARCHAR│ (hash SHA2_256)
                                  │ 🔗 IdRol         INT     │ (FK → Roles.Id)
                                  └─────────────────────────┘
```

| 👤 Usuario | 🔑 Contraseña (texto plano, solo demo) | 🎭 Rol |
|---|---|---|
| AdminUser | admin123 | Admin |
| SuperUser | super123 | Supervisor |
| SalesUser | sales123 | Vendedor |

**9️⃣ Probar la conexión remota desde otra máquina (opcional pero recomendado)**
1. Desde otra PC en la misma red, abre SSMS.
2. En **Connect to Server**, escribe `10.0.0.15,1433` (IP,puerto) como nombre de servidor.
3. Autentícate con el login `JUANCITO` y su contraseña.
4. Si conecta correctamente, tu configuración de red/firewall/TCP-IP quedó lista para que la app MAUI también se conecte. ✅

---

## 🚀 PASO 2: Enrutamiento Principal (`AppShell`)

Una vez que tienes el proyecto MAUI creado, el primer paso es configurar el **AppShell**. Este archivo actúa como el 🎼 "director de orquesta", definiendo las rutas (URLs internas) para navegar entre pantallas de manera segura.

> ℹ️ `AppShell.xaml` y `AppShell.xaml.cs` **ya vienen incluidos automáticamente** al crear un proyecto nuevo de .NET MAUI (plantilla "App"), por lo que aquí no se crean desde cero, sino que se **editan**.

### 🪜 Pasos para crear el proyecto y ubicar `AppShell.xaml`

1. Abre **Visual Studio 2022**.
2. Clic en **Create a new project**.
3. Busca la plantilla **.NET MAUI App** y selecciónala → **Next**.
4. Nombra el proyecto exactamente `CRUD_LOGIN_MAUI` (para que coincida con el `namespace` usado en todo el código) → elige la ubicación → **Next**.
5. Selecciona el **Framework** (.NET 8.0 o superior) → **Create**.
6. Espera a que Visual Studio genere la plantilla base.
7. En el **Solution Explorer**, ubica los archivos base: `App.xaml` (y su `App.xaml.cs`) y `AppShell.xaml` (y su `AppShell.xaml.cs`).
8. Configúralos usando los siguientes bloques de código.

### 🛠️ Inicialización Base (`App.xaml` y `App.xaml.cs`)

Estos archivos inician la aplicación y cargan el `AppShell`. 

**`App.xaml`** (Por defecto, carga los estilos y colores globales):
```xml
<?xml version = "1.0" encoding = "UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
             x:Class="CRUD_LOGIN_MAUI.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**`App.xaml.cs`** (Lógica de inicialización):
> ⚠️ **Importante:** Al tener desactivadas las advertencias de nulos globales, removemos el signo `?` de `IActivationState` que viene por defecto.

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace CRUD_LOGIN_MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		return new Window(new AppShell());
	}
}
```

### 📄 ¿Cómo editar el archivo `AppShell.xaml`?

En tu proyecto, abre el archivo `AppShell.xaml` y reemplaza su contenido por este. Hemos desactivado el menú lateral (`FlyoutBehavior="Disabled"`) para forzar que la navegación ocurra **solo** a través de botones y lógica de código — evitando que el usuario acceda manualmente a pantallas de otros roles.

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="CRUD_LOGIN_MAUI.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
    Title="CRUD_LOGIN_MAUI"
    FlyoutBehavior="Disabled"> <!-- Deshabilita el menú deslizable -->

    <!-- Página de Inicio por Defecto (Login) -->
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate local:MainPage}"
        Route="MainPage" />

    <!-- Rutas ocultas en el menú, solo accesibles por código C# -->
    <ShellContent Route="AdminPage" ContentTemplate="{DataTemplate local:AdminPage}" FlyoutItem.IsVisible="False"/>
    <ShellContent Route="SupervisorPage" ContentTemplate="{DataTemplate local:SupervisorPage}" FlyoutItem.IsVisible="False"/>
    <ShellContent Route="VendedorPage" ContentTemplate="{DataTemplate local:VendedorPage}" FlyoutItem.IsVisible="False"/>
</Shell>
```

### ⚙️ ¿Cómo editar el archivo `AppShell.xaml.cs`?

Aquí registramos adicionalmente rutas complejas como la pantalla de Roles, que no está declarada como `ShellContent` en el XAML, sino registrada manualmente para navegación por *push* (`Navigation.PushAsync`).

```csharp
namespace CRUD_LOGIN_MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Registrar rutas para poder navegar a ellas usando Shell.Current.GoToAsync()
        Routing.RegisterRoute("AdminPage", typeof(AdminPage));
        Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));
        Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));
        Routing.RegisterRoute("RolesPage", typeof(RolesPage));
    }
}
```

> 💡 **Tip:** El nombre del rol devuelto por la consulta SQL (`Admin`, `Supervisor`, `Vendedor`) se concatena directamente con `"Page"` en `MainPage.xaml.cs` para construir la ruta de destino. Por eso es fundamental que `NombreRol` en la base de datos coincida **exactamente** con el prefijo de cada página (`AdminPage`, `SupervisorPage`, `VendedorPage`).
>
> ⚠️ Este paso solo compilará sin errores una vez que existan físicamente las clases `AdminPage`, `SupervisorPage`, `VendedorPage`, `MainPage` y `RolesPage` en el proyecto — créalas siguiendo los pasos siguientes antes de compilar.

---

## 🔒 PASO 3: Pantalla de Login (`MainPage`)

Esta es la 🚪 puerta de entrada. En esta pantalla, el usuario coloca sus credenciales para ser autenticado y redirigido según su rol.

> ℹ️ `MainPage.xaml` **también viene incluido por defecto** en la plantilla de .NET MAUI, así que aquí se **edita** en vez de crearse desde cero.

### 🪜 Pasos para ubicar y preparar `MainPage`

1. En el **Solution Explorer**, ubica el archivo `MainPage.xaml` (raíz del proyecto, junto a `AppShell.xaml`).
2. Haz doble clic para abrirlo en el diseñador/editor XAML.
3. Borra todo el contenido de ejemplo (el famoso contador "Click me").
4. Pega el XAML del bloque de abajo.
5. Expande `MainPage.xaml` en el árbol y abre `MainPage.xaml.cs`.
6. Borra el código de ejemplo y pega el C# correspondiente.
7. Antes de compilar, agrega el paquete NuGet necesario para SQL Server (ver recuadro siguiente).

> 📦 **Instalar el conector de SQL Server (una sola vez por proyecto):**
> 1. Clic derecho sobre el proyecto `CRUD_LOGIN_MAUI` en el Solution Explorer → **Manage NuGet Packages...**
> 2. Pestaña **Browse**, busca `Microsoft.Data.SqlClient`.
> 3. Selecciónalo e instala la versión estable más reciente compatible con tu `TargetFramework`.
> 4. Clic en **Install** y acepta los términos de licencia.

### 🎨 ¿Cómo editar el archivo `MainPage.xaml` (La Vista)?

Abre `MainPage.xaml` y diseña la interfaz gráfica con campos para Usuario y Contraseña, además de un botón con un ícono de 👁️ "ojito" para mostrar/ocultar la contraseña temporalmente.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.MainPage">

    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#F5F5F5">

        <Image Source="https://images.icon-icons.com/2120/PNG/512/lock_padlock_locked_protected_security_icon_131240.png"
               HeightRequest="175" HorizontalOptions="Center" />

        <Entry x:Name="txtUsuario"
               Placeholder="Usuario"
               ClearButtonVisibility="WhileEditing"
               HorizontalTextAlignment="Center"
               TextColor="Black"
               FontSize="25"
               Margin="10"/>

        <!-- Grid para Contraseña + Botón Ojito -->
        <Grid Margin="10">
            <Entry x:Name="txtPassword"
                   Placeholder="Contraseña"
                   IsPassword="True"
                   HorizontalTextAlignment="Center"
                   TextColor="Black"
                   FontSize="25"/>

            <Button x:Name="btnTogglePassword"
                    Text="👁️"
                    Clicked="OnTogglePasswordClicked"
                    BackgroundColor="Transparent"
                    HorizontalOptions="End"
                    WidthRequest="60"/>
        </Grid>

        <Button Text="Ingresar"
                BackgroundColor="#fbc531"
                TextColor="#40739e"
                FontAttributes="Bold"
                Margin="20"
                Clicked="OnLogin_Clicked"
                FontSize="25" />

        <Label x:Name="lblMensaje"
               TextColor="Red"
               FontSize="22"
               FontAttributes="Bold"
               HorizontalTextAlignment="Center" />

    </VerticalStackLayout>
</ContentPage>


```

### 🧠 ¿Cómo editar el archivo `MainPage.xaml.cs` (La Lógica)?

Este código es vital ⚡. Aquí se toma la contraseña ingresada, se envía a la base de datos para compararla (encriptándola en el proceso con `SHA2_256`) y si coinciden, se obtiene el nombre del rol para redirigir dinámicamente (`AdminPage`, `VendedorPage`, etc).

```csharp
using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        txtUsuario.Text = string.Empty;
        txtPassword.Text = string.Empty;
        lblMensaje.Text = string.Empty;
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        btnTogglePassword.Text = txtPassword.IsPassword ? "👁️" : "🙈";
    }

    private async void OnLogin_Clicked(object sender, EventArgs e)
    {
        string usuario = txtUsuario.Text?.Trim();
        string password = txtPassword.Text?.Trim();

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            lblMensaje.Text = "❌ Por favor, ingrese sus credenciales.";
            return;
        }

        try
        {
            string connectionString = "Server=10.0.0.15,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT R.NombreRol 
                                 FROM Usuarios U 
                                 INNER JOIN Roles R ON U.IdRol = R.Id 
                                 WHERE U.Usuario = @Usuario 
                                 AND U.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                    command.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;

                    var roleResult = await command.ExecuteScalarAsync();

                    if (roleResult != null)
                    {
                        string role = roleResult.ToString();
                        await Shell.Current.GoToAsync($"{role}Page");
                    }
                    else
                    {
                        lblMensaje.Text = "❌ Usuario o contraseña incorrectos.";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lblMensaje.Text = $"❌ Error: {ex.Message}";
        }
    }
}


```

> 🛡️ **Buena práctica aplicada:** la consulta usa **parámetros SQL** (`@Usuario`, `@Password`) en lugar de concatenar strings, lo que previene ataques de **inyección SQL (SQL Injection)**.
>
> ⚠️ **Nota sobre la cadena de conexión:** en este manual la `connectionString` está escrita directamente en el código (*hardcoded*) por fines didácticos. En un proyecto real conviene moverla a un archivo de configuración seguro (por ejemplo, `appsettings.json` + variables de entorno, o `SecureStorage` de MAUI) para no exponer credenciales. Recuerda reemplazar `10.0.0.15` por la IP real de tu servidor SQL (ver Paso 1, punto 8️⃣).

---

## 🛠️ PASO 4: Panel de Control (`AdminPage`)

Pantalla exclusiva para que el 👑 Administrador registre, edite, elimine y consulte usuarios del sistema — el único rol con permisos de escritura sobre la base de datos.

### 🪜 Pasos para crear `AdminPage.xaml` y `AdminPage.xaml.cs`

1. En el **Solution Explorer**, clic derecho sobre el nombre del proyecto `CRUD_LOGIN_MAUI` → **Add** → **New Item...**
2. En el buscador de plantillas, escribe `ContentPage` y selecciona **.NET MAUI ContentPage (XAML)**.
3. En el campo **Name**, escribe exactamente `AdminPage.xaml`.
4. Clic en **Add**. Visual Studio generará automáticamente `AdminPage.xaml` y su code-behind `AdminPage.xaml.cs`.
5. Abre `AdminPage.xaml`, borra el contenido por defecto y pega el XAML de abajo.
6. Abre `AdminPage.xaml.cs`, borra el contenido por defecto y pega el C# correspondiente.
7. Verifica que el `x:Class` en el XAML (`CRUD_LOGIN_MAUI.AdminPage`) coincida con el `namespace` + nombre de clase del `.cs`.

### 🎨 Vista (`AdminPage.xaml`)

Se usa para la gestión completa (CRUD) de los usuarios, e incluye un buscador en tiempo real y una lista (`CollectionView`) con *compiled bindings*.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
             x:Class="CRUD_LOGIN_MAUI.AdminPage"
             Title="Administrador">

    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#E3F2FD">

            <!-- Título -->
            <Label Text="Panel de Control - Administrador"
                   FontSize="22"
                   FontAttributes="Bold"
                   TextColor="#0D47A1"
                   HorizontalOptions="Center"/>

            <!-- Entradas de usuario -->
            <Entry x:Name="txtUsuario" Placeholder="👤 Usuario"/>
            <Entry x:Name="txtPassword" Placeholder="🔑 Contraseña" IsPassword="True"/>

            <!-- Picker de roles -->
            <Picker x:Name="pickerRol" Title="Seleccionar Rol">
                <Picker.ItemsSource>
                    <x:Array Type="{x:Type x:String}">
                        <x:String>Admin</x:String>
                        <x:String>Supervisor</x:String>
                        <x:String>Vendedor</x:String>
                    </x:Array>
                </Picker.ItemsSource>
            </Picker>

            <!-- Buscador -->
            <Entry x:Name="txtBuscar"
                   Placeholder="🔎 Buscar por ID, Usuario o Rol..."
                   TextChanged="OnSearchChanged"
                   BackgroundColor="White"/>

            <!-- Botones CRUD -->
            <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto" ColumnSpacing="10" RowSpacing="10">
                <Button Grid.Row="0" Grid.Column="0" Text="➕ Insertar" BackgroundColor="Green" TextColor="White" Clicked="OnInsertClicked"/>
                <Button Grid.Row="0" Grid.Column="1" Text="🔄 Actualizar" BackgroundColor="DodgerBlue" TextColor="White" Clicked="OnUpdateClicked"/>
                <Button Grid.Row="0" Grid.Column="2" Text="🗑️ Eliminar" BackgroundColor="Red" TextColor="White" Clicked="OnDeleteClicked"/>
                <Button Grid.Row="1" Grid.Column="0" Text="🔍 Consultar" BackgroundColor="Orange" TextColor="White" Clicked="OnConsultClicked"/>
                <Button Grid.Row="1" Grid.Column="1" Text="✅ Validar" BackgroundColor="Purple" TextColor="White" Clicked="OnValidateClicked"/>
                <Button Grid.Row="1" Grid.Column="2" Text="🧹 Limpiar" BackgroundColor="Gray" TextColor="White" Clicked="OnClearClicked"/>
            </Grid>

            <!-- Mensajes -->
            <Label x:Name="lblMensaje" TextColor="Red" FontAttributes="Bold" HorizontalOptions="Center"/>

            <!-- Lista de usuarios con compiled bindings -->
            <CollectionView x:Name="listaUsuarios"
                            SelectionMode="Single"
                            SelectionChanged="OnUsuarioSelected"
                            HeightRequest="250">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="local:UsuarioItem">
                        <Grid Padding="10" ColumnDefinitions="40, *, *">
                            <Label Grid.Column="0" Text="{Binding Id}" FontAttributes="Bold" TextColor="Blue"/>
                            <Label Grid.Column="1" Text="{Binding Usuario}" FontAttributes="Bold"/>
                            <Label Grid.Column="2" Text="{Binding Rol}" TextColor="Gray"/>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <Button Text="⚙️ Roles"
                    BackgroundColor="Teal"
                    TextColor="White"
                    Clicked="OnRolesClicked"/>


            <!-- Botón logout -->
            <Button Text="Cerrar sesión"
                    BackgroundColor="DarkRed"
                    TextColor="White"
                    Margin="0,20"
                    Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>



```

### 🧩 Lógica de Ejecución Centralizada (`AdminPage.xaml.cs`)

Aquí usamos un método mágico ✨ central `EjecutarAccion` que nos ahorra repetir código: recibe la consulta SQL y el nombre de la acción (para el mensaje de confirmación), y encapsula la apertura de conexión, ejecución y recarga de la lista.

```csharp
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI;

public class UsuarioItem
{
    public int Id { get; set; }
    public string Usuario { get; set; }
    public string Rol { get; set; }
}

public partial class AdminPage : ContentPage
{
    private string connectionString = "Server=10.0.0.15,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
    private bool isProcessing = false;
    private int idSeleccionado = 0;
    private List<int> _rolesIds = new List<int>();

    public AdminPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarRoles();
    }

    private async Task CargarRoles()
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string query = "SELECT Id, NombreRol FROM Roles";
            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            _rolesIds.Clear();
            var roles = new List<string>();
            while (await reader.ReadAsync())
            {
                _rolesIds.Add((int)reader["Id"]);
                roles.Add(reader["NombreRol"].ToString());
            }

            pickerRol.ItemsSource = roles;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los roles: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnInsertClicked(object sender, EventArgs e) =>
        await EjecutarAccion(@"INSERT INTO Usuarios (Usuario, Password, IdRol) 
                               VALUES (@Usuario, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), @IdRol)", "insertar");

    private async void OnUpdateClicked(object sender, EventArgs e) =>
        await EjecutarAccion(@"UPDATE Usuarios 
                               SET Usuario=@Usuario, Password=CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), IdRol=@IdRol 
                               WHERE Id=@Id", "actualizar");

    private async void OnDeleteClicked(object sender, EventArgs e) =>
        await EjecutarAccion("DELETE FROM Usuarios WHERE Id=@Id", "eliminar");

    private async Task EjecutarAccion(string query, string accion)
    {
        if (isProcessing) return;

        bool confirmar = await DisplayAlert("Confirmación", $"¿Seguro que desea {accion} este usuario?", "Sí", "No");
        if (!confirmar) return;

        if (pickerRol.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Debe seleccionar un rol.", "OK");
            return;
        }

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text ?? "");
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text ?? "");

            cmd.Parameters.AddWithValue("@IdRol", _rolesIds[pickerRol.SelectedIndex]);
            cmd.Parameters.AddWithValue("@Id", idSeleccionado);

            await cmd.ExecuteNonQueryAsync();
            await DisplayAlert("Éxito", $"Usuario {accion}do correctamente.", "OK");

            LimpiarCampos();
            await CargarLista("SELECT U.Id, U.Usuario, R.NombreRol FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id", "");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { isProcessing = false; }
    }

    private async void OnConsultClicked(object sender, EventArgs e) =>
        await CargarLista(@"SELECT U.Id, U.Usuario, R.NombreRol 
                            FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id", "");

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string filtro = "%" + e.NewTextValue + "%";
        string query = @"SELECT U.Id, U.Usuario, R.NombreRol 
                         FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id 
                         WHERE U.Usuario LIKE @Filtro OR CAST(U.Id AS VARCHAR) LIKE @Filtro";
        await CargarLista(query, filtro);
    }

    private async Task CargarLista(string query, string parametro)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            if (!string.IsNullOrEmpty(parametro)) cmd.Parameters.AddWithValue("@Filtro", parametro);

            using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<UsuarioItem>();

            while (await reader.ReadAsync())
                lista.Add(new UsuarioItem
                {
                    Id = (int)reader["Id"],
                    Usuario = reader["Usuario"].ToString(),
                    Rol = reader["NombreRol"].ToString()
                });

            listaUsuarios.ItemsSource = lista;
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnUsuarioSelected(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as UsuarioItem;
        if (item != null)
        {
            idSeleccionado = item.Id;
            txtUsuario.Text = item.Usuario;
            txtPassword.Text = "";

            if (pickerRol.ItemsSource is List<string> roles)
            {
                pickerRol.SelectedIndex = roles.IndexOf(item.Rol);
            }
        }
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        string query = @"SELECT U.Usuario 
                         FROM Usuarios U 
                         WHERE U.Usuario=@U AND U.Password=CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @P), 2)";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@U", txtUsuario.Text);
        cmd.Parameters.AddWithValue("@P", txtPassword.Text);

        lblMensaje.Text = (await cmd.ExecuteScalarAsync() != null) ? "✅ Credenciales correctas" : "❌ Incorrecto";
    }

    private void OnClearClicked(object sender, EventArgs e) => LimpiarCampos();

    private async void OnRolesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RolesPage());
    }

    private void LimpiarCampos()
    {
        idSeleccionado = 0;
        txtUsuario.Text = txtPassword.Text = txtBuscar.Text = "";
        pickerRol.SelectedIndex = -1;
        lblMensaje.Text = "";
        listaUsuarios.ItemsSource = null;
    }
}



```

> 📝 **Detalle importante:** al actualizar un usuario, el campo de contraseña **siempre** se vuelve a encriptar con `HASHBYTES`, incluso si el usuario deja el campo vacío en pantalla — por eso, en un escenario real, conviene validar que `txtPassword` no esté vacío antes de sobrescribir el hash existente, o el usuario perdería su contraseña anterior.
>
> 🧭 **Navegación mixta:** nota que esta pantalla usa dos mecanismos distintos: `Shell.Current.GoToAsync` (rutas registradas en el Shell) para el logout, y `Navigation.PushAsync` (pila de navegación clásica) para ir a `RolesPage`.

---

## 🎭 PASO 5: Vistas (ROLES)

A diferencia del Administrador, estas vistas carecen de botones para modificar la base de datos... *excepto* `RolesPage`, que sí permite gestionar el catálogo de roles (es una extensión administrativa, accesible únicamente desde `AdminPage`).

### 🪜 Pasos para crear `RolesPage.xaml` y `RolesPage.xaml.cs`

1. Clic derecho sobre el proyecto `CRUD_LOGIN_MAUI` en el Solution Explorer → **Add** → **New Item...**
2. Busca la plantilla **.NET MAUI ContentPage (XAML)**.
3. En **Name**, escribe `RolesPage.xaml` → **Add**.
4. Visual Studio crea `RolesPage.xaml` junto con `RolesPage.xaml.cs`.
5. Abre `RolesPage.xaml`, borra el contenido por defecto y pega el XAML de abajo.
6. Abre `RolesPage.xaml.cs`, borra el contenido por defecto y pega el C# correspondiente.
7. Recuerda que esta página se abre mediante `Navigation.PushAsync(new RolesPage())` desde `AdminPage`, no mediante una ruta del Shell — no necesita `ShellContent` en `AppShell.xaml`, solo el `Routing.RegisterRoute` ya agregado en el Paso 2.

### 📄 `RolesPage.xaml` (Gestión del Catálogo de Roles)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
             x:Class="CRUD_LOGIN_MAUI.RolesPage"
             Title="Gestión de Roles">

    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#FFFDE7">

            <Label Text="Panel de Control - Roles"
                   FontSize="22"
                   FontAttributes="Bold"
                   TextColor="#BF360C"
                   HorizontalOptions="Center"/>

            <!-- Campo para nombre del rol -->
            <Entry x:Name="txtRol" Placeholder="🛡️ Nombre del Rol"/>

            <!-- Buscador -->
            <Entry x:Name="txtBuscar"
                   Placeholder="🔎 Buscar por ID o Nombre..."
                   TextChanged="OnSearchChanged"
                   BackgroundColor="White"/>

            <!-- Botones CRUD -->
            <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto" ColumnSpacing="10" RowSpacing="10">
                <Button Grid.Row="0" Grid.Column="0" Text="➕ Insertar" BackgroundColor="Green" TextColor="White" Clicked="OnInsertClicked"/>
                <Button Grid.Row="0" Grid.Column="1" Text="🔄 Actualizar" BackgroundColor="DodgerBlue" TextColor="White" Clicked="OnUpdateClicked"/>
                <Button Grid.Row="0" Grid.Column="2" Text="🗑️ Eliminar" BackgroundColor="Red" TextColor="White" Clicked="OnDeleteClicked"/>
                <Button Grid.Row="1" Grid.Column="0" Text="🔍 Consultar" BackgroundColor="Orange" TextColor="White" Clicked="OnConsultClicked"/>
                <Button Grid.Row="1" Grid.Column="2" Text="🧹 Limpiar" BackgroundColor="Gray" TextColor="White" Clicked="OnClearClicked"/>
            </Grid>

            <!-- Lista de roles -->
            <CollectionView x:Name="listaRoles"
                            SelectionMode="Single"
                            SelectionChanged="OnRolSelected"
                            HeightRequest="250">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="local:RolItem">
                        <Grid Padding="10" ColumnDefinitions="40, *">
                            <Label Grid.Column="0" Text="{Binding Id}" FontAttributes="Bold" TextColor="Blue"/>
                            <Label Grid.Column="1" Text="{Binding NombreRol}" FontAttributes="Bold"/>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <!-- Botón logout -->
            <Button Text="Cerrar sesión"
                    BackgroundColor="DarkRed"
                    TextColor="White"
                    Margin="0,20"
                    Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>

```

### ⚙️ `RolesPage.xaml.cs` (Lógica del Catálogo de Roles)

```csharp
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI;

public class RolItem
{
    public int Id { get; set; }
    public string NombreRol { get; set; }
}

public partial class RolesPage : ContentPage
{
    private string connectionString = "Server=10.0.0.15,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
    private bool isProcessing = false;
    private int idSeleccionado = 0;

    public RolesPage() => InitializeComponent();

    private async void OnLogoutClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnInsertClicked(object sender, EventArgs e) =>
        await EjecutarAccion("INSERT INTO Roles (NombreRol) VALUES (@NombreRol)", "insertar");

    private async void OnUpdateClicked(object sender, EventArgs e) =>
        await EjecutarAccion("UPDATE Roles SET NombreRol=@NombreRol WHERE Id=@Id", "actualizar");

    private async void OnDeleteClicked(object sender, EventArgs e) =>
        await EjecutarAccion("DELETE FROM Roles WHERE Id=@Id", "eliminar");

    private async Task EjecutarAccion(string query, string accion)
    {
        if (isProcessing) return;

        bool confirmar = await DisplayAlert("Confirmación", $"¿Seguro que desea {accion} este rol?", "Sí", "No");
        if (!confirmar) return;

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@NombreRol", txtRol.Text ?? "");
            cmd.Parameters.AddWithValue("@Id", idSeleccionado);

            await cmd.ExecuteNonQueryAsync();
            await DisplayAlert("Éxito", $"Rol {accion}do correctamente.", "OK");

            LimpiarCampos();
            await CargarLista("SELECT Id, NombreRol FROM Roles", "");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { isProcessing = false; }
    }

    private async void OnConsultClicked(object sender, EventArgs e) =>
        await CargarLista("SELECT Id, NombreRol FROM Roles", "");

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string filtro = "%" + e.NewTextValue + "%";
        string query = "SELECT Id, NombreRol FROM Roles WHERE NombreRol LIKE @Filtro OR CAST(Id AS VARCHAR) LIKE @Filtro";
        await CargarLista(query, filtro);
    }

    private async Task CargarLista(string query, string parametro)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            if (!string.IsNullOrEmpty(parametro)) cmd.Parameters.AddWithValue("@Filtro", parametro);

            using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<RolItem>();

            while (await reader.ReadAsync())
                lista.Add(new RolItem
                {
                    Id = (int)reader["Id"],
                    NombreRol = reader["NombreRol"].ToString()
                });

            listaRoles.ItemsSource = lista;
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnRolSelected(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as RolItem;
        if (item != null)
        {
            idSeleccionado = item.Id;
            txtRol.Text = item.NombreRol;
        }
    }

    private void OnClearClicked(object sender, EventArgs e) => LimpiarCampos();

    private void LimpiarCampos()
    {
        idSeleccionado = 0;
        txtRol.Text = txtBuscar.Text = "";
        listaRoles.ItemsSource = null;
    }
}

```

> ⚠️ **Cuidado con la integridad referencial:** al eliminar un rol que todavía tiene usuarios asociados (`Usuarios.IdRol`), SQL Server rechazará la operación por la restricción `FOREIGN KEY`. Esto es intencional: protege la base de datos de quedar con usuarios "huérfanos" sin rol válido.

---

## 👔 PASO 6: Vistas Limitadas (Supervisor)

A diferencia del Administrador, estas vistas carecen de botones para modificar la base de datos, cumpliendo su propósito de **perfiles de solo lectura / operación restringida**.

### 🪜 Pasos para crear `SupervisorPage.xaml` y `SupervisorPage.xaml.cs`

1. Clic derecho sobre el proyecto `CRUD_LOGIN_MAUI` → **Add** → **New Item...**
2. Selecciona la plantilla **.NET MAUI ContentPage (XAML)**.
3. En **Name**, escribe `SupervisorPage.xaml` → **Add**.
4. Se generan `SupervisorPage.xaml` y `SupervisorPage.xaml.cs`.
5. Abre `SupervisorPage.xaml`, borra el contenido de ejemplo y pega el XAML de abajo.
6. Abre `SupervisorPage.xaml.cs`, borra el contenido de ejemplo y pega el C# correspondiente.
7. Verifica que ya exista la ruta `Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));` en `AppShell.xaml.cs` (Paso 2) y el `ShellContent` correspondiente para que `Shell.Current.GoToAsync("SupervisorPage")` funcione tras el login.

### 📄 `SupervisorPage.xaml` (Pantalla Restringida)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.SupervisorPage"
             Title="SupervisorPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#FFF3E0">
        <Label Text="Bienvenido Supervisor"
               FontSize="24"
               FontAttributes="Bold"
               TextColor="#E65100"
               HorizontalOptions="Center" 
               Margin="10"/>

        <!-- Logo -->
        <Image Source="https://cdn-icons-png.flaticon.com/256/3461/3461567.png"
                 HeightRequest="175"
                 HorizontalOptions="Center" />

        <Label Text="Ventana: Reportes y supervision"
               FontSize="18"
               TextColor="Black"
               HorizontalOptions="Center" 
                Margin="10" />

        <Button Text="Cerrar Sesion" 
                Clicked="OnLogoutClicked" 
                BackgroundColor="#D32F2F" 
                TextColor="White" 
                HorizontalOptions="Center" 
                Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>

```

### ⚙️ `SupervisorPage.xaml.cs` (Lógica Restringida)

```csharp
namespace CRUD_LOGIN_MAUI;

public partial class SupervisorPage : ContentPage
{
    public SupervisorPage()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}

```

> 💡 **Idea de extensión:** este espacio ("Ventana: Reportes y supervisión") es el punto ideal para incorporar, por ejemplo, gráficos de ventas por vendedor, alertas de inventario o reportes en PDF — sin otorgar permisos de edición sobre `Usuarios` o `Roles`.

---

## 🛍️ PASO 7: Vistas Limitadas (Vendedor)

A diferencia del Administrador, estas vistas carecen de botones para modificar la base de datos, cumpliendo su propósito de **perfil operativo enfocado en ventas**.

### 🪜 Pasos para crear `VendedorPage.xaml` y `VendedorPage.xaml.cs`

1. Clic derecho sobre el proyecto `CRUD_LOGIN_MAUI` → **Add** → **New Item...**
2. Selecciona la plantilla **.NET MAUI ContentPage (XAML)**.
3. En **Name**, escribe `VendedorPage.xaml` → **Add**.
4. Se generan `VendedorPage.xaml` y `VendedorPage.xaml.cs`.
5. Abre `VendedorPage.xaml`, borra el contenido de ejemplo y pega el XAML de abajo.
6. Abre `VendedorPage.xaml.cs`, borra el contenido de ejemplo y pega el C# correspondiente.
7. Confirma que `Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));` y su `ShellContent` (Paso 2) ya estén presentes para permitir la redirección `Shell.Current.GoToAsync("VendedorPage")` desde el login.
8. Con esto, las 5 páginas (`MainPage`, `AdminPage`, `RolesPage`, `SupervisorPage`, `VendedorPage`) y el `AppShell` quedan completos — compila el proyecto (`Ctrl+Shift+B`) para verificar que no haya errores antes de ejecutar.

### 📄 `VendedorPage.xaml` (Pantalla Restringida)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.VendedorPage"
             Title="VendedorPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#E8F5E9">
        <Label Text="Bienvenido Vendedor"
               FontSize="24"
               FontAttributes="Bold"
               TextColor="#1B5E20"
               HorizontalOptions="Center"
               Margin="10"/>

        <!-- Logo -->
        <Image Source="https://cdn-icons-png.flaticon.com/512/2316/2316167.png"
                 HeightRequest="175"
                 HorizontalOptions="Center" />

        <Label Text="Ventana: Modulo de ventas"
               FontSize="18"
               TextColor="Black"
               HorizontalOptions="Center"
                Margin="10"/>

        <Button Text="Cerrar Sesion" 
                Clicked="OnLogoutClicked" 
                BackgroundColor="#D32F2F" 
                TextColor="White" 
                HorizontalOptions="Center" 
                Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>

```

### ⚙️ `VendedorPage.xaml.cs` (Lógica Restringida)

```csharp
namespace CRUD_LOGIN_MAUI;

public partial class VendedorPage : ContentPage
{
    public VendedorPage()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}

```

> 💡 **Idea de extensión:** el "Módulo de ventas" es el lugar natural para añadir un formulario de registro de pedidos o consulta de catálogo de productos, siempre manteniendo al Vendedor sin acceso a la gestión de usuarios o roles.

---

## 🧱 Buenas Prácticas y Consideraciones Adicionales

| Área | Recomendación |
|---|---|
| 🔐 Seguridad | Evitar cadenas de conexión *hardcodeadas*; usar `SecureStorage`, variables de entorno o un backend intermediario. |
| 🧂 Hashing | Agregar *salt* único por usuario antes de aplicar `HASHBYTES` para mayor resistencia ante ataques de diccionario. |
| 🌐 Producción | Reemplazar la conexión directa a SQL Server desde el cliente MAUI por una **API REST intermedia**, evitando exponer el servidor de base de datos directamente al dispositivo. |
| ♻️ Mantenibilidad | Extraer la lógica de acceso a datos (ADO.NET) a una capa de servicios/repositorios independiente de las páginas XAML (patrón MVVM). |
| 🧪 Validaciones | Añadir validaciones de formato (longitud mínima de contraseña, caracteres permitidos) antes de insertar/actualizar usuarios. |
| 🔌 Conectividad | Verificar siempre que el firewall, el protocolo TCP/IP y el puerto 1433 estén habilitados si la app y el servidor SQL están en máquinas distintas. |

---

## ✅ Conclusión

¡Eso es todo! 🎉 Siguiendo esta arquitectura modular y limpia, tu aplicación es **escalable, segura y cada rol tiene estrictamente acceso solo a lo que le corresponde**:

- 👑 **Admin** → control total (usuarios y roles).
- 👔 **Supervisor** → vista de reportes y supervisión (solo lectura).
- 🛍️ **Vendedor** → módulo operativo de ventas (solo lectura).

> 🚀 A partir de esta base, el proyecto puede crecer hacia una arquitectura **MVVM completa**, integración con una **API REST**, y funcionalidades adicionales por rol sin comprometer la separación de responsabilidades.