using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Frontend.Rest
{
    public class ClientException : Exception, IClientException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        public ClientException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        /// <param name="message">The corresponding error message.</param>
        public ClientException(string message)
            : base(message)
        {
            Error = new ClientError()
            {
                Code = HttpStatusCode.InternalServerError.ToString(),
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        /// <param name="message">The corresponding error message.</param>
        /// <param name="httpStatus">The Http Status code.</param>
        public ClientException(string message, HttpStatusCode httpStatus)
            : base(message)
        {
            HttpStatus = httpStatus;

            Error = new ClientError()
            {
                Code = HttpStatus.ToString(),
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        /// <param name="message">The corresponding error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ClientException(string message, Exception innerException)
            : base(message, innerException)
        {
            Error = new ClientError()
            {
                Code = HttpStatusCode.InternalServerError.ToString(),
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        /// <param name="message">The corresponding error message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="httpStatus">The http status.</param>
        /// <param name="innerException">The inner exception.</param>
        public ClientException(string message, string errorCode, HttpStatusCode httpStatus, Exception innerException)
            : base(message, innerException)
        {
            HttpStatus = httpStatus;

            Error = new ClientError()
            {
                Code = errorCode,
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        /// <param name="error">The error entity.</param>
        /// <param name="httpStatus">The http status.</param>
        public ClientException(ClientError error, HttpStatusCode httpStatus)
        {
            Error = error;
            HttpStatus = httpStatus;
        }

        public ClientException(HttpStatusCode httpStatus, ClientError error)
        {
            HttpStatus = httpStatus;
            Error = error;
        }

        protected ClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets http status of http response.
        /// </summary>
        public HttpStatusCode HttpStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the httpError message.
        /// </summary>
        public ClientError Error { get; set; }

        /// <summary>
        /// Create Client Exception of Bad Request.
        /// </summary>
        /// <param name="message">The corresponding error message.</param>
        /// <returns>Client Exception Instance.</returns>
        public static ClientException BadRequest(string message)
        {
            return new ClientException(
                         new ClientError()
                         {
                             Code = ((int)HttpStatusCode.BadRequest).ToString(),
                             Message = message
                         },
                         HttpStatusCode.BadRequest);
        }
    }
}
