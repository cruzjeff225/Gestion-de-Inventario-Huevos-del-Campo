using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace Gestión_de_Inventario_Huevos_del_Campo.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(
            this Controller controller,
            string viewName,
            TModel model,
            bool partial = false)
        {
            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            var engine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var viewResult = engine.FindView(controller.ControllerContext, viewName, !partial);

            if (viewResult.View == null)
                throw new FileNotFoundException($"La vista '{viewName}' no fue encontrada.");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }
    }
}
