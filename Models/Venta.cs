using System.ComponentModel.DataAnnotations;

namespace Gestión_de_Inventario_Huevos_del_Campo.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public decimal Total { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre del cliente debe tener al menos 3 caracteres.")]
        [RegularExpression(@"^[\p{L}0-9\s\.\-']+$", ErrorMessage = "El nombre del cliente contiene caracteres inválidos.")]
        public string Cliente { get; set; } = string.Empty;


        public bool Anulada { get; set; } = false;


        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
