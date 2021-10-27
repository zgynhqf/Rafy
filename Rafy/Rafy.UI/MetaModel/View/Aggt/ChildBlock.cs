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
using Rafy.ManagedProperty;
using System.Xml.Serialization;

namespace Rafy.MetaModel.View
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
        /// 本构造函数直接使用该属性对应实体的默认视图中的名称作用本子块的 Label
        /// </summary>
        /// <param name="childrenProperty"></param>
        public ChildBlock(IListProperty childrenProperty)
        {
            if (childrenProperty == null) throw new ArgumentNullException("childrenProperty");

            this.ChildrenProperty = childrenProperty;
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

        /// <summary>
        /// 本子块显示的标题
        /// </summary>
        public string Label { get; set; }

        [XmlIgnore]
        public AggtBlocks Owner { get; internal set; }

        private IManagedProperty _ChildrenProperty;
        /// <summary>
        /// 子属性一般情况下是一个 IListProperty，
        /// 但是也有可能是一个 IRefProperty
        /// </summary>
        [XmlIgnore]
        public IManagedProperty ChildrenProperty
        {
            get
            {
                if (this._ChildrenProperty == null && !string.IsNullOrEmpty(this._ChildrenPropertyName))
                {
                    var em = CommonModel.Entities.Get(this.Owner.MainBlock.EntityType);
                    PropertyMeta cp = em.ChildrenProperty(this._ChildrenPropertyName);
                    cp = cp ?? em.Property(this._ChildrenPropertyName);

                    this._ChildrenProperty = cp.ManagedProperty;
                }
                return this._ChildrenProperty;
            }
            set
            {
                this._ChildrenProperty = value;
                if (value != null)
                {
                    this._ChildrenPropertyName = value.Name;

                    var listProperty = value as IListProperty;
                    if (listProperty != null)
                    {
                        this.EntityType = listProperty.ListEntityType;
                    }
                    else
                    {
                        var refProperty = value as IRefProperty;
                        if (refProperty == null) throw new InvalidOperationException("子属性必须是一个列表属性或者一个引用属性。");

                        this.EntityType = refProperty.RefEntityType;
                    }

                    //使用该属性对应实体的默认视图中的名称作用本子块的 Label
                    if (string.IsNullOrEmpty(this.Label))
                    {
                        var defaultView = UIModel.Views.CreateBaseView(this.EntityType);
                        this.Label = defaultView.Label;
                    }
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
                    var em = this.ViewMeta.EntityMeta;
                    this._ChildrePropertyMeta =
                        em.Property(this.ChildrenProperty) as PropertyMeta ??
                        em.ChildrenProperty(this.ChildrenProperty);
                }

                return this._ChildrePropertyMeta;
            }
        }
    }
}