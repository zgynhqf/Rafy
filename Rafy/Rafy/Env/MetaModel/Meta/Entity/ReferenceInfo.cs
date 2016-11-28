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
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 实体引用关系
    /// </summary>
    public class ReferenceInfo : Freezable
    {
        #region 字段

        private EntityMeta _RefTypeMeta;

        #endregion

        /// <summary>
        /// 对应的引用实体属性（托管属性）。
        /// </summary>
        public IRefEntityProperty RefEntityProperty { get; internal set; }

        /// <summary>
        /// 引用实体的实体元数据
        /// 
        /// 当此引用属性引用的是一个抽象的实体类，这个抽象类并没有对应的实体元数据，可能这个抽象的子类才有实体元数据，
        /// 在这种情况下，此属性值将为 null。
        /// </summary>
        public EntityMeta RefTypeMeta
        {
            get
            {
                this.InitLazyRefTypeMeta();
                return this._RefTypeMeta;
            }
            set { this.SetValue(ref this._RefTypeMeta, value); }
        }

        /// <summary>
        /// 引用的类型
        /// </summary>
        public ReferenceType Type
        {
            get { return this.RefEntityProperty.ReferenceType; }
        }

        /// <summary>
        /// 引用实体的实体类型
        /// </summary>
        public Type RefType
        {
            get { return this.RefEntityProperty.RefEntityType; }
        }

        /// <summary>
        /// 懒加载 RefTypeMeta 属性。
        /// </summary>
        private void InitLazyRefTypeMeta()
        {
            if (this._RefTypeMeta == null)
            {
                this._RefTypeMeta = CommonModel.Entities.Find(this.RefType);
            }
        }
    }
}