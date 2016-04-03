/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.Attributes;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 子属性元数据
    /// </summary>
    public class ChildrenPropertyMeta : PropertyMeta
    {
        private EntityMeta _ChildEntityMeta;

        /// <summary>
        /// 此孩子属性对应的实体类的类型
        /// </summary>
        public EntityMeta ChildType
        {
            get { return this._ChildEntityMeta; }
            set { this.SetValue(ref this._ChildEntityMeta, value); }
        }
    }
}