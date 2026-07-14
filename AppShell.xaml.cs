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
