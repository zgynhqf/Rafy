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
using Rafy;
using System.IO;
using System.Diagnostics;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 某个小块的定义
    /// 
    /// 该类可以被 XML 序列化
    /// </summary>
    public class Block
    {
        #region 构造器

        public Block(Type entityType)
        {
            this.BlockType = BlockType.List;
            this.EntityType = entityType;
        }

        protected Block()
        {
            this.BlockType = BlockType.List;
        }

        #endregion

        #region public string KeyLabel { get; set; }

        private string _keyLabel;
        /// <summary>
        /// 当前块在整个聚合块中的唯一标记。（将被权限系统所使用）
        /// 
        /// 默认使用当前实体类型的 Label 作为此标记。
        /// 如果一个聚合块中出现两个相同的块使用同一实体时，应该手动设置本属性以区分两个块。
        /// </summary>
        [XmlIgnore]
        public string KeyLabel
        {
            get
            {
                if (this._keyLabel != null) { return this._keyLabel; }
                return this.ViewMeta.Label;
            }
            set { this._keyLabel = value; }
        }
        /// <summary>
        /// 内部使用！！！
        /// 为了简单地满足 XML 序列化，所以写了这个“私有”属性
        /// </summary>
        [XmlAttribute("KeyLabel")]
        public string _KeyLabelForXml
        {
            get { return this._keyLabel; }
            set { this._keyLabel = value; }
        }

        #endregion

        #region public Type EntityType { get; set; }

        private string __EntityTypeForXml;
        private Type _EntityType;
        /// <summary>
        /// 当前块显示的实体类型
        /// </summary>
        [XmlIgnore]
        public Type EntityType
        {
            get
            {
                if (this._EntityType == null && !string.IsNullOrEmpty(this.__EntityTypeForXml))
                {
                    this._EntityType = Type.GetType(this.__EntityTypeForXml);
                }
                return this._EntityType;
            }
            set
            {
                this._EntityType = value;
                this.__EntityTypeForXml = value.AssemblyQualifiedName;
            }
        }
        /// <summary>
        /// 内部使用！！！
        /// 为了简单地满足 XML 序列化，所以写了这个“私有”属性
        /// </summary>
        [XmlAttribute("EntityType")]
        public string _EntityTypeForXml
        {
            get { return this.__EntityTypeForXml; }
            set
            {
                this.__EntityTypeForXml = value;
                this._EntityType = null;
            }
        }

        #endregion

        /// <summary>
        /// 该块如果不是使用默认视图，则这个属性表示所使用的扩展视图的名称
        /// </summary>
        public Type ExtendView { get; set; }

        /// <summary>
        /// 块类型
        /// </summary>
        public BlockType BlockType { get; set; }

        private EntityViewMeta _ViewMeta;
        /// <summary>
        /// 缓存 EVM 的属性（当前块显示的实体类型）
        /// </summary>
        public EntityViewMeta ViewMeta
        {
            get
            {
                this.InitViewMeta();
                return this._ViewMeta;
            }
        }

        internal void InitViewMeta()
        {
            if (this._ViewMeta == null)
            {
                this._ViewMeta = UIModel.Views.Create(this.EntityType, ViewConfig.GetViewName(this.ExtendView));

                this.UseBlockDefaultCommands(this._ViewMeta);
            }
        }

        /// <summary>
        /// 使用本块中默认的一些按钮。
        /// </summary>
        protected virtual void UseBlockDefaultCommands(EntityViewMeta meta)
        {
            if (RafyEnvironment.Location.IsWPFUI)
            {
                if (this.BlockType == BlockType.Report)
                {
                    meta.AsWPFView().UseCommands(WPFCommandNames.RefreshDataSourceInRDLC, WPFCommandNames.ShowReportData);
                }
            }
        }

        #region WPF

        /// <summary>
        /// 如果该块是自定义界面，则此属性表示这个这个自定义的 UI 界面所对应的 LogicalView 类型
        /// </summary>
        public string CustomViewType { get; set; }

        #endregion
    }
}