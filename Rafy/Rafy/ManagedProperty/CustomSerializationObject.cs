/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using Rafy.ManagedProperty;
using System.ComponentModel;
using System.Security.Permissions;
using Rafy.Serialization.Mobile;

namespace Rafy
{
    /// <summary>
    /// 需要自定义序列化的类，都可以直接从此类继承。
    /// </summary>
    [Serializable]
    public abstract class CustomSerializationObject : MobileObject, ISerializable//, IDeserializationCallback
    {
        protected CustomSerializationObject() { }

        /// <summary>
        /// 反序列化构造函数。
        /// 
        /// 需要更高安全性，加上 SecurityPermissionAttribute 标记。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected CustomSerializationObject(SerializationInfo info, StreamingContext context) { }

        /// <summary>
        /// 序列化数据到 info 中。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void Serialize(SerializationInfo info, StreamingContext context) { }

        /// <summary>
        /// 反序列化完成时，调用此函数。 
        /// </summary>
        /// <param name="context"></param>
        protected virtual void OnDeserialized(StreamingContext context) { }

        #region 系统接口

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Serialize(info, context);
        }

        //void IDeserializationCallback.OnDeserialization(object sender)
        //{
        //    //this.SetObjectData(this._info, new StreamingContext(StreamingContextStates.All));
        //}

        [OnDeserialized]
        private void OnDeserialization(StreamingContext context)
        {
            //this.SetObjectData(this._info, context);
            this.OnDeserialized(context);
        }

        #endregion
    }
}
