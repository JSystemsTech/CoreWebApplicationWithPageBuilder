using AuthNAuthZ.TokenProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharedServices.ApiClient
{
    public class UrlHelpers
    {
        public static string AddQueryString<TParams>(string url, TParams parameters)
        {
            var queryParams = parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(parameters, null).ToString());
            return AddQueryString(url, queryParams);
        }
        public static string AddQueryString(string url, string name, object value)
            => AddQueryString(url, new Dictionary<string, string> { { name, value.ToString() } });
        public static string AddQueryString(string url, IDictionary<string, string> queryParams)
        {
            if (queryParams.Count() == 0)
            {
                return url;
            }
            string[] uri = url.Split('?');
            string queryStr = uri.Count() > 1 ? uri[1] : null;
            string queryToAppend = string.Join("&", queryParams.Select(item => $"{item.Key}={HttpUtility.UrlEncode(item.Value)}"));
            return $"{uri[0]}?{(string.IsNullOrWhiteSpace(queryStr) ? queryToAppend : $"{queryStr}&{queryToAppend}")}";
        }
    }
    public static class HttpClientExtensions
    {
        private static StringContent BuildJsonBody<T>(T data)
            => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        private static StringContent EmptyBody => new StringContent(string.Empty, Encoding.UTF8, "application/json");
        
        private static async Task<TData> ParseResponseAsync<TData>(HttpResponseMessage response)
        {
            TData data = default;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                data = string.IsNullOrWhiteSpace(content) ? data : JsonConvert.DeserializeObject<TData>(content);
            }
            return data;
        }
        public static HttpClient WithAuthHeader(this HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpClient;
        }
        
        public static async Task<TData> PostDataAsync<TBody, TData>(this HttpClient httpClient, string url, TBody body)
        {
            using (var response = await httpClient.PostAsync(url, BuildJsonBody(body)))
            {
                return await ParseResponseAsync<TData>(response);
            }
        }
        public static async Task<TData> PostDataAsync<TData>(this HttpClient httpClient, string url)
        {
            using (var response = await httpClient.PostAsync(url, EmptyBody))
            {
                return await ParseResponseAsync<TData>(response);
            }
        }
        public static async Task<TData> GetDataAsync<TParams, TData>(this HttpClient httpClient, string url, TParams parameters)
        {
            using (var response = await httpClient.GetAsync(UrlHelpers.AddQueryString(url, parameters)))
            {
                return await ParseResponseAsync<TData>(response);
            }
        }
        public static async Task<TData> GetDataAsync<TData>(this HttpClient httpClient, string url)
        {
            using (var response = await httpClient.GetAsync(url))
            {
                return await ParseResponseAsync<TData>(response);
            }
        }
    }
    public abstract class ApiBase
    {

        private HttpClient HttpClient { get; set; }
        public ApiBase(HttpClient httpClient){
            HttpClient = httpClient;
            Init();
        }
        public ApiBase(string apiBaseUrl, HttpClient httpClient)
        {
            HttpClient = httpClient;
            HttpClient.BaseAddress = new Uri(apiBaseUrl);
            Init();
        }
        private void Init()
        {
            if (!HttpClient.BaseAddress.AbsoluteUri.EndsWith("/"))
            {
                HttpClient.BaseAddress = new Uri($"{HttpClient.BaseAddress.AbsoluteUri}/");
            }
        }
        protected async Task<TData> PostAsync<TBody, TData>(string endpoint, string token, TBody body)
        => await HttpClient.WithAuthHeader(token).PostDataAsync<TBody, TData>(endpoint, body);
        protected async Task<TData> PostAsync<TData>(string endpoint, string token)
        => await HttpClient.WithAuthHeader(token).PostDataAsync<TData>(endpoint);

        protected async Task<TData> GetAsync<TParams, TData>(string endpoint, string token, TParams parameters)
        => await HttpClient.WithAuthHeader(token).GetDataAsync<TParams, TData>(endpoint, parameters);
        protected async Task<TData> GetAsync<TData>(string endpoint, string token)
        => await HttpClient.WithAuthHeader(token).GetDataAsync<TData>(endpoint);


        #region Synchronous Wrappers
        protected TData Post<TBody, TData>(string endpoint, string token, TBody body)
            => PostAsync<TBody, TData>(endpoint, token, body).GetAwaiter().GetResult();
        protected TData Post<TData>(string endpoint, string token)
            => PostAsync<TData>(endpoint, token).GetAwaiter().GetResult();
        protected TData Get<TParams, TData>(string endpoint, string token, TParams parameters)
            => GetAsync<TParams, TData>(endpoint, token, parameters).GetAwaiter().GetResult();
        protected TData Get<TData>(string endpoint, string token)
            => GetAsync<TData>(endpoint, token).GetAwaiter().GetResult();
        #endregion
    }
    public interface IAuthenticationProviderApi
    {
        bool IsValid(string token);
        string Update(string token, IEnumerable<TokenClaim> claims);
        DateTime? GetExpirationDate(string token);
        IEnumerable<TokenClaim> GetClaims(string token);
    }
    public sealed class AuthenticationProviderApi : ApiBase, IAuthenticationProviderApi
    {
        public AuthenticationProviderApi(HttpClient httpClient) : base(httpClient) { }
        public AuthenticationProviderApi(string apiBaseUrl, HttpClient httpClient) : base(apiBaseUrl, httpClient) { }

        public bool IsValid(string token) => Post<dynamic, bool>("IsValid", token, new { token });
        public string Update(string token, IEnumerable<TokenClaim> claims) => Post<dynamic, string>("Update", token, new { token, claims });
        public DateTime? GetExpirationDate(string token) => Post<dynamic, DateTime?>("GetExpirationDate", token, new { token });
        public IEnumerable<TokenClaim> GetClaims(string token) => Post<dynamic, TokenClaim[]>("GetClaims", token, new { token });
    }

}
