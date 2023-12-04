/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130328 23:26
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130328 23:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 引用属性。
    /// </summary>
    public class Reference : Property
    {
        private ReferenceType _referenceType;
        /// <summary>
        /// 引用属性类型
        /// </summary>
        public ReferenceType ReferenceType
        {
            get { return this._referenceType; }
            set { this._referenceType = value; }
        }

        private string _keyProperty;
        /// <summary>
        /// 引用属性的 Id 属性。
        /// </summary>
        public string KeyProperty
        {
            get { return this._keyProperty; }
            set { this._keyProperty = value; }
        }

        private string _entityProperty;
        /// <summary>
        /// 本引用对应的实体引用属性。
        /// 
        /// 可能一些引用没有编写对应的引用实体属性，而只有引用 Id 属性，这时，这个属性返回空。
        /// </summary>
        public string EntityProperty
        {
            get { return this._entityProperty; }
            set { this._entityProperty = value; }
        }

        private EntityType _refEntityType;
        /// <summary>
        /// 引用的实体类型
        /// </summary>
        public EntityType RefEntityType
        {
            get { return this._refEntityType; }
            set
            {
                if (_refEntityType != value)
                {
                    if (value != null)
                    {
                        //同步 _idProperty
                        if (string.IsNullOrWhiteSpace(_keyProperty))
                        {
                            if (!string.IsNullOrEmpty(_entityProperty))
                            {
                                _keyProperty = _entityProperty + Convention.Id;
                            }
                            else
                            {
                                _keyProperty = value.Name + Convention.Id;
                            }
                        }
                    }

                    _refEntityType = value;
                }
            }
        }

        private bool _nullable;
        /// <summary>
        /// 此引用是否可空。
        /// </summary>
        public bool Nullable
        {
            get { return _nullable; }
            set { _nullable = value; }
        }

        internal override string GetName()
        {
            return _entityProperty;
        }

        internal override string GetPropertyType()
        {
            return _refEntityType != null ? _refEntityType.Name : string.Empty;
        }
    }
}