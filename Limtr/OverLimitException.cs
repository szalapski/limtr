using System;

namespace Limtr {
    /// <summary>
    /// Thrown when a operation is over the allowed rate limit unexpectedly.
    /// </summary>
    public class OverLimitException : InvalidOperationException{
        /// <summary>
        /// Creates a new OverLimitException.
        /// </summary>
        public OverLimitException() : base() { }
        /// <summary>
        /// Creates a new OverLimitException with a message.
        /// </summary>
        public OverLimitException(string message) : base(message) { }
        /// <summary>
        /// Creates a new OverLimitException with a message and an inner exception.
        /// </summary>
        public OverLimitException(string message, Exception innerException) : base(message, innerException) { }
    }
}
