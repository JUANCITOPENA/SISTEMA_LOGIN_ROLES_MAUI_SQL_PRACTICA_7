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
