using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace SimplCommerce.SearchApi.HttpManager
{
    public static class HttpClientAuthExtensions
    {
        public static HttpClient WithUserInfo(this HttpClient client, string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                return client;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", (object)user, (object)pass))));
            return client;
        }
    }
}
