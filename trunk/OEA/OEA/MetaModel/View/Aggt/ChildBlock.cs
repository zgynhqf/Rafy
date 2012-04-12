/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using System.Xml.Serialization;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 孩子块
    /// 
    /// 支持 XML 序列化
    /// </summary>
    public class ChildBlock : Block
    {
        /// <summary>
        /// 构造一个自定义界面孩子块
        /// </summary>
        /// <param name="label"></param>
        /// <param name="customUI"></param>
        public ChildBlock(string label, string customUI)
        {
            if (label == null) throw new ArgumentNullException("label");
            if (customUI == null) throw new ArgumentNullException("customUI");

            this.Label = label;
            this.CustomViewType = customUI;
        }

        /// <summary>
        /// 通过一个聚合子实体属性构造一个界面块
        /// </summary>
        /// <param name="label"></param>
        /// <param name="childrenProperty"></param>
        public ChildBlock(string label, IListProperty childrenProperty)
        {
            if (label == null) throw new ArgumentNullException("label");
            if (childrenProperty == null) throw new ArgumentNullException("childrenProperty");

            this.Label = label;
            this.ChildrenProperty = childrenProperty;
        }

        internal ChildBlock() { }

        /// <summary>
        /// 本子块显示的标题
        /// </summary>
        public string Label { get; set; }

        [XmlIgnore]
        public AggtBlocks Owner { get; internal set; }

        private IListProperty _ChildrenProperty;
        [XmlIgnore]
        public IListProperty ChildrenProperty
        {
            get
            {
                if (this._ChildrenProperty == null && !string.IsNullOrEmpty(this._ChildrenPropertyName))
                {
                    this._ChildrenProperty = CommonModel.Entities.Get(this.Owner.MainBlock.EntityType)
                        .ChildrenProperty(this._ChildrenPropertyName)
                        .ManagedProperty as IListProperty;
                }
                return this._ChildrenProperty;
            }
            set
            {
                this._ChildrenProperty = value;
                if (value != null)
                {
                    this._ChildrenPropertyName = value.Name;
                    this.EntityType = value.ListEntityType;
                }
            }
        }

        private string _ChildrenPropertyName;
        public string ChildrenPropertyName
        {
            get { return _ChildrenPropertyName; }
            set
            {
                this._ChildrenPropertyName = value;
                this._ChildrenProperty = null;
            }
        }

        private PropertyMeta _ChildrePropertyMeta;
        /// <summary>
        /// 缓存 PropertyMeta 的属性
        /// </summary>
        public PropertyMeta ChildrenPropertyMeta
        {
            get
            {
                if (this._ChildrePropertyMeta == null)
                {
                    this._ChildrePropertyMeta = this.EVM.EntityMeta.FindProperty(this.ChildrenProperty);
                }

                return this._ChildrePropertyMeta;
            }
        }
    }
}