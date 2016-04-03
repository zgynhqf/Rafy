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
using System.ComponentModel;
using Rafy.Serialization;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// 当使用 MobileFormatter 作为序列化时，才会运行此类的代码。
    /// 也就是说，二进制序列化不会使用以下代码。
    /// </summary>
    [Serializable]
    public abstract class MobileObject : IMobileObject
    {
        #region IMobileObject 成员

        void IMobileObject.SerializeRef(ISerializationContext info)
        {
            OnMobileSerializeRef(info);
        }

        void IMobileObject.SerializeState(ISerializationContext info)
        {
            OnMobileSerializeState(info);
        }

        void IMobileObject.DeserializeState(ISerializationContext info)
        {
            OnMobileDeserializeState(info);
        }

        void IMobileObject.DeserializeRef(ISerializationContext info)
        {
            OnMobileDeserializeRef(info);
        }

        #endregion

        /// <summary>
        /// Override this method to insert your child object
        /// references into the MobileFormatter serialzation stream.
        /// </summary>
        /// <param name="context">Object containing the data to serialize.</param>
        protected virtual void OnMobileSerializeRef(ISerializationContext context)
        {
            FieldsSerializationHelper.SerialzeFields(this, context);
        }

        /// <summary>
        /// Override this method to insert your field values
        /// into the MobileFormatter serialzation stream.
        /// </summary>
        /// <param name="context">Object containing the data to serialize.</param>
        protected virtual void OnMobileSerializeState(ISerializationContext context)
        {
            FieldsSerializationHelper.SerialzeFields(this, context);
        }

        /// <summary>
        /// Override this method to retrieve your field values
        /// from the MobileFormatter serialzation stream.
        /// </summary>
        /// <param name="context">Object containing the data to serialize.</param>
        protected virtual void OnMobileDeserializeState(ISerializationContext context)
        {
            FieldsSerializationHelper.DeserialzeFields(this, context);
        }

        /// <summary>
        /// Override this method to retrieve your child object
        /// references from the MobileFormatter serialzation stream.
        /// </summary>
        /// <param name="context">Object containing the data to serialize.</param>
        protected virtual void OnMobileDeserializeRef(ISerializationContext context)
        {
            FieldsSerializationHelper.DeserialzeFields(this, context);
        }
    }
}
