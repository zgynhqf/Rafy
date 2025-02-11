﻿using System;
using System.Runtime.Serialization.Formatters.Binary;

namespace Rafy.Serialization
{
    /// <summary>
    /// Wraps the <see cref="BinaryFormatter"/>
    /// in the 
    /// <see cref="ISerializationFormatter"/>
    /// interface so it can be used in a standardized
    /// manner.
    /// </summary>
    public class BinaryFormatterWrapper : ISerializationFormatter
    {
        private BinaryFormatter _formatter = new BinaryFormatter
        {
            SurrogateSelector = new CustomSurrogateSelector()
        };

        #region ISerializationFormatter Members

        /// <summary>
        /// Converts a serialization stream into an
        /// object graph.
        /// </summary>
        /// <param name="serializationStream">
        /// Byte stream containing the serialized data.</param>
        /// <returns>A deserialized object graph.</returns>
        public object Deserialize(System.IO.Stream serializationStream)
        {
            return _formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// Converts an object graph into a byte stream.
        /// </summary>
        /// <param name="serializationStream">
        /// Stream that will contain the the serialized data.</param>
        /// <param name="graph">Object graph to be serialized.</param>
        public void Serialize(System.IO.Stream serializationStream, object graph)
        {
            _formatter.Serialize(serializationStream, graph);
        }

        #endregion

        /// <summary>
        /// Gets a reference to the underlying
        /// <see cref="BinaryFormatter"/>
        /// object.
        /// </summary>
        public BinaryFormatter Formatter
        {
            get
            {
                return _formatter;
            }
        }
    }
}
