using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimplCommerce.SearchApi.HttpManager;
using System;
using System.Threading.Tasks;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.SearchApi.ViewModels;

namespace SimplCommerce.SearchApi.Controllers
{
    [ApiController]
    public class SearchController : Controller
    {
        private readonly ILogger _logger;
        private readonly IElasticClient _elasticClient;

        public SearchController(ILogger<SearchController> logger, IElasticClient elasticClient)
        {
            _logger = logger;
            _elasticClient = elasticClient;
        }

        [HttpGet("api/search/{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }


        [Route("api/search")]
        [HttpPost]
        public async Task<IActionResult> HandleAsync([FromBody]SearchOption searchoption)
        {
            try
            {
                var response = new SearchResult();
                await Task.Run(() => { });
                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error in request.{ex}");
                return BadRequest();
            }
        }
    }
}