/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130830
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130830 17:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;

namespace Rafy.MetaModel
{
    public abstract class ViewConfig : EntityConfigBase
    {
        /// <summary>
        /// 如果是扩展视图，需要重写这个属性。
        /// </summary>
        internal string ExtendView
        {
            get
            {
                if (this.IsExtendView) { return GetViewName(this.GetType()); }
                return null;
            }
        }

        /// <summary>
        /// 如果是扩展视图，需要重写这个属性，并返回 true。
        /// </summary>
        protected virtual bool IsExtendView
        {
            get { return false; }
        }

        /// <summary>
        /// 返回视图类型对应的视图名称。
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>
        internal static string GetViewName(Type viewType)
        {
            if (viewType == null) return null;

            return viewType.FullName;
        }
    }

    /// <summary>
    /// 实体配置基类
    /// </summary>
    public abstract class WPFViewConfig : ViewConfig
    {
        private WPFEntityViewMeta _view;

        protected internal WPFEntityViewMeta View
        {
            get
            {
                if (this._view == null)
                {
                    //只获取 代码视图
                    this._view = UIModel.Views.Create(this.EntityType, this.ExtendView, null) as WPFEntityViewMeta;
                }

                return this._view;
            }
            internal set { this._view = value; }
        }

        internal void UseDefaultMeta()
        {
            this._view = null;
        }

        /// <summary>
        /// 子类重写此方法，并完成对 Meta 属性的配置。
        /// 
        /// 注意：
        /// * 为了给当前类的子类也运行同样的配置，这个方法可能会被调用多次。
        /// </summary>
        protected internal virtual void ConfigView() { }
    }

    /// <summary>
    /// 泛型版本的实体配置基类
    /// </summary>
    public abstract class WPFViewConfig<TEntity> : WPFViewConfig
    {
        protected internal override sealed Type EntityType
        {
            get { return typeof(TEntity); }
        }
    }

    /// <summary>
    /// 实体配置基类
    /// </summary>
    public abstract class WebViewConfig : ViewConfig
    {
        private WebEntityViewMeta _view;

        protected internal WebEntityViewMeta View
        {
            get
            {
                if (this._view == null)
                {
                    //只获取 代码视图
                    this._view = UIModel.Views.Create(this.EntityType, this.ExtendView, null) as WebEntityViewMeta;
                }

                return this._view;
            }
            internal set { this._view = value; }
        }

        internal void UseDefaultMeta()
        {
            this._view = null;
        }

        /// <summary>
        /// 子类重写此方法，并完成对 Meta 属性的配置。
        /// 
        /// 注意：
        /// * 为了给当前类的子类也运行同样的配置，这个方法可能会被调用多次。
        /// </summary>
        protected internal virtual void ConfigView() { }
    }

    /// <summary>
    /// 泛型版本的实体配置基类
    /// </summary>
    public abstract class WebViewConfig<TEntity> : WebViewConfig
    {
        protected internal override sealed Type EntityType
        {
            get { return typeof(TEntity); }
        }
    }
}