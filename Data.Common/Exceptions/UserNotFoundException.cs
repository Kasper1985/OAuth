using System;

namespace Data.Common.Exceptions
{
    public class UserNotFoundException : Exception
    {
        private readonly int userId;

        public UserNotFoundException() : base() { }
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        public UserNotFoundException(int userId) : base() => this.userId = userId;
        public UserNotFoundException(int userId, string message) : base(message) => this.userId = userId;
        public UserNotFoundException(int userId, string message, Exception innerException) : base(message, innerException) => this.userId = userId;
    }
}
