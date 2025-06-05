namespace Gestión_de_Inventario_Huevos_del_Campo.Models
{
    public class UsuariosViewModel
    {
        public Usuarios NuevoUsuario { get; set; } = new Usuarios(); // Usuario a agregar
        public string SearchString { get; set; } = string.Empty; // Para buscar usuarios
        public List<Usuarios> ListaUsuarios { get; set; } = new List<Usuarios>();
    }
}