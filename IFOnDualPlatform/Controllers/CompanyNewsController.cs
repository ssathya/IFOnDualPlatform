using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Reqeusts;
using Microsoft.Extensions.Logging;
using Utilities.Application;

namespace IFOnDualPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	public class CompanyNewsController : ControllerBase
    {
		private readonly ILogger<CompanyNewsController> _logger;
		private readonly EnvHandler _envHandler;

		public CompanyNewsController(ILogger<CompanyNewsController> logger, EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
		}
        // GET: api/ExternalNews
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ExternalNews/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ExternalNews
        [HttpPost]
        public IActionResult Post([FromBody] CompanyNews companyNews)
        {
			_logger.LogInformation("In Company News Controller");
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = "Response data",
				IsResponseSuccess = true
			};
			return Ok(appResponse);

		}

        // PUT: api/ExternalNews/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
