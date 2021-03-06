﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Rest
{
    public class ClientError : IClientError
    {
        /// <summary>
        /// Gets or sets error code in error entity.
        /// </summary>
        public string Code
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public Guid RequestId { get; set; }
    }
}
