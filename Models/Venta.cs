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
        [MaxLength(100)]
        public string Cliente { get; set; } = string.Empty;

        public bool Anulada { get; set; } = false;


        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
