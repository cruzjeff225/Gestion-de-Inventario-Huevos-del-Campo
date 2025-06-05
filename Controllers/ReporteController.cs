using Gestión_de_Inventario_Huevos_del_Campo.Db;
using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Gestión_de_Inventario_Huevos_del_Campo.PDF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextDocument = iTextSharp.text.Document;
using PageSize = iTextSharp.text.PageSize;

namespace Gestión_de_Inventario_Huevos_del_Campo.Controllers
{
    public class ReporteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReporteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ExportarPdf()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var reporte = new ReporteVentasPdf(ventas, "Reporte de Ventas");
            var pdfBytes = reporte.GeneratePdf();

            return File(pdfBytes, "application/pdf", "ReporteVentas.pdf");
        }

        public async Task<IActionResult> Productos(FiltroFechasViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GenerarPDFProductos(DateTime? FechaInicio, DateTime? FechaFin)
        {
            var productos = _context.Productos.AsQueryable();

            if (FechaInicio.HasValue)
                productos = productos.Where(p => p.FechaRegistro >= FechaInicio.Value);

            if (FechaFin.HasValue)
                productos = productos.Where(p => p.FechaRegistro <= FechaFin.Value);

            var listaProductos = productos.ToList();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                iTextDocument document = new iTextDocument(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                writer.CloseStream = false;
                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Reporte de Productos", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;

                string[] headers = { "Nombre", "Cantidad", "Precio", "Lote", "Fecha Registro", "Fecha Vencimiento" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                foreach (var producto in listaProductos)
                {
                    table.AddCell(new PdfPCell(new Phrase(producto.Nombre)));
                    table.AddCell(new PdfPCell(new Phrase(producto.Cantidad.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(producto.PrecioCompra.ToString("C"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.Lote)));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaRegistro.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaVencimiento.ToString("yyyy-MM-dd"))));
                    
                }

                document.Add(table);
                document.Close();

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Productos.pdf");
            }
        }
    }


}
