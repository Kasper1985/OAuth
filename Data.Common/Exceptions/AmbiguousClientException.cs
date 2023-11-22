using System;

namespace Data.Common.Exceptions
{
    public class AmbiguousClientException : Exception
    {
        private readonly int clientId;

        public AmbiguousClientException(): base() { }
        public AmbiguousClientException(string message) : base(message) { }
        public AmbiguousClientException(string message, Exception innerException) : base(message, innerException) { }

        public AmbiguousClientException(int clientId) : base() => this.clientId = clientId;
        public AmbiguousClientException(int clientId, string message) : base(message) => this.clientId = clientId;
        public AmbiguousClientException(int clientId, string message, Exception innerException) : base(message, innerException) => this.clientId = clientId;
    }
}
