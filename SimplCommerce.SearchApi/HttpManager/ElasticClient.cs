using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SimplCommerce.SearchApi.HttpManager
{
    public class ElasticClient: IElasticClient
    {
        private readonly HttpClient _httpClient;
        public ElasticClient(HttpClient client)
        {
            _httpClient = client;
        }
        public async Task<HttpResponseMessage> HttpClientPost(HttpRequestMessage message, CancellationToken ct)
        {
            var response = await _httpClient.PostAsync(message.RequestUri, message.Content);
            return response;
        }

        public async Task<HttpResponseMessage> HttpClientGet(string url, CancellationToken ct)
        {

            var response = await _httpClient.GetAsync(url);
            return response;
        }

    }
}
