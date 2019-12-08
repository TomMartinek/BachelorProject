using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BachelorProject.Controllers
{
    /** 
        Tento konroler obsahuje logiku spojenou s chybami vzniklími při práci s aplikací.
    */

    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        // konstruktor této třídy
        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        // metoda pro zpracování http požadavku s kódem 404
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Omlouváme se, tento zdroj nebyl nalezen";
                    logger.LogWarning($"404 Error Occured. Path = {statusCodeResult.OriginalPath}" +
                                      $" and QueryString = {statusCodeResult.OriginalQueryString}");
                    break;
            }

            return View("NotFound");
        }

        // metoda pro zpracování a zalogování chyby
        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            logger.LogError($"The path {exceptionDetails.Path} " +
                            $"threw an exception {exceptionDetails.Error}");

            return View("Error");

        }
    }
}
