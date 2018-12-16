using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Frontend.Rest
{
    public interface IServiceClient
    {
        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="request">The request.</param>
        /// <param name="setHeadersCallback">The set headers callback.</param>
        /// <returns></returns>
        Task<TResponse> GetAsync<TResponse>(string method, WebRequest request, Action<WebRequest> setHeadersCallback = null);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="request">The request.</param>
        /// <param name="setHeadersCallback">The set headers callback.</param>
        /// <returns></returns>
        Task<TResponse> SendAsync<TRequest, TResponse>(string method, TRequest requestBody, WebRequest request, Action<WebRequest> setHeadersCallback = null);
    }
}
