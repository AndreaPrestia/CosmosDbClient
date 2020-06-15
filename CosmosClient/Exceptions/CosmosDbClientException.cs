using System;

namespace CosmosDbClient.Exceptions
{
    public class CosmosDbClientException : Exception
    {
        public CosmosDbClientException(string message) : base(message)
        {
        }

        public CosmosDbClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CosmosDbClientException()
        {
        }
    }
}
