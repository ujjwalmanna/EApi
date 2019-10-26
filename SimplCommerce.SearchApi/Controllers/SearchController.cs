using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimplCommerce.SearchApi.HttpManager;
using System;
using System.Threading.Tasks;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.SearchApi.ViewModels;
using Microsoft.Extensions.Options;
using SimplCommerce.SearchApi.Configurations;
using SimplCommerce.QueryBuilder;
using System.Net.Http;
using System.Threading;
using System.Text;
using SimplCommerce.SearchApi.Extensions;
using SimplCommerce.Module.Core.Models;
using System.Linq;

namespace SimplCommerce.SearchApi.Controllers
{
    [ApiController]
    public class SearchController : Controller
    {
        private readonly ILogger _logger;
        private readonly IElasticClient _elasticClient;
        private readonly ISearchQueryBuilder _searchQueryBuilder;
        private readonly IOptions<ElasticSettings> _config;

        public SearchController(ILogger<SearchController> logger, IElasticClient elasticClient, IOptions<ElasticSettings> config,ISearchQueryBuilder searchQueryBuilder)
        {
            _logger = logger;
            _elasticClient = elasticClient;
            _searchQueryBuilder = searchQueryBuilder;
            _config = config;
        }


        [Route("api/search")]
        [HttpPost]
        [ProducesResponseType(typeof(HttpResponseMessage), 200)]
        public async Task<IActionResult> HandleAsync([FromBody]SearchOption searchOption)
        {
            try
            {
                var queryToBeUsed = _searchQueryBuilder.GetQuery(searchOption);

                var result = await _elasticClient.HttpClientPost(new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_config.Value.Url}/{_config.Value.IndexName}/{_config.Value.IndexType}/_search"),
                    Content = new StringContent(queryToBeUsed, Encoding.UTF8, "application/json")
                }, CancellationToken.None);

                var responseContent = await result.Content.ReadAsStringAsync();

                var resultData = responseContent.ToListOf<SqlViewProduct>();

                var totalResultCount = responseContent.TotalCount();

                var products = resultData
                        .Select(x => ProductThumbnail.FromSqlViewProduct(x))
                        .ToList();
                var searchResult = new SearchResult
                {
                    Products = products,
                    TotalProduct = totalResultCount
                };
                return Ok(searchResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in request.{ex}");
                return BadRequest();
            }
        }
    }
}

