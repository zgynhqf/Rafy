using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using OEA.ManagedProperty;
using System.ComponentModel;
using OEA.Serialization.Mobile;

namespace SimpleCsla
{
    [Serializable]
    public abstract class ReadOnlyBindingList<T> : MobileBindingList<T>
    {
        public bool IsReadOnly { get; set; }
    }

    //public abstract class MobileBindingList<T> : ReadOnlyBindingList<T>, ISerializable, IDeserializationCallback
    //{
    //    #region Serialization / Deserialization

    //    private SerializationInfo _info;

    //    protected MobileBindingList() { }

    //    /// <summary>
    //    /// 反序列化构造函数。
    //    /// 
    //    /// 需要更高安全性，加上以下这句：
    //    /// [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
    //    /// </summary>
    //    /// <param name="info"></param>
    //    /// <param name="context"></param>
    //    protected MobileBindingList(SerializationInfo info, StreamingContext context)
    //    {
    //        this._info = info;

    //        this.OnSetObjectData(info, context);
    //    }

    //    protected virtual void OnGetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        info.AddValue("SimpleCsla.Core.MobileList.AllowEdit", AllowEdit);
    //        info.AddValue("SimpleCsla.Core.MobileList.AllowNew", AllowNew);
    //        info.AddValue("SimpleCsla.Core.MobileList.AllowRemove", AllowRemove);
    //        info.AddValue("SimpleCsla.Core.MobileList.RaiseListChangedEvents", RaiseListChangedEvents);
    //    }

    //    protected virtual void OnSetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        AllowEdit = info.GetValue<bool>("SimpleCsla.Core.MobileList.AllowEdit");
    //        AllowNew = info.GetValue<bool>("SimpleCsla.Core.MobileList.AllowNew");
    //        AllowRemove = info.GetValue<bool>("SimpleCsla.Core.MobileList.AllowRemove");
    //        RaiseListChangedEvents = info.GetValue<bool>("SimpleCsla.Core.MobileList.RaiseListChangedEvents");
    //    }

    //    [SecurityCritical]
    //    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        this.OnGetObjectData(info, context);
    //    }

    //    void IDeserializationCallback.OnDeserialization(object sender)
    //    {
    //        //this.SetObjectData(this._info, new StreamingContext(StreamingContextStates.All));
    //    }

    //    [OnDeserialized]
    //    private void OnDeserialization(StreamingContext context)
    //    {
    //        //this.SetObjectData(this._info, context);
    //    }

    //    #endregion
    //}
}