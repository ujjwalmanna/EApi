using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimplCommerce.SearchApi.HttpManager
{
    public interface IElasticClient
    {
        Task<HttpResponseMessage> HttpClientPost(HttpRequestMessage message, CancellationToken ct);
        Task<HttpResponseMessage> HttpClientGet(string url, CancellationToken ct);
    }
}
