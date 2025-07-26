using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using OneTon.Logging;
using Sirenix.Utilities;

namespace OneTon.Networking
{
    public static class HttpRequests
    {
        private static LogService logger = LogService.GetStatic(typeof(HttpRequests));
        public const int DefaultTimeout = 30;
        public static async Task<HttpResponseMessage> Put(string url, HttpContent encodedContent, int timeoutSeconds, Dictionary<string, string> headers = null)
        {
            return await Request(HttpMethod.Put, url, encodedContent, timeoutSeconds, headers);
        }

        public static async Task<HttpResponseMessage> Post(string url, HttpContent encodedContent, int timeoutSeconds, Dictionary<string, string> headers = null)
        {
            return await Request(HttpMethod.Post, url, encodedContent, timeoutSeconds, headers);
        }

        public static async Task<HttpResponseMessage> Get(string url, int timeoutSeconds, Dictionary<string, string> headers = null)
        {
            return await Request(HttpMethod.Get, url, null, timeoutSeconds, headers);
        }

        public static async Task<HttpResponseMessage> Delete(string url, int timeoutSeconds, Dictionary<string, string> headers = null)
        {
            return await Request(HttpMethod.Delete, url, null, timeoutSeconds, headers);
        }


        private static async Task<HttpResponseMessage> Request(HttpMethod method, string url, HttpContent encodedContent, int timeoutSeconds, Dictionary<string, string> headers = null)
        {
            logger.Debug($"{method.Method} {url}");

            // Create HttpClient instance
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            headers?.Keys.ForEach(key =>
                {
                    client.DefaultRequestHeaders.Add(key, headers[key]);
                }
            );

            HttpResponseMessage response = null;
            try
            {
                // Send request
                if (method == HttpMethod.Put)
                {
                    response = await client.PutAsync(url, encodedContent);
                }
                else if (method == HttpMethod.Post)
                {
                    response = await client.PostAsync(url, encodedContent);
                }
                else if (method == HttpMethod.Get)
                {
                    response = await client.GetAsync(url);
                }
                else if (method == HttpMethod.Delete)
                {
                    response = await client.DeleteAsync(url);
                }
                else
                {
                    throw new Exception($"HttpMethod.{method} is not implemented.");
                }

                logger.Debug($"response.StatusCode: {response.StatusCode}");
                logger.Debug($"response.Content: {await response.Content.ReadAsStringAsync()}");
            }
            catch (HttpRequestException e)
            {
                logger.Error("HTTP Request Exception: " + e.Message);
            }

            return response;
        }
    }
}