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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;
using System.Collections.ObjectModel;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Inherit from this base class to easily
    /// create a serializable list class.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the items contained in the list.
    /// </typeparam>
    [Serializable]
    public class MobileCollection<T> : ObservableCollection<T>, IMobileObject
    {
        #region IMobileObject Members

        void IMobileObject.SerializeState(ISerializationContext info)
        {
            OnSerializeState(info);
        }

        void IMobileObject.SerializeRef(ISerializationContext info)
        {
            OnSerializeRef(info);
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
        /// Override this method to get custom child object
        /// values from the serialization stream.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <exception cref="System.InvalidOperationException">CannotSerializeCollectionsNotOfIMobileObject</exception>
        protected virtual void OnSerializeRef(ISerializationContext info)
        {
            if (!typeof(IMobileObject).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException("CannotSerializeCollectionsNotOfIMobileObject");
            }

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

        /// <summary>
        /// Override this method to get custom field values
        /// from the serialization stream.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        protected virtual void OnSerializeState(ISerializationContext info)
        {
            //使用 bool 数组
            //bool[] values = { AllowEdit, AllowNew, AllowRemove, RaiseListChangedEvents };
            //info.AddValue("ListValues", values);

            //使用 byte
            //byte iValues = 0;
            //if (AllowEdit) iValues |= 8;
            //if (AllowNew) iValues |= 4;
            //if (AllowRemove) iValues |= 2;
            //if (RaiseListChangedEvents) iValues |= 1;
            //info.AddState("$ListValues", iValues);

            //var bc = new BitContainer();
            //bc.SetValue(Bit._1, AllowEdit);
            //bc.SetValue(Bit._2, AllowNew);
            //bc.SetValue(Bit._4, AllowRemove);
            //bc.SetValue(Bit._8, RaiseListChangedEvents);
            //info.AddState("$ListValues", bc.ToInt32());
        }

        /// <summary>
        /// Override this method to set custom field values
        /// into the serialization stream.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        protected virtual void OnDeserializeState(ISerializationContext info)
        {
            //var bc = new BitContainer(info.GetState<int>("$ListValues"));
            //AllowEdit = bc.GetValue(Bit._1);
            //AllowNew = bc.GetValue(Bit._2);
            //AllowRemove = bc.GetValue(Bit._4);
            //RaiseListChangedEvents = bc.GetValue(Bit._8);

            //使用 byte
            //var iValues = info.GetState<byte>("$ListValues");
            //AllowEdit = (iValues & 8) == 8;
            //AllowNew = (iValues & 4) == 4;
            //AllowRemove = (iValues & 2) == 2;
            //RaiseListChangedEvents = (iValues & 1) == 1;

            //使用 bool 数组
            //var values = info.GetValue<bool[]>("ListValues");
            //AllowEdit = values[0];
            //AllowNew = values[1];
            //AllowRemove = values[2];
            //RaiseListChangedEvents = values[3];
        }

        /// <summary>
        /// Override this method to set custom child object
        /// values into the serialization stream.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <exception cref="System.InvalidOperationException">CannotSerializeCollectionsNotOfIMobileObject</exception>
        protected virtual void OnDeserializeRef(ISerializationContext info)
        {
            if (!typeof(IMobileObject).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException("CannotSerializeCollectionsNotOfIMobileObject");
            }

            var formatter = info.RefFormatter;

            var references = info.GetState<object[]>("$list");
            if (references != null)
            {
                foreach (int reference in references)
                {
                    T child = formatter.GetObject<T>(reference);
                    this.Add(child);
                }
            }
        }
    }
}