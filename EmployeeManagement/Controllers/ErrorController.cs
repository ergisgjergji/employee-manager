using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }


        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch(statusCode)
            {
                case 404:
                    ViewBag.Path = statusCodeResult.OriginalPath;
                    ViewBag.QueryString = statusCodeResult.OriginalQueryString;

                    string warningMessage = "404 error occured! " +
                        $"\nPath: '{statusCodeResult.OriginalPath}'" +
                        $"\nQueryString: '{statusCodeResult.OriginalQueryString}'";

                    logger.LogWarning(warningMessage);
                    break;
            }
            return View("PageNotFound");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            string errorMessage = "An exception occurred!" +
                $"\nPath: {exceptionDetails.Path} " +
                $"\nException:\n {exceptionDetails.Error}'";

            logger.LogError(errorMessage);

            return View("Error");
        }
    }
}