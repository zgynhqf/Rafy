using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Serialization.Mobile;
using Rafy.Serialization;
using serialization = System.Runtime.Serialization;
using System.IO;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Implements a list that is serializable using
    /// the MobileFormatter.
    /// </summary>
    /// <typeparam name="T">
    /// Type of object contained in the list.
    /// </typeparam>
    [Serializable]
    public class MobileList<T> : List<T>, IMobileObject
    {
        /// <summary>
        /// Creates an instance of the type.
        /// </summary>
        public MobileList() : base() { }

        /// <summary>
        /// Creates an instance of the type.
        /// </summary>
        /// <param name="capacity">Capacity of the list.</param>
        public MobileList(int capacity) : base(capacity) { }

        /// <summary>
        /// Creates an instance of the type.
        /// </summary>
        /// <param name="collection">Data to add to list.</param>
        public MobileList(IEnumerable<T> collection) : base(collection) { }

        #region IMobileObject Members

        void IMobileObject.SerializeRef(ISerializationContext info)
        {
            OnSerializeRef(info);
        }

        void IMobileObject.SerializeState(ISerializationContext info)
        {
            OnSerializeState(info);
        }

        void IMobileObject.DeserializeState(ISerializationContext info)
        {
            OnDeserializeState(info);
        }

        void IMobileObject.DeserializeRef(ISerializationContext info)
        {
            OnDeserializeRef(info);
        }

        #endregion

        /// <summary>
        /// Override this method to manually serialize child objects
        /// contained within the current object.
        /// </summary>
        /// <param name="info">Object containing serialized values.</param>
        protected virtual void OnSerializeRef(ISerializationContext info)
        {
            FieldsSerializationHelper.SerialzeFields(this, info);

            bool mobileChildren = typeof(IMobileObject).IsAssignableFrom(typeof(T));
            if (mobileChildren)
            {
                var formatter = info.RefFormatter;

                var references = new List<int>(this.Count);
                for (int x = 0; x < this.Count; x++)
                {
                    T child = this[x];
                    if (child != null)
                    {
                        var childInfo = formatter.SerializeObject(child);
                        references.Add(childInfo.ReferenceId);
                    }
                }

                if (references.Count > 0)
                {
                    info.AddState("$list", references);
                }
            }
            else
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    serialization.DataContractSerializer serializer = new serialization.DataContractSerializer(GetType());
                    serializer.WriteObject(stream, this);
                    stream.Flush();
                    info.AddState("$list", stream.ToArray());
                }
            }
        }

        /// <summary>
        /// Override this method to add extra field values to
        /// the serialization stream.
        /// </summary>
        /// <param name="info">Object containing field values.</param>
        protected virtual void OnSerializeState(ISerializationContext info)
        {
            FieldsSerializationHelper.SerialzeFields(this, info);
        }

        /// <summary>
        /// Override this method to retrieve extra field values to
        /// the serialization stream.
        /// </summary>
        /// <param name="info">Object containing field values.</param>
        protected virtual void OnDeserializeState(ISerializationContext info)
        {
            FieldsSerializationHelper.DeserialzeFields(this, info);
        }

        /// <summary>
        /// Override this method to manually deserialize child objects
        /// from data in the serialization stream.
        /// </summary>
        /// <param name="info">Object containing serialized values.</param>
        protected virtual void OnDeserializeRef(ISerializationContext info)
        {
            FieldsSerializationHelper.DeserialzeFields(this, info);

            bool mobileChildren = typeof(IMobileObject).IsAssignableFrom(typeof(T));
            if (mobileChildren)
            {
                var formatter = info.RefFormatter;
                var references = info.GetState<object[]>("$list");
                if (references != null)
                {
                    foreach (int reference in references)
                    {
                        T child = (T)formatter.GetObject(reference);
                        this.Add(child);
                    }
                }
            }
            else
            {
                var bufferObj = info.GetState<object[]>("$list");

                var buffer = new byte[bufferObj.Length];
                for (int i = 0, c = bufferObj.Length; i < c; i++)
                {
                    buffer[i] = (byte)(int)bufferObj[i];
                }

                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    var dcs = new serialization.DataContractSerializer(GetType());
                    MobileList<T> list = (MobileList<T>)dcs.ReadObject(stream);
                    AddRange(list);
                }
            }
        }
    }
}
