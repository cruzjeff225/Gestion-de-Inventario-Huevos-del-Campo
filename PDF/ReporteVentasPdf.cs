using Gestión_de_Inventario_Huevos_del_Campo.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gestión_de_Inventario_Huevos_del_Campo.PDF
{
    public class ReporteVentasPdf : IDocument
    {
        private readonly List<Venta> ventas;
        private readonly string titulo;

        public ReporteVentasPdf(List<Venta> ventas, string titulo)
        {
            this.ventas = ventas;
            this.titulo = titulo;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(titulo).FontSize(18).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    foreach (var venta in ventas)
                    {
                        col.Item().Text($"Venta #{venta.Id} - {venta.Fecha:dd/MM/yyyy} - Cliente: {venta.Cliente}").Bold();
                        col.Item().Element(c => CrearTablaDetalles(c, venta));
                        col.Item().PaddingBottom(10).Element(e =>
                            e.Text($"Total: ${venta.Total:F2}")
                            .FontSize(12)
                            .Bold()
                            .AlignRight()
                        );
                        col.Item().PaddingVertical(10).Element(e =>
                            e.LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2)
                        );
                    }
                });
            });
        }

        void CrearTablaDetalles(IContainer container, Venta venta)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(); // Producto
                    columns.ConstantColumn(60); // Cantidad
                    columns.ConstantColumn(80); // Precio
                    columns.ConstantColumn(80); // Subtotal
                });

                table.Header(header =>
                {
                    header.Cell().Text("Producto").Bold();
                    header.Cell().Text("Cant.").Bold();
                    header.Cell().Text("Precio").Bold();
                    header.Cell().Text("Subtotal").Bold();
                });

                foreach (var detalle in venta.Detalles)
                {
                    table.Cell().Text(detalle.Producto.Nombre);
                    table.Cell().Text(detalle.Cantidad.ToString());
                    table.Cell().Text($"${detalle.PrecioUnitario:F2}");
                    table.Cell().Text($"${detalle.Subtotal:F2}");
                }
            });
        }
    }
}
