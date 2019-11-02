using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimplCommerce.SearchApi.HttpManager;
using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimplCommerce.SearchApi.Controllers
{
    [ApiController]
    public class SearchController : Controller
    {
        private readonly ILogger _logger;
        private readonly IElasticClient _elasticClient;
        private readonly ISearchQueryBuilder _searchQueryBuilder;
        private readonly ISearchViewDetailQueryBuilder _searchViewDetailQueryBuilder;
        private readonly IOptions<ElasticSettings> _config;

        public SearchController(ILogger<SearchController> logger, IElasticClient elasticClient, IOptions<ElasticSettings> config,ISearchQueryBuilder searchQueryBuilder, ISearchViewDetailQueryBuilder searchViewDetailQueryBuilder)
        {
            _logger = logger;
            _elasticClient = elasticClient;
            _searchQueryBuilder = searchQueryBuilder;
            _searchViewDetailQueryBuilder = searchViewDetailQueryBuilder;
            _config = config;
        }


        [Route("api/search")]
        [HttpPost]
        [ProducesResponseType(typeof(SearchResult), 200)]
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

        [Route("api/search/{id}")]
        [HttpPost]
        [ProducesResponseType(typeof(ProductDetail), 200)]
        public async Task<IActionResult> HandleViewDetailAsync(long id)
        {
            try
            {
                var queryToBeUsed = _searchViewDetailQueryBuilder.GetQuery(id);

                var result = await _elasticClient.HttpClientPost(new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_config.Value.Url}/{_config.Value.IndexName}/{_config.Value.IndexType}/_search"),
                    Content = new StringContent(queryToBeUsed, Encoding.UTF8, "application/json")
                }, CancellationToken.None);

                var responseContent = await result.Content.ReadAsStringAsync();

                var resultData = responseContent.ToListOf<ProductDetail>();

                var productDetail = resultData.FirstOrDefault();

                //Required to set the following details

                if (productDetail != null)
                {

                    productDetail.Attributes = new List<ProductDetailAttribute>(); //Get Attribute Details
                    productDetail.Categories =new List<ProductDetailCategory>(); //Get Category Details
                    productDetail.Countries =new List<SelectListItem>();
                    productDetail.CalculatedProductPrice = null; // Call pricing service
                    productDetail.Variations = null;// Get variation details
                    productDetail.OptionDisplayValues = null;//Get option details
                    productDetail.Images = null;//Call image service
                    productDetail.Countries= new List<SelectListItem>(); //set countries value
                }


                return Ok(productDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in request.{ex}");
                return BadRequest();
            }
        }

    }
}

