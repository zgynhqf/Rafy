using System;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Placeholder for null child objects.
    /// </summary>
    [Serializable()]
    public sealed class NullPlaceholder : IMobileObject
    {
        public static readonly string TypeShortName = "n";

        public static readonly NullPlaceholder Instance = new NullPlaceholder();

        private NullPlaceholder() { }

        #region IMobileObject Members

        public void SerializeState(ISerializationContext info)
        {
            // Nothing
        }

        public void SerializeRef(ISerializationContext info)
        {
            // Nothing
        }

        public void DeserializeState(ISerializationContext info)
        {
            // Nothing
        }

        public void DeserializeRef(ISerializationContext info)
        {
            // Nothing
        }

        #endregion
    }
}
