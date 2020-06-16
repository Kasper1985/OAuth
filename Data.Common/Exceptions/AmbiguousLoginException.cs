using System;

namespace Data.Common.Exceptions
{
    public class AmbiguousLoginException : Exception
    {
        private readonly int userId;
        private readonly string login;

        public AmbiguousLoginException() : base() { }
        public AmbiguousLoginException(string message) : base(message) { }
        public AmbiguousLoginException(string message, Exception innerException) : base(message, innerException) { }

        public AmbiguousLoginException(int userId) : base() => this.userId = userId;
        public AmbiguousLoginException(int userId, string message) : base(message) => this.userId = userId;
        public AmbiguousLoginException(int userId, string message, Exception innerException) : base(message, innerException) => this.userId = userId;

        public AmbiguousLoginException(string login, int userId) : base() { this.login = login; this.userId = userId; }
        public AmbiguousLoginException(string login, string message) : base(message) => this.login = login;
        public AmbiguousLoginException(string login, string message, Exception innerException) : base(message, innerException) => this.login = login;
    }
}
