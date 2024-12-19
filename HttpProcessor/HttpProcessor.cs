using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HttpProcessor.Exceptions;
using Microsoft.Extensions.Logging;

namespace HttpProcessor
{

    public class HttpProcessor
    {

        private readonly HttpClient _httpClient;
        private readonly HttpProcessorSettings _processorSettings;
        private readonly ILogger _logger;

        public HttpProcessor(HttpProcessorSettings processorSettings)
        {
            _httpClient = new HttpClient();
            _processorSettings = processorSettings;
            Settings = processorSettings;
            _logger = null;
        }

        public HttpProcessor(HttpProcessorSettings processorSettings, ILogger logger) : this(processorSettings)
        {
            _logger = logger;
        }

        public delegate Task<bool> AuthenticateDelegate(HttpProcessor httpProcessor, HttpClient httpClient, ILogger logger);

        public delegate void NonSuccessfulStatusCodeReceivedDelegate(HttpStatusCode statusCode, string receivedBody, HttpMethod requestMethod, string requestUri, ILogger logger);

        public HttpProcessorSettings Settings { get; private set; }

        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Delegate to be called when authentication is needed. It's called either manually via calling Authenticate() on the object of this class or automatically when the server responds with 401 Unauthorized status code. It undergoes same retry conditions as any other request.
        /// If authentication was successful, it should return true, otherwise false.
        /// </summary>
        public AuthenticateDelegate AuthenticateMethod { get; set; } = (httpProcessor, httpClient, logger) => Task.FromResult(true);

        /// <summary>
        /// Delegate to be called when non-successful status code was received, apart from 401 Unauthorized. 
        /// </summary>
        public NonSuccessfulStatusCodeReceivedDelegate NonSuccessfulStatusCodeReceived { get; set; } =
            (statusCode, receivedBody, requestMethod, requestUri, logger) => throw new HttpProcessorGeneralException(
                $"Non successful status code received. Request {requestMethod} {requestUri} Status code: {statusCode}, body: \"{receivedBody}\".");

        private string GetAbsoluteUrl(string endpoint)
        {
            var baseUrl = _processorSettings.BaseUrl;

            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            if (endpoint.StartsWith("/"))
                endpoint = endpoint.Substring(1);

            return baseUrl + endpoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="HttpProcessorUnauthorizedException"></exception>
        /// <exception cref="HttpProcessorGeneralException"></exception>
        private async Task<T> SendRequest<T>(string endpoint, object bodyObject,
            Dictionary<string, string> customHeaders, HttpMethod method)
        {
            var requestUri = GetAbsoluteUrl(endpoint);

            var timeout = _processorSettings.Timeout;
            var retryCount = _processorSettings.RetryCount;
            var timeBetweenRetries = _processorSettings.TimeBetweenRetries;

            var currentRetry = 0;

            var responseObject = default(T);

            do
            {
                var requestMessage = new HttpRequestMessage(method, requestUri);

                //Add BODY
                if (bodyObject != null)
                    requestMessage.Content = new StringContent(JsonSerializer.Serialize(bodyObject, JsonSerializerOptions.Web), Encoding.UTF8,
                        "application/json");

                //Add HEADERS
                if (customHeaders != null)
                    foreach (var customHeader in customHeaders)
                    {
                        requestMessage.Headers.Add(customHeader.Key, customHeader.Value);
                    }

                try
                {
                    var cancellationTokenSource = new CancellationTokenSource(timeout * 1000);

                    var response = await _httpClient.SendAsync(requestMessage, cancellationTokenSource.Token);

                    if (!response.IsSuccessStatusCode)
                    {

                        //Re-authenticating on token expiration
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var currentAuthenticationRetry = 0;
                            do
                            {
                                await Authenticate();
                            } while (currentAuthenticationRetry++ < retryCount);

                            if (currentAuthenticationRetry == retryCount)
                                throw new HttpProcessorUnauthorizedException(
                                    $"Failed to authenticate after {retryCount} retries.");
                        }

                        NonSuccessfulStatusCodeReceived(response.StatusCode, await response.Content.ReadAsStringAsync(),
                            method, endpoint, _logger);
                    }

                    responseObject = JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions.Web);

                    break;
                }
                catch (Exception ex) when
                    (ex.GetType() != typeof(HttpProcessorGeneralException))
                {
                    //just retry if the exception is not HttpProcessorGeneralException or HttpProcessorUnauthorizedException
                }

                await Task.Delay(timeBetweenRetries * 1000);
            } while (currentRetry++ < retryCount);

            if (retryCount == currentRetry)
                throw new HttpProcessorGeneralException(
                    $"Failed to get data from the server after {retryCount} retries.");

            return responseObject;
        }

        /// <summary>
        /// This method is automatically called inside <see cref="HttpProcessor"/> whenever the response from the server is 401 Unauthorized.
        /// It should be called manually before first request to a protected resource is made. 
        /// If it is not called manually, the first request to the protected resource will first call the resource, when it gets 401 as response, it will be called automatically.
        /// </summary>
        /// <returns>Result of the <see cref="AuthenticateMethod"/> delegate call.</returns>
        public async Task<bool> Authenticate()
        {
            return await AuthenticateMethod(this, _httpClient, _logger);
        }

        /// <summary>
        /// Sends GET request to the server and returns the response of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            return await SendRequest<T>(endpoint, null, customHeaders, HttpMethod.Get);
        }

        /// <summary>
        /// Sends POST request to the server and returns the response of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            return await SendRequest<T>(endpoint, bodyObject, customHeaders, HttpMethod.Post);
        }

        /// <summary>
        /// Sends PUT request to the server and returns the response of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<T> PutAsync<T>(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            return await SendRequest<T>(endpoint, bodyObject, customHeaders, HttpMethod.Put);
        }

        /// <summary>
        /// Sends PATCH request to the server and returns the response of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<T> PatchAsync<T>(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            return await SendRequest<T>(endpoint, bodyObject, customHeaders, new HttpMethod("PATCH"));
        }

        /// <summary>
        /// Sends DELETE request to the server and returns the response of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="bodyObject"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<T> DeleteAsync<T>(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            return await SendRequest<T>(endpoint, bodyObject, customHeaders, HttpMethod.Delete);
        }



        public async Task GetAsync(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            await GetAsync<object>(endpoint, null, customHeaders);
        }

        public async Task PostAsync(string endpoint, object bodyObject,
            Dictionary<string, string> customHeaders = null)
        {
            await PostAsync<object>(endpoint, bodyObject, customHeaders);
        }

        public async Task PutAsync(string endpoint, object bodyObject,
            Dictionary<string, string> customHeaders = null)
        {
            await PutAsync<object>(endpoint, bodyObject, customHeaders);
        }

        public async Task PatchAsync(string endpoint, object bodyObject,
            Dictionary<string, string> customHeaders = null)
        {
            await PatchAsync<object>(endpoint, bodyObject, customHeaders);
        }

        public async Task DeleteAsync(string endpoint, object bodyObject = null,
            Dictionary<string, string> customHeaders = null)
        {
            await DeleteAsync<object>(endpoint, bodyObject, customHeaders);
        }

    }
}