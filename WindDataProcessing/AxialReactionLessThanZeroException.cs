using System;
using System.Runtime.Serialization;

namespace WindDataProcessing
{
    [Serializable]
    internal class AxialReactionLessThanZeroException : Exception
    {
        public AxialReactionLessThanZeroException()
        {
        }

        public AxialReactionLessThanZeroException(string message) : base(message)
        {
        }

        public AxialReactionLessThanZeroException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AxialReactionLessThanZeroException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}