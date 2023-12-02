/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Reflection;
using Rafy.WPF.Command;

namespace Rafy.WPF
{
    /// <summary>
    /// 查询面板
    /// </summary>
    public abstract class QueryLogicalView : DetailLogicalView
    {
        public static readonly string ResultSurrounderType = "result";

        internal QueryLogicalView(WPFEntityViewMeta evm) : base(evm) { }

        /// <summary>
        /// 查询后的结果视图。
        /// </summary>
        public IEnumerable<LogicalView> ResultViews
        {
            get { return this.Relations.FindAll(ResultSurrounderType); }
        }

        /// <summary>
        /// 查询面板的当前实体是一个 <see cref="Criteria"/> 对象。
        /// </summary>
        public new Criteria Current
        {
            get { return base.Current as Criteria; }
            set { base.Current = value; }
        }

        /// <summary>
        /// 为这个查询面板构造并使用一个新的查询对象。
        /// </summary>
        /// <returns></returns>
        public void AttachNewCriteria()
        {
            var criteria = Entity.New(this.EntityType) as Criteria;
            if (criteria == null) throw new InvalidProgramException("导航类需要继承自 Criteria 类。");

            this.Data = criteria;
        }

        /// <summary>
        /// 让这个查询面板执行查询。
        /// <remarks>如果没有查询条件对象，或者查询对象验证不通过，则不能查询。</remarks>
        /// </summary>
        public bool TryExecuteQuery()
        {
            if (this.CanQuery())
            {
                foreach (var resultView in this.ResultViews)
                {
                    this.FireQuery(resultView);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 主动为某个特定的视图使用本导航面板来执行查询。
        /// <remarks>如果没有查询条件对象，或者查询对象验证不通过，则不能查询。</remarks>
        /// </summary>
        /// <param name="resultView"></param>
        public bool TryExecuteQuery(LogicalView resultView)
        {
            if (this.CanQuery())
            {
                this.FireQuery(resultView);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 执行查询操作。
        /// <remarks>可能执行本地查询，也可能是远程查询。</remarks>
        /// </summary>
        /// <param name="resultView"></param>
        private void FireQuery(LogicalView resultView)
        {
            var args = new QueryEventArgs { ResultView = resultView };

            this.OnQuerying(args);

            //如果是本地查询模式，则尝试为 ListLogicalView 进行过滤。
            var criteria = this.Current;
            if (criteria.CanLocalFilter)
            {
                var listView = resultView as ListLogicalView;
                if (listView != null)
                {
                    listView.Filter = criteria.LocalFilter;

                    this.OnQueryCompleted(args);
                    return;
                }
            }

            //需要复制一个查询对象，这样可以保存下当前的查询值，
            //否则会造成 ReloadDataAsync 方法查询时不是使用当前的这些值来查询。
            var queryCriteria = Entity.New(this.EntityType);
            queryCriteria.Clone(this.Current, CloneOptions.NewComposition());

            //导航面板的查询使用隐式查询。
            resultView.DataLoader.LoadDataAsync(
                () =>
                {
                    var repo = RF.Find(resultView.EntityType);

                    return MethodCaller.CallMethod(repo, EntityConvention.GetByCriteriaMethod, criteria) as IDomainComponent;
                },
                () => this.OnQueryCompleted(args)
                );
        }

        #region 事件

        /// <summary>
        /// 查询/过滤前事件
        /// </summary>
        public event EventHandler<QueryEventArgs> Querying;

        protected virtual void OnQuerying(QueryEventArgs e)
        {
            var handler = this.Querying;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// 查询/过滤完成，数据已达本地
        /// </summary>
        public event EventHandler<QueryEventArgs> QueryCompleted;

        protected virtual void OnQueryCompleted(QueryEventArgs e)
        {
            var handler = this.QueryCompleted;
            if (handler != null) handler(this, e);
        }

        #endregion

        /// <summary>
        /// 返回当前是否可以进行查询。
        /// <remarks>如果没有查询条件对象，或者查询对象验证不通过，则不能查询。</remarks>
        /// </summary>
        /// <returns></returns>
        private bool CanQuery()
        {
            var current = this.Current;
            if (current != null)
            {
                var brokenRules = current.Validate();
                return brokenRules.Count == 0;
            }
            return false;
        }

        /// <summary>
        /// 使用这个查询面板中的查询对象数据，
        /// 给实体的外键设置值。
        /// </summary>
        /// <param name="newEntity">需要写入值的新创建的实体</param>
        internal void SyncRefEntities(Entity newEntity)
        {
            if (newEntity == null) throw new ArgumentNullException("newEntity");

            var criteria = this.Current;
            if (criteria != null)
            {
                var destProperties = newEntity.PropertiesContainer.GetAvailableProperties();

                //对每一个导航的实体引用属性，都给 referenceEntity 赋相应的值
                //只有导航查询实体中的引用实体ID属性名和被查询实体的引用实体ID属性名相同时，才能设置
                foreach (var naviProperty in this.Meta.EntityProperties)
                {
                    //如果是一个引用键属性
                    if (RefPropertyHelper.IsRefKeyProperty(naviProperty.PropertyMeta.ManagedProperty, out var criteriaRef))
                    {
                        foreach (var mp in destProperties)
                        {
                            var entityRef = mp as IRefEntityProperty;

                            //约定：被查询实体的引用实体Key属性名与 naviProperty 的名称相同，并且二者类型一致时，才能被设置。
                            if (entityRef != null
                                && entityRef.RefKeyProperty.Name == criteriaRef.Name
                                && entityRef.RefEntityType == criteriaRef.RefEntityType
                                )
                            {
                                //读值，并写值到新对象中。
                                var value = criteria.GetRefEntity(criteriaRef.RefEntityProperty);
                                newEntity.SetRefEntity(entityRef, value);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 查询面板事件参数
    /// </summary>
    public class QueryEventArgs : EventArgs
    {
        /// <summary>
        /// 正在查询的结果视图。
        /// </summary>
        public LogicalView ResultView { get; internal set; }
    }
}