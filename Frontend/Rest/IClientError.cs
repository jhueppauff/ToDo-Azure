using System;

namespace Frontend.Rest
{
    public interface IClientError
    {
        string Code { get; set; }
        string Message { get; set; }
        Guid RequestId { get; set; }
    }
}