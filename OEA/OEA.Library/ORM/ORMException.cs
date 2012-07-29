using System;
using System.Runtime.Serialization;

namespace OEA.ORM
{
    [Serializable]
    public class ORMException : Exception
    {
        public ORMException(string message) : base(message) { }

        public ORMException(string message, Exception cause) : base(message, cause) { }

        public ORMException(Exception cause) : base(cause.Message, cause) { }

        protected ORMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}