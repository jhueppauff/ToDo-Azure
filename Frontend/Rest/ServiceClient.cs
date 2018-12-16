using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Frontend.Rest
{
    public class ServiceClient : IServiceClient
    {
        /// <summary>
        /// Default timeout for calls
        /// </summary>
        private const int DEFAULT_TIMEOUT = 2 * 60 * 1000; // 2 minutes timeout

        /// <summary>
        /// Default timeout for calls, overridable by subclasses
        /// </summary>
        protected virtual int DefaultTimeout => DEFAULT_TIMEOUT;

        /// <summary>
        /// The default resolver
        /// </summary>
        private readonly CamelCasePropertyNamesContractResolver _defaultResolver = new CamelCasePropertyNamesContractResolver();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClient"/> class.
        /// </summary>
        public ServiceClient()
        {
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="request">The request.</param>
        /// <param name="setHeadersCallback">The set headers callback.</param>
        /// <returns>
        /// The response object.
        /// </returns>
        public async Task<TResponse> GetAsync<TResponse>(string method, WebRequest request, Action<WebRequest> setHeadersCallback = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            try
            {
                request.Method = method;
                if (null == setHeadersCallback)
                {
                    SetCommonHeaders(request);
                }
                else
                {
                    setHeadersCallback(request);
                }

                var getResponseAsync = Task.Factory.FromAsync<WebResponse>(
                    request.BeginGetResponse,
                    request.EndGetResponse,
                    null);

                await Task.WhenAny(getResponseAsync, Task.Delay(DefaultTimeout)).ConfigureAwait(false);

                //Abort request if timeout has expired
                if (!getResponseAsync.IsCompleted)
                {
                    request.Abort();
                }

                return ProcessAsyncResponse<TResponse>(getResponseAsync.Result as HttpWebResponse);
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    HandleException(e);
                    return true;
                });
                return default(TResponse);
            }
            catch (Exception e)
            {
                HandleException(e);
                return default(TResponse);
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="request">The request.</param>
        /// <param name="setHeadersCallback">The set headers callback.</param>
        /// <returns>The response object.</returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public async Task<TResponse> SendAsync<TRequest, TResponse>(string method, TRequest requestBody, WebRequest request, Action<WebRequest> setHeadersCallback = null)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                request.Method = method;
                if (null == setHeadersCallback)
                {
                    SetCommonHeaders(request);
                }
                else
                {
                    setHeadersCallback(request);
                }

                if (requestBody is Stream)
                {
                    request.ContentType = "application/octet-stream";
                }

                var asyncState = new WebRequestAsyncState()
                {
                    RequestBytes = SerializeRequestBody(requestBody),
                    WebRequest = (HttpWebRequest)request,
                };

                var continueRequestAsyncState = await Task.Factory.FromAsync<Stream>(
                                                    asyncState.WebRequest.BeginGetRequestStream,
                                                    asyncState.WebRequest.EndGetRequestStream,
                                                    asyncState,
                                                    TaskCreationOptions.None).ContinueWith<WebRequestAsyncState>(
                                                       task =>
                                                       {
                                                           var requestAsyncState = (WebRequestAsyncState)task.AsyncState;
                                                           if (requestBody != null)
                                                           {
                                                               using (var requestStream = task.Result)
                                                               {
                                                                   if (requestBody is Stream)
                                                                   {
                                                                       (requestBody as Stream).CopyTo(requestStream);
                                                                   }
                                                                   else
                                                                   {
                                                                       requestStream.Write(requestAsyncState.RequestBytes, 0, requestAsyncState.RequestBytes.Length);
                                                                   }
                                                               }
                                                           }

                                                           return requestAsyncState;
                                                       }).ConfigureAwait(false);

                var continueWebRequest = continueRequestAsyncState.WebRequest;
                var getResponseAsync = Task.Factory.FromAsync<WebResponse>(
                    continueWebRequest.BeginGetResponse,
                    continueWebRequest.EndGetResponse,
                    continueRequestAsyncState);

                await Task.WhenAny(getResponseAsync, Task.Delay(DefaultTimeout)).ConfigureAwait(false);

                //Abort request if timeout has expired
                if (!getResponseAsync.IsCompleted)
                {
                    request.Abort();
                }

                return ProcessAsyncResponse<TResponse>(getResponseAsync.Result as HttpWebResponse);
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    HandleException(e);
                    return true;
                });
                return default(TResponse);
            }
            catch (Exception e)
            {
                HandleException(e);
                return default(TResponse);
            }
        }

        /// <summary>
        /// Processes the asynchronous response.
        /// </summary>
        /// <typeparam name="T">Type of response.</typeparam>
        /// <param name="webResponse">The web response.</param>
        /// <returns>The response.</returns>
        private T ProcessAsyncResponse<T>(HttpWebResponse webResponse)
        {
            using (webResponse)
            {
                if (webResponse.StatusCode == HttpStatusCode.OK ||
                    webResponse.StatusCode == HttpStatusCode.Accepted ||
                    webResponse.StatusCode == HttpStatusCode.Created)
                {
                    if (webResponse.ContentLength != 0)
                    {
                        using (var stream = webResponse.GetResponseStream())
                        {
                            if (stream != null)
                            {
                                if (webResponse.ContentType == "image/jpeg" ||
                                    webResponse.ContentType == "image/png")
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        return (T)(object)ms.ToArray();
                                    }
                                }
                                else
                                {
                                    string message = string.Empty;
                                    using (StreamReader reader = new StreamReader(stream))
                                    {
                                        message = reader.ReadToEnd();
                                    }

                                    JsonSerializerSettings settings = new JsonSerializerSettings
                                    {
                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                        NullValueHandling = NullValueHandling.Ignore,
                                        ContractResolver = _defaultResolver
                                    };

                                    return JsonConvert.DeserializeObject<T>(message, settings);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (webResponse.Headers.AllKeys.Contains("Operation-Location"))
                        {
                            string message = string.Format("{{Url: \"{0}\"}}", webResponse.Headers["Operation-Location"]);

                            JsonSerializerSettings settings = new JsonSerializerSettings
                            {
                                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = _defaultResolver
                            };

                            return JsonConvert.DeserializeObject<T>(message, settings);
                        }
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Set request content type.
        /// </summary>
        /// <param name="request">Web request object.</param>
        private void SetCommonHeaders(WebRequest request)
        {
            request.ContentType = "application/json";
        }

        /// <summary>
        /// Serialize the request body to byte array.
        /// </summary>
        /// <typeparam name="T">Type of request object.</typeparam>
        /// <param name="requestBody">Strong typed request object.</param>
        /// <returns>Byte array.</returns>
        private byte[] SerializeRequestBody<T>(T requestBody)
        {
            if (requestBody == null || requestBody is Stream)
            {
                return new byte[0];
            }
            else
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    ContractResolver = _defaultResolver
                };

                return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody, settings));
            }
        }

        /// <summary>
        /// Process the exception happened on rest call.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        private void HandleException(Exception exception)
        {
            if (exception is WebException webException && webException.Response != null && webException.Response.ContentType.ToLower().Contains("application/json"))
            {
                Stream stream = null;

                try
                {
                    stream = webException.Response.GetResponseStream();
                    if (stream != null)
                    {
                        string errorObjectString;
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            stream = null;
                            errorObjectString = reader.ReadToEnd();
                        }

                        ClientError errorCollection = JsonConvert.DeserializeObject<ClientError>(errorObjectString);

                        // HandwritingOcr error message use the latest format, so add the logic to handle this issue.
                        if (errorCollection.Code == null && errorCollection.Message == null)
                        {
                            var errorType = new { Error = new ClientError() };
                            var errorObj = JsonConvert.DeserializeAnonymousType(errorObjectString, errorType);
                            errorCollection = errorObj.Error;
                        }

                        if (errorCollection != null)
                        {
                            throw new ClientException
                            {
                                Error = errorCollection,
                            };
                        }
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
            }

            throw exception;
        }
    }
}
