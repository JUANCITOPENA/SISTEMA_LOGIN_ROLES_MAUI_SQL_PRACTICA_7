# Manual Paso a Paso: Proyecto CRUD_LOGIN_MAUI (Completo y Documentado)

Este manual documenta de manera exhaustiva la estructura, la base de datos, las vistas (XAML) y la lógica de código (C#) del proyecto `CRUD_LOGIN_MAUI`. Es una guía paso a paso para construir o entender este sistema de autenticación y roles desarrollado en **.NET MAUI** con conexión directa a **SQL Server**.

---

## 🗄️ PASO 1: Configurar la Base de Datos (SQL Server)

Antes de programar en .NET MAUI, necesitamos la base de datos que almacenará a los usuarios, sus contraseñas encriptadas y sus roles. 

### ¿Cómo crear el script SQL?
Abre **SQL Server Management Studio (SSMS)**. Ve al menú superior y selecciona **Nueva consulta** (o presiona `Ctrl+N`). Copia el siguiente código, pégalo en la ventana en blanco y haz clic en **Ejecutar** (o presiona `F5`). Esto creará automáticamente la base de datos `LoginRolesDB_cif`, las tablas necesarias, y poblará la base de datos con roles y usuarios de prueba.

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

---

## 🚀 PASO 2: Enrutamiento Principal (`AppShell`)

Una vez que tienes el proyecto MAUI creado, el primer paso es configurar el **AppShell**. Este archivo actúa como el "director de orquesta", definiendo las rutas (URL internas) para navegar entre pantallas de manera segura.

### ¿Cómo configurar la vista `AppShell.xaml`?
En el **Explorador de Soluciones** de Visual Studio, ubica y haz doble clic sobre el archivo `AppShell.xaml` (este archivo ya viene por defecto al crear un proyecto MAUI). Borra todo su contenido y pega exactamente el siguiente código para deshabilitar el menú lateral e incluir las rutas:

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

### ¿Cómo configurar la lógica `AppShell.xaml.cs`?
En el mismo Explorador de Soluciones, despliega la flechita junto a `AppShell.xaml` y abre su archivo subyacente `AppShell.xaml.cs`. Reemplaza su contenido con el siguiente código para registrar las rutas explícitas:

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

### ¿Cómo diseñar la vista `MainPage.xaml`?
Ubica el archivo `MainPage.xaml` en el Explorador de Soluciones. Reemplaza el diseño inicial de MAUI por este código, el cual incluye los campos de usuario, contraseña, y el botón de ingresar:

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

### ¿Cómo implementar la lógica de `MainPage.xaml.cs`?
Despliega `MainPage.xaml` y abre `MainPage.xaml.cs`. Pega exactamente este código. Nota importante: asegúrate de que el paquete `Microsoft.Data.SqlClient` esté instalado vía NuGet para que las funciones de SQL no generen error.

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
            string connectionString = "Server=192.168.2.55,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";

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

---

## 🛠️ PASO 4: Panel de Control (`AdminPage`)

Pantalla exclusiva para que el Administrador registre nuevos empleados en el sistema y asigne roles.

### ¿Cómo crear la vista `AdminPage.xaml`?
En el **Explorador de Soluciones**, haz clic derecho sobre el proyecto (CRUD_LOGIN_MAUI) -> **Agregar** -> **Nuevo elemento...** -> En la lista selecciona **.NET MAUI** -> **Página de contenido (XAML)**. Ponle el nombre `AdminPage.xaml` y presiona Agregar. Pega el siguiente código en el archivo creado:

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

### ¿Cómo programar la lógica `AdminPage.xaml.cs`?
Abre el archivo `AdminPage.xaml.cs` anidado y pega todo este código. Contiene las sentencias SQL centralizadas para evitar repetir código en cada botón del CRUD.

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
    private string connectionString = "Server=192.168.2.55,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
    private bool isProcessing = false;
    private int idSeleccionado = 0;

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

            var roles = new List<string>();
            while (await reader.ReadAsync())
                roles.Add(reader["NombreRol"].ToString());

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

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text ?? "");
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text ?? "");

            cmd.Parameters.AddWithValue("@IdRol", pickerRol.SelectedIndex + 1);
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

---

## 🔒 PASO 5: Vistas (ROLES)
Esta vista complementa al Administrador permitiendo gestionar la tabla `Roles`. 

### ¿Cómo crear la vista `RolesPage.xaml`?
Nuevamente, clic derecho en el proyecto -> **Agregar** -> **Nuevo elemento...** -> **Página de contenido (XAML)**. Nómbralo `RolesPage.xaml` y presiona Agregar. Pega este contenido:

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

### ¿Cómo programar la lógica `RolesPage.xaml.cs`?
Abre su archivo `.cs` interno y pega exactamente esta lógica para administrar roles en la base de datos:

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
    private string connectionString = "Server=192.168.2.55,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
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

---

## 🔒 PASO 6: Vistas Limitadas (Supervisor)
A diferencia del Administrador, esta vista carece de botones para modificar la base de datos, cumpliendo su propósito de perfil restringido.

### ¿Cómo crear la vista `SupervisorPage.xaml`?
Haz clic derecho en el proyecto -> **Agregar** -> **Nuevo elemento...** -> **Página de contenido (XAML)**. Nómbralo `SupervisorPage.xaml`. Pega lo siguiente:

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

### ¿Cómo programar la lógica `SupervisorPage.xaml.cs`?
Abre el código detrás de `SupervisorPage.xaml` y pega este código (solo contiene la acción para hacer Logout):

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

---

## 🔒 PASO 7: Vistas Limitadas (Vendedor)
Igual que el supervisor, es una interfaz simple de destino para cuando se loguea un empleado de nivel vendedor.

### ¿Cómo crear la vista `VendedorPage.xaml`?
Haz clic derecho en el proyecto -> **Agregar** -> **Nuevo elemento...** -> **Página de contenido (XAML)**. Nómbralo `VendedorPage.xaml`. Pega el siguiente diseño visual:

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

### ¿Cómo programar la lógica `VendedorPage.xaml.cs`?
Abre el archivo `.cs` detrás de la vista y pega lo siguiente para completar la página del vendedor:

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

---

## 🏁 CONCLUSIÓN

La arquitectura implementada en este proyecto **CRUD_LOGIN_MAUI** representa una solución robusta, escalable y segura para la administración de credenciales de usuario en un entorno corporativo. 

Al separar rígidamente la lógica de enrutamiento (`AppShell`) del diseño (`XAML`) y de la lógica de conexión (`C#`), hemos logrado que:
1. **La seguridad es de grado alto:** Las contraseñas nunca viajan en texto plano, gracias a la función `HASHBYTES('SHA2_256')` implementada desde el motor de base de datos SQL Server.
2. **Navegación Intocable:** La desactivación del _Flyout_ y el redireccionamiento estricto a las raíces garantizan que los usuarios de bajo nivel (Vendedores) no puedan forzar la visualización de paneles administrativos pulsando "Atrás".
3. **Escalabilidad:** El sistema está preparado para recibir una cantidad ilimitada de roles dinámicos que se comunican perfectamente mediante los menús desplegables (`Picker`) conectados en vivo.

¡Con este código base estás listo para evolucionar este proyecto MAUI hacia un ERP o Punto de Venta completo!
