using System.Net;

namespace Frontend.Rest
{
    public interface IClientException
    {
        ClientError Error { get; set; }
        HttpStatusCode HttpStatus { get; }
    }
}