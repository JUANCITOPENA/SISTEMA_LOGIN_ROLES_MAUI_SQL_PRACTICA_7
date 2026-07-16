# Manual Paso a Paso: Proyecto CRUD_LOGIN_MAUI (Completo)

Este manual documenta la estructura, las vistas (XAML) y la lógica de código (C#) del proyecto `CRUD_LOGIN_MAUI`, un sistema de autenticación y roles desarrollado en .NET MAUI con conexión directa a SQL Server.

## 🗂️ 1. Punto de Partida (README)
El proyecto incluye un archivo `README_PUNTO_DE_PARTIDA.md` que establece las bases tecnológicas:
- **Frontend:** .NET MAUI (XAML / C#).
- **Backend / DB:** SQL Server (Red Local).
- **Paquetes NuGet:** `Microsoft.Data.SqlClient` (7.0.2).

---

## 🚀 2. Enrutamiento Principal (`AppShell`)
El archivo `AppShell` maneja la pila de navegación de MAUI, permitiendo una experiencia limpia. Todas las pantallas registran sus rutas para navegar dinámicamente sin encadenar vistas innecesarias.

### `AppShell.xaml`
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="CRUD_LOGIN_MAUI.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
    Title="CRUD_LOGIN_MAUI"
    FlyoutBehavior="Disabled">

    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate local:MainPage}"
        Route="MainPage" />

    <ShellContent Route="AdminPage" ContentTemplate="{DataTemplate local:AdminPage}" FlyoutItem.IsVisible="False"/>
    <ShellContent Route="SupervisorPage" ContentTemplate="{DataTemplate local:SupervisorPage}" FlyoutItem.IsVisible="False"/>
    <ShellContent Route="VendedorPage" ContentTemplate="{DataTemplate local:VendedorPage}" FlyoutItem.IsVisible="False"/>
</Shell>
```

### `AppShell.xaml.cs`
```csharp
namespace CRUD_LOGIN_MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("AdminPage", typeof(AdminPage));
        Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));
        Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));
        Routing.RegisterRoute("RolesPage", typeof(RolesPage));
    }
}
```

---

## 📄 3. Pantallas Principales (Vistas y Lógica)

### 3.1. Pantalla de Acceso (`MainPage`)
Maneja la lógica de inicio de sesión conectando a la base de datos, validando usuarios con SHA2_256 y redireccionando a la vista del rol correspondiente.

**`MainPage.xaml`**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.MainPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#F5F5F5">
        <Image Source="https://images.icon-icons.com/2120/PNG/512/lock_padlock_locked_protected_security_icon_131240.png" HeightRequest="175" HorizontalOptions="Center" />
        <Entry x:Name="txtUsuario" Placeholder="Usuario" ClearButtonVisibility="WhileEditing" HorizontalTextAlignment="Center" TextColor="Black" FontSize="25" Margin="10"/>
        <Grid Margin="10">
            <Entry x:Name="txtPassword" Placeholder="Contraseña" IsPassword="True" HorizontalTextAlignment="Center" TextColor="Black" FontSize="25"/>
            <Button x:Name="btnTogglePassword" Text="👁️" Clicked="OnTogglePasswordClicked" BackgroundColor="Transparent" HorizontalOptions="End" WidthRequest="60"/>
        </Grid>
        <Button Text="Ingresar" BackgroundColor="#fbc531" TextColor="#40739e" FontAttributes="Bold" Margin="20" Clicked="OnLogin_Clicked" FontSize="25" />
        <Label x:Name="lblMensaje" TextColor="Red" FontSize="22" FontAttributes="Bold" HorizontalTextAlignment="Center" />
    </VerticalStackLayout>
</ContentPage>
```

**`MainPage.xaml.cs`**
```csharp
using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        txtUsuario.Text = txtPassword.Text = lblMensaje.Text = string.Empty;
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
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT R.NombreRol FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id WHERE U.Usuario = @Usuario AND U.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
            command.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;

            var roleResult = await command.ExecuteScalarAsync();

            if (roleResult != null)
            {
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

### 3.2. Panel del Administrador (`AdminPage`)
Vista completa para la gestión y creación de usuarios dentro de la aplicación.

**`AdminPage.xaml`** (Estructura resumida para legibilidad)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI"
             x:Class="CRUD_LOGIN_MAUI.AdminPage" Title="Administrador">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#E3F2FD">
            <Label Text="Panel de Control - Administrador" FontSize="22" FontAttributes="Bold" TextColor="#0D47A1" HorizontalOptions="Center"/>
            <Entry x:Name="txtUsuario" Placeholder="👤 Usuario"/>
            <Entry x:Name="txtPassword" Placeholder="🔑 Contraseña" IsPassword="True"/>
            <Picker x:Name="pickerRol" Title="Seleccionar Rol"> ... </Picker>
            <Entry x:Name="txtBuscar" Placeholder="🔎 Buscar por ID, Usuario o Rol..." TextChanged="OnSearchChanged" BackgroundColor="White"/>
            <!-- Botones (Insertar, Actualizar, Eliminar, Consultar, etc.) -->
            <CollectionView x:Name="listaUsuarios" SelectionMode="Single" SelectionChanged="OnUsuarioSelected" HeightRequest="250">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="local:UsuarioItem"> ... </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
            <Button Text="⚙️ Roles" BackgroundColor="Teal" TextColor="White" Clicked="OnRolesClicked"/>
            <Button Text="Cerrar sesión" BackgroundColor="DarkRed" TextColor="White" Margin="0,20" Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

**`AdminPage.xaml.cs`** (Resumen)
Se encarga de ejecutar operaciones SQL (`INSERT`, `UPDATE`, `DELETE`) directamente con `SqlConnection` e invoca al `RolesPage`.

### 3.3. Panel de Roles (`RolesPage`)
Gestiona directamente los roles permitidos en el sistema.

**`RolesPage.xaml`** (Estructura)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" xmlns:local="clr-namespace:CRUD_LOGIN_MAUI" x:Class="CRUD_LOGIN_MAUI.RolesPage" Title="Gestión de Roles">
    <!-- UI con Entradas de Rol, Buscador y Grid con botones CRUD -->
</ContentPage>
```

**`RolesPage.xaml.cs`** (Lógica)
Conecta a la base de datos para ejecutar `INSERT`, `UPDATE` y `DELETE` sobre la tabla de Roles, actualizando automáticamente el `CollectionView` en pantalla.

### 3.4. Pantalla de Vendedor (`VendedorPage`)
Una vista limpia y sencilla sin acceso administrativo.

**`VendedorPage.xaml`**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.VendedorPage"
             Title="VendedorPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#E8F5E9">
        <Label Text="Bienvenido Vendedor" FontSize="24" FontAttributes="Bold" TextColor="#1B5E20" HorizontalOptions="Center" Margin="10"/>
        <Image Source="https://cdn-icons-png.flaticon.com/512/2316/2316167.png" HeightRequest="175" HorizontalOptions="Center" />
        <Label Text="Ventana: Modulo de ventas" FontSize="18" TextColor="Black" HorizontalOptions="Center" Margin="10"/>
        <Button Text="Cerrar Sesion" Clicked="OnLogoutClicked" BackgroundColor="#D32F2F" TextColor="White" HorizontalOptions="Center" Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>
```
**`VendedorPage.xaml.cs`**
```csharp
namespace CRUD_LOGIN_MAUI;

public partial class VendedorPage : ContentPage
{
    public VendedorPage() => InitializeComponent();

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
```

### 3.5. Pantalla de Supervisor (`SupervisorPage`)
Interfaz exclusiva para perfil de supervisión.

**`SupervisorPage.xaml`**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.SupervisorPage"
             Title="SupervisorPage">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#FFF3E0">
        <Label Text="Bienvenido Supervisor" FontSize="24" FontAttributes="Bold" TextColor="#E65100" HorizontalOptions="Center" Margin="10"/>
        <Image Source="https://cdn-icons-png.flaticon.com/256/3461/3461567.png" HeightRequest="175" HorizontalOptions="Center" />
        <Label Text="Ventana: Reportes y supervision" FontSize="18" TextColor="Black" HorizontalOptions="Center" Margin="10" />
        <Button Text="Cerrar Sesion" Clicked="OnLogoutClicked" BackgroundColor="#D32F2F" TextColor="White" HorizontalOptions="Center" Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>
```
**`SupervisorPage.xaml.cs`**
```csharp
namespace CRUD_LOGIN_MAUI;

public partial class SupervisorPage : ContentPage
{
    public SupervisorPage() => InitializeComponent();

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
```

---

## 🎯 4. Conclusión
El proyecto emplea **XAML** para construir de forma declarativa e independiente cada vista de rol, mientras que la parte de **C#** controla los eventos y efectúa las conexiones a la Base de Datos para verificar las credenciales y operar sobre el gestor de personal y roles.
