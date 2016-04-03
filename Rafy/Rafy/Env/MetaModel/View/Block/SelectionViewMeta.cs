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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 选择实体的相关视图信息
    /// </summary>
    public class SelectionViewMeta : Freezable
    {
        private Type _SelectionEntityType;
        /// <summary>
        /// 被用来选择的实体类型。
        /// 必填项。
        /// 
        /// 这个类型可以直接与引用属性对应的实体类型相同，也可以是一个其它的实体，方便客户进行选择。
        /// </summary>
        public Type SelectionEntityType
        {
            get
            {
                //如果是显式设置的值，则使用此值。
                if (this._SelectionEntityType != null) return this._SelectionEntityType;

                //如果没有显式设置，而且是一个引用属性，则使用引用属性对应的实体。
                if (this._RefInfo != null) { return this._RefInfo.RefType; }

                return null;
            }
            set
            {
                if (value != null) this._RefInfo = null;

                this.SetValue(ref this._SelectionEntityType, value);
            }
        }

        private ReferenceInfo _RefInfo;
        /// <summary>
        /// 对应的引用信息
        /// 
        /// 可能为 null，此时表示此属性只是在界面上有引用元数据，
        /// 但是定义的实体类型却没有相关的引用信息（例如根本没有定义实体引用属性）。
        /// </summary>
        public ReferenceInfo RefInfo
        {
            get { return this._RefInfo; }
            set { this.SetValue(ref this._RefInfo, value); }
        }

        [UnAutoFreeze]
        private EntityViewMeta _RefTypeDefaultView;
        /// <summary>
        /// 引用实体对应的默认视图
        /// </summary>
        public EntityViewMeta RefTypeDefaultView
        {
            get
            {
                if (this._RefTypeDefaultView == null)
                {
                    this._RefTypeDefaultView = UIModel.Views.CreateBaseView(this.SelectionEntityType);
                }

                return this._RefTypeDefaultView;
            }
        }

        private IManagedProperty _DataSourceProperty;
        /// <summary>
        /// 查询时，数据来源的属性。在这个属性里面查找值。
        /// 
        /// 如果未设置这个值，则会调用数据层方法查询完整的实体列表。
        /// </summary>
        public IManagedProperty DataSourceProperty
        {
            get { return this._DataSourceProperty; }
            set { this.SetValue(ref this._DataSourceProperty, value); }
        }

        #region WPF

        private Func<object> _DataSourceProvider;
        /// <summary>
        /// 如果是没有指定界面时，可以使用这个属性来指定弹出窗口的数据源。
        /// 可选。
        /// </summary>
        public Func<object> DataSourceProvider
        {
            get { return this._DataSourceProvider; }
            set { this.SetValue(ref this._DataSourceProvider, value); }
        }

        private EntitySelectionMode _SelectionMode;
        /// <summary>
        /// 选择模式：多选/单选。
        /// </summary>
        public EntitySelectionMode SelectionMode
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

        private IManagedProperty _SelectedValuePath;
        /// <summary>
        /// 选择后的值在目标实体中对应的托管属性。
        /// 可选。
        /// 如果是非引用属性，需要填写这个属性的值。否则默认为 Entity.IdProperty
        /// </summary>
        public IManagedProperty SelectedValuePath
        {
            get { return this._SelectedValuePath; }
            set { this.SetValue(ref this._SelectedValuePath, value); }
        }

        private IManagedProperty _RefIdHost;
        /// <summary>
        /// 如果在属性被设置完成后，还需要把被选择实体的 Id 一便赋值过来时，
        /// 则需要指定此值来说明使用哪个属性来接收被选择实体的 Id 或者 Id 列表。
        /// 
        /// 如果是单选模式，则这个属性的类型必须是 int?，
        /// 如果是多选模式，则这个属性的类型必须是 int[]（数组类型）。
        /// </summary>
        public IManagedProperty RefIdHost
        {
            get { return this._RefIdHost; }
            set { this.SetValue(ref this._RefIdHost, value); }
        }

        private RefSelectedCallBack _RefSelectedCallBack;
        /// <summary>
        /// 引用选择完毕后的回调函数
        /// </summary>
        public RefSelectedCallBack RefSelectedCallBack
        {
            get { return this._RefSelectedCallBack; }
            set { this.SetValue(ref this._RefSelectedCallBack, value); }
        }

        #endregion
    }

    /// <summary>
    /// 引用选择完毕后的回调函数
    /// </summary>
    /// <param name="owner">弹出窗口对应的当前实体</param>
    /// <param name="selectedEntities">被选择的实体列表，Count 的范围是自然数。</param>
    public delegate void RefSelectedCallBack(ManagedPropertyObject owner, IList selectedEntities);
}