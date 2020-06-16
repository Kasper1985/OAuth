using System;

namespace Data.Common.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        private readonly int clientId;

        public ClientNotFoundException() : base() { }
        public ClientNotFoundException(string message) : base(message) { }
        public ClientNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        public ClientNotFoundException(int clientId) : base() => this.clientId = clientId;
        public ClientNotFoundException(int clientId, string message) : base(message) => this.clientId = clientId;
        public ClientNotFoundException(int clientId, string message, Exception innerException) : base(message, innerException) => this.clientId = clientId;
    }
}
