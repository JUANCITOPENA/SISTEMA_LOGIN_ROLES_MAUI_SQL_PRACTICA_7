# Punto de Partida: CRUD_LOGIN_MAUI (Versión 1.0)

Este es el punto de restauración estable y funcional del sistema de autenticación y roles desarrollado en .NET MAUI.

## 🛠️ Stack Tecnológico
- **Frontend:** .NET MAUI (XAML / C#).
- **Backend / DB:** SQL Server (Conectado de manera directa para desarrollo en red local).
- **Paquetes NuGet:** `Microsoft.Data.SqlClient` (7.0.2).

## 🚀 Mejoras Implementadas
- **Navegación Limpia:** Se implementó `await Shell.Current.GoToAsync("//MainPage")` al hacer logout, asegurando que la pila de navegación se limpie y no se consuma memoria adicional o quede un encadenamiento visual indeseado.
- **Red Local:** La cadena de conexión fue actualizada a la IP correcta (`192.168.218.18`).
- **Roles:** Soporte y distribución de roles con vistas separadas (Admin, Supervisor, Vendedor, y la página de control de Roles).
- **Compilación Cero Errores:** Se verificó y compiló el proyecto base libre de dependencias rotas y problemas de sintaxis.

## 🗂️ Estructura del Proyecto
- `MainPage`: Pantalla principal y de inicio de sesión (Login).
- `AdminPage`: Panel CRUD global para administración de usuarios (insertar, actualizar, eliminar, consultar y validar).
- `RolesPage`: Pantalla para la gestión y CRUD dinámico de roles de usuario en BD.
- `SupervisorPage`: Panel de reportes de supervisión.
- `VendedorPage`: Módulo de acceso para vendedores.
- `AppShell`: Gestor de las rutas del proyecto y ruteo centralizado.

*Creado por Antigravity CLI / Ing. Juancito Peña.*
