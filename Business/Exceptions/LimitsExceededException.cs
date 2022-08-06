using System.Runtime.Serialization;

namespace Business.Exceptions
{
    [Serializable]
    public class LimitsExceededException : InvalidOperationException
    {
        public LimitsExceededException() : base() { }

        public LimitsExceededException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public LimitsExceededException(string? limitName) : base($"You cannot exceed the {limitName} tasks limit") { }

        public LimitsExceededException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
