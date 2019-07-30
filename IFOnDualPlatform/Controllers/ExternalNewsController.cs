using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Reqeusts;

namespace IFOnDualPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalNewsController : ControllerBase
    {
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
			IAppResponse appResponse = new AppResponse();
			appResponse.ResponseData = "Response data";
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
