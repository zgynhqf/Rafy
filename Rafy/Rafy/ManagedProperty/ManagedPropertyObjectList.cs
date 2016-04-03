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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Rafy.Serialization.Mobile;
using System.Reflection;
using Rafy.Serialization;
using System.Collections;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性对象的集合基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ManagedPropertyObjectList<T> : MobileCollection<T>//, IListSource
    {
        #region Mobile Srialization

        protected override void OnSerializeRef(ISerializationContext info)
        {
            base.OnSerializeRef(info);

            FieldsSerializationHelper.SerialzeFields(this, info);
        }

        protected override void OnSerializeState(ISerializationContext info)
        {
            base.OnSerializeState(info);

            FieldsSerializationHelper.SerialzeFields(this, info);
        }

        protected override void OnDeserializeState(ISerializationContext info)
        {
            FieldsSerializationHelper.DeserialzeFields(this, info);

            base.OnDeserializeState(info);
        }

        protected override void OnDeserializeRef(ISerializationContext info)
        {
            FieldsSerializationHelper.DeserialzeFields(this, info);

            base.OnDeserializeRef(info);
        }

        #endregion

        //#region IListSource Members

        //bool IListSource.ContainsListCollection
        //{
        //    get { return false; }
        //}

        //[NonSerialized]
        //private ManagedPropertyObjectListView _viewCache;

        //public ManagedPropertyObjectListView ViewCache
        //{
        //    get { return this._viewCache; }
        //}

        //IList IListSource.GetList()
        //{
        //    if (this._viewCache == null)
        //    {
        //        this._viewCache = new ManagedPropertyObjectListView(this);
        //    }

        //    return this._viewCache;
        //}

        //#endregion
    }
}