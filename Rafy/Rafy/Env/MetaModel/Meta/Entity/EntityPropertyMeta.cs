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
    /// 业务属性实体模型
    /// </summary>
    public class EntityPropertyMeta : PropertyMeta
    {
        private ReferenceInfo _ReferenceInfo;
        /// <summary>
        /// 如果此属性是引用实体属性，则这个属性不为空，并表示引用的相关信息。
        /// </summary>
        public ReferenceInfo ReferenceInfo
        {
            get { return this._ReferenceInfo; }
            set { this.SetValue(ref this._ReferenceInfo, value); }
        }

        private ColumnMeta _ColumnMeta;
        /// <summary>
        /// 如果这个属性不为 null，表示该属性映射数据库中的某个字段。
        /// </summary>
        public ColumnMeta ColumnMeta
        {
            get { return this._ColumnMeta; }
            set { this.SetValue(ref this._ColumnMeta, value); }
        }
    }
}