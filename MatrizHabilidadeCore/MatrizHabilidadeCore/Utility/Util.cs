using NPOI.SS.UserModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace MatrizHabilidadeCore.Utility
{
    public static class Util
    {
        public static DateTime DateTimeNow()
        {
            return DateTime.UtcNow.AddHours(-3);
        }

        public static string GetFarol(this double value)
        {
            var result = "";

            if (value <= 20)
            {
                result = "green";
            }
            else if (20 < value && value < 50)
            {
                result = "yellow";
            }
            else if (value >= 50)
            {
                result = "red";
            }

            return result;
        }

        public static bool IsCellDateFormatted(this ICell cell)
        {
            return DateUtil.IsCellDateFormatted(cell);
        }

        public static string GetStringValue(this ICell cell)
        {
            string value = "";

            if (cell != null)
            {
                if (cell.CellType == CellType.String)
                {
                    value = cell.StringCellValue;
                }
                else if (cell.CellType == CellType.Numeric)
                {
                    if (cell.IsCellDateFormatted())
                    {
                        value = cell.DateCellValue.ToString("dd/MM/yyyy HH:mm:ss");
                    }
                    else
                    {
                        value = cell.NumericCellValue.ToString();
                    }
                }
            }

            return value;
        }
    }

    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

            if (viewResult.Success == false)
            {
                return $"A view with the name {viewName} could not be found";
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime value)
        {
            return value.ToString("dd/MM/yyyy HH:mm");
        }
        
    }
}