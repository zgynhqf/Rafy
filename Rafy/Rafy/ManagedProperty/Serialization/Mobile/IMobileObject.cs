using System;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Interface to be implemented by any object
    /// that supports serialization by the
    /// MobileFormatter.
    /// </summary>
    public interface IMobileObject
    {
        /// <summary>
        /// Method called by MobileFormatter when an object
        /// should serialize its data. The data should be
        /// serialized into the SerializationInfo parameter.
        /// </summary>
        /// <param name="info">
        /// Object to contain the serialized data.
        /// </param>
        void SerializeState(ISerializationContext info);

        /// <summary>
        /// Method called by MobileFormatter when an object
        /// should serialize its child references. The data should be
        /// serialized into the SerializationInfo parameter.
        /// </summary>
        /// <param name="info">Object to contain the serialized data.</param>
        void SerializeRef(ISerializationContext info);

        /// <summary>
        /// Method called by MobileFormatter when an object
        /// should be deserialized. The data should be
        /// deserialized from the SerializationInfo parameter.
        /// </summary>
        /// <param name="info">
        /// Object containing the serialized data.
        /// </param>
        void DeserializeState(ISerializationContext info);

        /// <summary>
        /// Method called by MobileFormatter when an object
        /// should deserialize its child references. The data should be
        /// deserialized from the SerializationInfo parameter.
        /// </summary>
        /// <param name="info">Object containing the serialized data.</param>
        void DeserializeRef(ISerializationContext info);
    }
}
