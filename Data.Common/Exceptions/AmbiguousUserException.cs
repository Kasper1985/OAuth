using System;

namespace Data.Common.Exceptions
{
    public class AmbiguousUserException : Exception
    {
        private readonly int userId;

        public AmbiguousUserException() : base() { }
        public AmbiguousUserException(string message) : base(message) { }
        public AmbiguousUserException(string message, Exception innerException) : base(message, innerException) { }

        public AmbiguousUserException(int userId) : base() => this.userId = userId;
        public AmbiguousUserException(int userId, string message) : base(message) => this.userId = userId;
        public AmbiguousUserException(int userId, string message, Exception innerException) : base(message, innerException) => this.userId = userId;
    }
}
