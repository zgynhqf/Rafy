/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110316
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100316
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 实体引用关系
    /// </summary>
    public class ReferenceInfo : Freezable
    {
        #region 字段

        private ReferenceType _Type;

        private Type _RefType;

        private string _RefEntityProperty;

        private EntityMeta _RefTypeMeta;

        #endregion

        public ReferenceType Type
        {
            get { return this._Type; }
            set { this.SetValue(ref this._Type, value); }
        }

        /// <summary>
        /// 引用实体的实体类型
        /// </summary>
        public Type RefType
        {
            get { return this._RefType; }
            set { this.SetValue(ref this._RefType, value); }
        }

        /// <summary>
        /// 引用实体的属性名
        /// </summary>
        public string RefEntityProperty
        {
            get { return this._RefEntityProperty; }
            set { this.SetValue(ref this._RefEntityProperty, value); }
        }

        /// <summary>
        /// 引用实体的实体元数据
        /// 
        /// 当此引用属性引用的是一个抽象的实体类，这个抽象类并没有对应的实体元数据，可能这个抽象的子类才有实体元数据，
        /// 在这种情况下，此属性值将为 null。
        /// </summary>
        public EntityMeta RefTypeMeta
        {
            get { return this._RefTypeMeta; }
            set { this.SetValue(ref this._RefTypeMeta, value); }
        }
    }

    public enum ReferenceType
    {
        /// <summary>
        /// 一般的外键引用
        /// </summary>
        Normal,

        /// <summary>
        /// 此引用表示父实体的引用
        /// </summary>
        Parent,

        /// <summary>
        /// 此引用表示子实体的引用，一对一的子实体关系。
        /// </summary>
        Child
    }
}