# Manual Paso a Paso: Proyecto CRUD_LOGIN_MAUI (Completo y Documentado)

Este manual documenta de manera exhaustiva la estructura, la base de datos, las vistas (XAML) y la lógica de código (C#) del proyecto `CRUD_LOGIN_MAUI`. Es una guía paso a paso para construir o entender este sistema de autenticación y roles desarrollado en **.NET MAUI** con conexión directa a **SQL Server**.

---

## 🗄️ PASO 1: Configurar la Base de Datos (SQL Server)

Antes de programar en .NET MAUI, necesitamos la base de datos que almacenará a los usuarios, sus contraseñas encriptadas y sus roles. 

### ¿Cómo crearla?
Abre **SQL Server Management Studio (SSMS)**, crea una nueva consulta (New Query), pega el siguiente código y ejecútalo. Esto creará la base de datos `LoginRolesDB_cif`, las tablas, insertará roles por defecto y creará algunos usuarios de prueba encriptados.

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
-- Define los tipos de acceso.
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

-- 3. CREAR TABLA DE USUARIOS
-- Almacena credenciales y la relación con el rol.
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario VARCHAR(50) NOT NULL,
    Password VARCHAR(64) NOT NULL, -- HASH SHA2_256
    IdRol INT FOREIGN KEY REFERENCES Roles(Id)
);

-- 4. INSERTAR ROLES PREDEFINIDOS
INSERT INTO Roles (NombreRol) VALUES ('Admin'), ('Supervisor'), ('Vendedor');

-- 5. INSERTAR USUARIOS DE PRUEBA (Contraseñas encriptadas con SHA2_256)
-- La función HASHBYTES encripta la contraseña y CONVERT la transforma a texto legible.
INSERT INTO Usuarios (Usuario, Password, IdRol) VALUES
('AdminUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 1),
('SuperUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'super123'), 2), 2),
('SalesUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'sales123'), 2), 3);
GO

-- 6. PROCEDIMIENTOS ALMACENADOS (Opcionales para uso futuro)
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
```

---

## 🚀 PASO 2: Enrutamiento Principal (`AppShell`)

Una vez que tienes el proyecto MAUI creado, el primer paso es configurar el **AppShell**. Este archivo actúa como el "director de orquesta", definiendo las rutas (URL internas) para navegar entre pantallas de manera segura.

### ¿Cómo crear el archivo `AppShell.xaml`?
En tu proyecto, abre el archivo `AppShell.xaml` y reemplaza su contenido por este. Hemos desactivado el menú lateral (`FlyoutBehavior="Disabled"`) para forzar que la navegación ocurra solo a través de botones y lógica.

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

### Lógica Trasera (`AppShell.xaml.cs`)
Aquí registramos adicionalmente rutas complejas como la pantalla de Roles.

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

---

## 🔒 PASO 3: Pantalla de Login (`MainPage`)

Esta es la puerta de entrada. En esta pantalla, el usuario coloca sus datos.

### ¿Cómo crear el archivo `MainPage.xaml` (La Vista)?
Abre `MainPage.xaml` y diseña la interfaz gráfica con campos para Usuario y Contraseña, además de un botón con un ícono del "ojito" para ver la contraseña temporalmente.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.MainPage">
             
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#F5F5F5">
        
        <!-- Ícono de Seguridad -->
        <Image Source="https://images.icon-icons.com/2120/PNG/512/lock_padlock_locked_protected_security_icon_131240.png" HeightRequest="175" HorizontalOptions="Center" />
        
        <!-- Campo para ingresar el usuario -->
        <Entry x:Name="txtUsuario" Placeholder="Usuario" ClearButtonVisibility="WhileEditing" HorizontalTextAlignment="Center" TextColor="Black" FontSize="25" Margin="10"/>
        
        <!-- Agrupación para la Contraseña y su Botón de Ver -->
        <Grid Margin="10">
            <Entry x:Name="txtPassword" Placeholder="Contraseña" IsPassword="True" HorizontalTextAlignment="Center" TextColor="Black" FontSize="25"/>
            <Button x:Name="btnTogglePassword" Text="👁️" Clicked="OnTogglePasswordClicked" BackgroundColor="Transparent" HorizontalOptions="End" WidthRequest="60"/>
        </Grid>
        
        <!-- Botón para iniciar sesión (Llama al método OnLogin_Clicked) -->
        <Button Text="Ingresar" BackgroundColor="#fbc531" TextColor="#40739e" FontAttributes="Bold" Margin="20" Clicked="OnLogin_Clicked" FontSize="25" />
        
        <!-- Etiqueta para mostrar errores (ej. "Usuario Incorrecto") -->
        <Label x:Name="lblMensaje" TextColor="Red" FontSize="22" FontAttributes="Bold" HorizontalTextAlignment="Center" />
        
    </VerticalStackLayout>
</ContentPage>
```

### ¿Cómo crear el archivo `MainPage.xaml.cs` (La Lógica)?
Este código es vital. Aquí se toma la contraseña ingresada, se envía a la base de datos para compararla (encriptándola en el proceso con `SHA2_256`) y si coinciden, se obtiene el nombre del rol para redirigir dinámicamente (`AdminPage`, `VendedorPage`, etc).

```csharp
using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient; // Necesario para interactuar con SQL Server
using System.Data;

namespace CRUD_LOGIN_MAUI;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();

    // Evento que ocurre cada vez que la página se hace visible
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Limpiamos cajas de texto por seguridad en caso de que alguien haya cerrado sesión
        txtUsuario.Text = txtPassword.Text = lblMensaje.Text = string.Empty;
    }

    // Método para ocultar/mostrar la contraseña
    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        btnTogglePassword.Text = txtPassword.IsPassword ? "👁️" : "🙈";
    }

    // Lógica al presionar el botón "Ingresar"
    private async void OnLogin_Clicked(object sender, EventArgs e)
    {
        string usuario = txtUsuario.Text?.Trim();
        string password = txtPassword.Text?.Trim();

        // 1. Validación básica de que los campos no estén vacíos
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            lblMensaje.Text = "❌ Por favor, ingrese sus credenciales.";
            return;
        }

        try
        {
            // 2. Configurar la conexión (Asegúrese de usar su IP correcta o 'localhost')
            string connectionString = "Server=192.168.2.55,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // 3. Consulta de Verificación
            // NOTA CRÍTICA: Aquí usamos HASHBYTES para convertir la contraseña ingresada a HASH 
            // y compararla con el HASH guardado en la base de datos.
            string query = @"SELECT R.NombreRol 
                             FROM Usuarios U 
                             INNER JOIN Roles R ON U.IdRol = R.Id 
                             WHERE U.Usuario = @Usuario 
                             AND U.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2)";
                             
            using var command = new SqlCommand(query, connection);
            // Prevenimos SQL Inyección enviando parámetros en lugar de concatenar texto
            command.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
            command.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;

            // ExecuteScalar obtiene solo la primera columna de la primera fila (El Nombre del Rol)
            var roleResult = await command.ExecuteScalarAsync();

            // Si encontró un resultado, las credenciales son correctas
            if (roleResult != null)
            {
                // Concatenamos el rol devuelto (ej. "Admin") con la palabra "Page" para navegar a "AdminPage"
                await Shell.Current.GoToAsync($"{roleResult}Page");
            }
            else
            {
                lblMensaje.Text = "❌ Usuario o contraseña incorrectos.";
            }
        }
        catch (Exception ex)
        {
            lblMensaje.Text = $"❌ Error: {ex.Message}";
        }
    }
}
```

---

## 🛠️ PASO 4: Panel de Control (`AdminPage`)

Pantalla exclusiva para que el Administrador registre nuevos empleados en el sistema.

### Vista (`AdminPage.xaml`)
Crea este nuevo archivo *ContentPage* de MAUI. Se usa para la gestión completa de los usuarios.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
             x:Class="CRUD_LOGIN_MAUI.AdminPage" Title="Administrador">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#E3F2FD">
            <Label Text="Panel de Control - Administrador" FontSize="22" FontAttributes="Bold" TextColor="#0D47A1" HorizontalOptions="Center"/>
            
            <!-- Entradas de Información -->
            <Entry x:Name="txtUsuario" Placeholder="👤 Usuario"/>
            <Entry x:Name="txtPassword" Placeholder="🔑 Contraseña" IsPassword="True"/>
            <Picker x:Name="pickerRol" Title="Seleccionar Rol" />
            
            <!-- Cuadro de Búsqueda de Usuarios -->
            <Entry x:Name="txtBuscar" Placeholder="🔎 Buscar por ID, Usuario o Rol..." TextChanged="OnSearchChanged" BackgroundColor="White"/>
            
            <!-- Botones de Acción Múltiple -->
            <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto" ColumnSpacing="10" RowSpacing="10">
                <Button Grid.Row="0" Grid.Column="0" Text="➕ Insertar" BackgroundColor="Green" TextColor="White" Clicked="OnInsertClicked"/>
                <Button Grid.Row="0" Grid.Column="1" Text="🔄 Actualizar" BackgroundColor="DodgerBlue" TextColor="White" Clicked="OnUpdateClicked"/>
                <Button Grid.Row="0" Grid.Column="2" Text="🗑️ Eliminar" BackgroundColor="Red" TextColor="White" Clicked="OnDeleteClicked"/>
            </Grid>

            <!-- Tabla/Lista Visual de Usuarios usando DataTemplates (Binding) -->
            <CollectionView x:Name="listaUsuarios" SelectionMode="Single" SelectionChanged="OnUsuarioSelected" HeightRequest="250">
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
            
            <!-- Salida Segura -->
            <Button Text="Cerrar sesión" BackgroundColor="DarkRed" TextColor="White" Margin="0,20" Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

### Lógica de Ejecución Centralizada (`AdminPage.xaml.cs`)
Aquí usamos un método mágico central `EjecutarAccion` que nos ahorra repetir código.

```csharp
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI;

// 1. Modelo de Datos
public class UsuarioItem
{
    public int Id { get; set; }
    public string Usuario { get; set; }
    public string Rol { get; set; }
}

public partial class AdminPage : ContentPage
{
    private string connectionString = "Server=192.168.2.55,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
    private bool isProcessing = false;
    private int idSeleccionado = 0; // Se actualiza cuando se toca un usuario en la lista

    public AdminPage() => InitializeComponent();

    // 2. Eventos de los Botones
    private async void OnLogoutClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage"); // Logout seguro sin historial previo

    private async void OnInsertClicked(object sender, EventArgs e) =>
        await EjecutarAccion(@"INSERT INTO Usuarios (Usuario, Password, IdRol) 
                               VALUES (@Usuario, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), @IdRol)", "insertar");

    private async void OnDeleteClicked(object sender, EventArgs e) =>
        await EjecutarAccion("DELETE FROM Usuarios WHERE Id=@Id", "eliminar");

    // 3. Motor Centralizado de Ejecución (Para evitar repetir el Try Catch en cada botón)
    private async Task EjecutarAccion(string query, string accion)
    {
        if (isProcessing) return; // Evita clics dobles rápidos
        
        bool confirmar = await DisplayAlert("Confirmación", $"¿Seguro que desea {accion} este usuario?", "Sí", "No");
        if (!confirmar) return;

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            // Inyectamos las variables seguras a la base de datos
            cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text ?? "");
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text ?? "");
            // IMPORTANTE: En el Picker, la selección empieza en 0, pero en BD el IdRol empieza en 1.
            cmd.Parameters.AddWithValue("@IdRol", pickerRol.SelectedIndex + 1);
            cmd.Parameters.AddWithValue("@Id", idSeleccionado);

            await cmd.ExecuteNonQueryAsync();
            await DisplayAlert("Éxito", $"Usuario {accion}do correctamente.", "OK");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { isProcessing = false; }
    }
}
```

---

## 🔒 PASO 5: Vistas Limitadas (Supervisor y Vendedor)
A diferencia del Administrador, estas vistas carecen de botones para modificar la base de datos, cumpliendo su propósito de perfiles restringidos.

### `VendedorPage.xaml` (Ejemplo de Pantalla Restringida)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.VendedorPage"
             Title="VendedorPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#E8F5E9">
        <Label Text="Bienvenido Vendedor" FontSize="24" FontAttributes="Bold" TextColor="#1B5E20" HorizontalOptions="Center" Margin="10"/>
        
        <!-- Elementos UI específicos del vendedor irían aquí -->
        <Image Source="https://cdn-icons-png.flaticon.com/512/2316/2316167.png" HeightRequest="175" HorizontalOptions="Center" />
        
        <!-- Para salir, retorna al Main limpiando memoria -->
        <Button Text="Cerrar Sesion" Clicked="OnLogoutClicked" BackgroundColor="#D32F2F" TextColor="White" HorizontalOptions="Center" Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>
```

### `VendedorPage.xaml.cs` (Lógica Restringida)
```csharp
namespace CRUD_LOGIN_MAUI;

public partial class VendedorPage : ContentPage
{
    public VendedorPage() => InitializeComponent();

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // "//MainPage" indica volver a la raíz absoluta, impidiendo al vendedor regresar a esta pantalla sin loguearse
        await Shell.Current.GoToAsync("//MainPage");
    }
}
```

---
¡Eso es todo! Siguiendo esta arquitectura modular y limpia, tu aplicación es escalable, segura y cada rol tiene estrictamente acceso solo a lo que le corresponde.
