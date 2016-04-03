/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111111
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111111
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

using Rafy.MetaModel.View;

namespace Rafy.MetaModel
{
    public abstract class EntityConfigBase
    {
        /// <summary>
        /// 所处插件的启动级别。
        /// </summary>
        internal int PluginIndex { get; set; }

        /// <summary>
        /// 继承层次
        /// </summary>
        internal int InheritanceCount { get; set; }

        /// <summary>
        /// 本实体配置对应的实体类
        /// </summary>
        protected internal abstract Type EntityType { get; }
    }

    /// <summary>
    /// 实体配置基类
    /// </summary>
    public abstract class EntityConfig : EntityConfigBase
    {
        private EntityMeta _meta;

        protected internal EntityMeta Meta
        {
            get
            {
                if (this._meta == null)
                {
                    this._meta = CommonModel.Entities.Get(this.EntityType);
                }

                return this._meta;
            }
            internal set { this._meta = value; }
        }

        internal void UseDefaultMeta()
        {
            this._meta = null;
        }

        /// <summary>
        /// 子类重写此方法，并完成对 Meta 属性的配置。
        /// 
        /// 注意：
        /// * 为了给当前类的子类也运行同样的配置，这个方法可能会被调用多次。
        /// </summary>
        protected internal virtual void ConfigMeta() { }

        /// <summary>
        /// 子类重写此方法，并完成对实体验证规则的配置。
        /// 
        /// 注意：
        /// * 为了给当前类的子类也运行同样的配置，这个方法可能会被调用多次。
        /// </summary>
        protected internal virtual void AddValidations(IValidationDeclarer rules) { }

        //暂时去掉 CustomizeMeta、CustomizeView 方法。
        ///// <summary>
        ///// 客户化该类的元数据
        ///// 
        ///// 注意：
        ///// 不同于 ConfigView ConfigMeta 这两个方法，这个方法只会运行一次。
        ///// 其机理类似于在 Module 的 ModelOperation 中写的代码。只是在这提供此接口进行更方便的 配置。
        ///// 这个方法中的代码只会影响 AppModel.Entities.FindViewMeta(Type entityType) 查询出来的实体元数据，
        ///// 而不会影响该类的子类、以及其它聚合树中该类对应的元数据。
        ///// 如果要配置其它聚合树，请不要使用 View 及 Meta 属性，而是使用 
        ///// AppModel.Entities.FindViewMeta(Type entityType, Type rootType) 等 API 自行查询元数据并进行客户化。
        ///// </summary>
        //protected internal virtual void CustomizeMeta() { }

        //protected internal virtual void CustomizeView() { }
    }

    /// <summary>
    /// 泛型版本的实体配置基类
    /// </summary>
    public abstract class EntityConfig<TEntity> : EntityConfig
    {
        protected internal override sealed Type EntityType
        {
            get { return typeof(TEntity); }
        }
    }
}