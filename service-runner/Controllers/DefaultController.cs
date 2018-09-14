using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace service_runner.Controllers
{
    [Route("/")]
    public class DefaultController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            return Redirect("index.html");
        }
    }
}
