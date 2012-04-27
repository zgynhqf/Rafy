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

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 外键引用实体属性的相关视图信息
    /// </summary>
    public class ReferenceViewInfo : Freezable
    {
        private ReferenceInfo _RefInfo;
        /// <summary>
        /// 对应的引用信息
        /// 
        /// 可能为 null，此时表示此属性只是在界面上有引用元数据，
        /// 但是定义的实体类型却没有相关的引用信息（例如根本没有定义实体引用属性）。
        /// </summary>
        public ReferenceInfo ReferenceInfo
        {
            get { return this._RefInfo; }
            set { this.SetValue(ref this._RefInfo, value); }
        }

        private ReferenceSelectionMode _SelectionMode;
        /// <summary>
        /// 选择模式：多选/单选。
        /// </summary>
        public ReferenceSelectionMode SelectionMode
        {
            get { return this._SelectionMode; }
            set { this.SetValue(ref this._SelectionMode, value); }
        }

        private string _SplitterIfMulti = ",";
        /// <summary>
        /// 多选模式下，返回的值应该根据这个进行分隔
        /// </summary>
        public string SplitterIfMulti
        {
            get { return this._SplitterIfMulti; }
            set { this.SetValue(ref this._SplitterIfMulti, value); }
        }

        private string _SelectedValuePath;
        /// <summary>
        /// 一个路径表达式。
        /// 下拉选择时，从当前选中的对象找到值，赋值给LookupPropertyName指定的属性
        /// </summary>
        public string SelectedValuePath
        {
            get { return this._SelectedValuePath; }
            set { this.SetValue(ref this._SelectedValuePath, value); }
        }

        private string _DataSourceProperty;
        /// <summary>
        /// 查询时，数据来源的属性。在这个属性里面查找值。
        /// 级联属性过滤串,格式如:属性.子属性.子子属性...
        /// 
        /// 如果未设置这个值，则会调用数据层方法查询
        /// </summary>
        public string DataSourceProperty
        {
            get { return this._DataSourceProperty; }
            set { this.SetValue(ref this._DataSourceProperty, value); }
        }

        private string _RootPIdProperty;
        /// <summary>
        /// 如果Lookup是树形结构，设置或获取RootPId的属性名，可以只显示部分树
        /// </summary>
        public string RootPIdProperty
        {
            get { return this._RootPIdProperty; }
            set { this.SetValue(ref this._RootPIdProperty, value); }
        }

        private EntityViewMeta _RefTypeDefaultView;
        public EntityViewMeta RefTypeDefaultView
        {
            get
            {
                if (this._RefTypeDefaultView == null)
                {
                    this._RefTypeDefaultView = UIModel.Views.CreateDefaultView(this.RefType);
                }

                return this._RefTypeDefaultView;
            }
        }

        private Type _RefType;
        /// <summary>
        /// 引用实体类型
        /// </summary>
        public Type RefType
        {
            get
            {
                if (this._RefInfo != null)
                {
                    return this._RefInfo.RefType;
                }

                if (this._RefType == null) { throw new InvalidOperationException("属性不能为空，请先设置本属性。"); }
                return this._RefType;
            }
            set { this.SetValue(ref this._RefType, value); }
        }

        public string RefEntityProperty
        {
            get
            {
                if (this._RefInfo != null)
                {
                    return this._RefInfo.RefEntityProperty;
                }

                return null;
            }
        }
    }
}