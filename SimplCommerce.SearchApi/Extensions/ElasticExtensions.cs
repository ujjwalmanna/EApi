using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimplCommerce.SearchApi.Extensions
{
    public static class ElasticExtensions
    {
        public const string SourceField = "_source";
        public const string ValueField = "value";
        public const string HitsField = "hits";
        public const string TotalField = "total";
        public static T To<T>(this JObject json) => (json[SourceField] ?? json).ToObject<T>();
        public static T To<T>(this string jsonString) => JObject.Parse(jsonString).To<T>();
        public static T To<T>(this Task<string> stringTask) => stringTask.Result.To<T>();
        public static T To<T>(this HttpContent content) => content.ReadAsStringAsync().To<T>();
        public static T To<T>(this HttpResponseMessage response) => response.Content.To<T>();
        public static T To<T>(this Task<HttpResponseMessage> responseTask) => responseTask.Result.To<T>();
        public static List<T> ToListOf<T>(this JObject json) => ((JArray)json[HitsField][HitsField]).Select(hit => ((JObject)hit).To<T>()).ToList();
        public static List<T> ToListOf<T>(this string jsonString) => JObject.Parse(jsonString).ToListOf<T>();
        public static List<T> ToListOf<T>(this Task<string> stringTask) => stringTask.Result.ToListOf<T>();
        public static List<T> ToListOf<T>(this HttpContent content) => content.ReadAsStringAsync().ToListOf<T>();
        public static List<T> ToListOf<T>(this HttpResponseMessage response) => response.Content.ToListOf<T>();
        public static List<T> ToListOf<T>(this Task<HttpResponseMessage> responseTask) => responseTask.Result.ToListOf<T>();
        public static int TotalCount(this string jsonString)=> JObject.Parse(jsonString).TotalCount();
        public static int TotalCount(this JObject json) => (json[HitsField][TotalField][ValueField]).ToObject<int>();
    }
}
