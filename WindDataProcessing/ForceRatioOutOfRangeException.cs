using System;
using System.Runtime.Serialization;

namespace WindDataProcessing
{
    [Serializable]
    internal class ForceRatioOutOfRangeException : Exception
    {
        public ForceRatioOutOfRangeException()
        {
        }

        public ForceRatioOutOfRangeException(string message) : base(message)
        {
        }

        public ForceRatioOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ForceRatioOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}