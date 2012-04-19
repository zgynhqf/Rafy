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
using Itenso.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using OEA.ManagedProperty;


namespace OEA.Module.WPF
{
    /// <summary>
    /// 查询面板
    /// </summary>
    public abstract class QueryObjectView : DetailObjectView
    {
        internal QueryObjectView(EntityViewMeta evm)
            : base(evm) { }

        /// <summary>
        /// 查询后的结果使用的 View
        /// </summary>
        public IEnumerable<ObjectView> ResultViews
        {
            get
            {
                var res = SurrounderType.Result.GetDescription();
                foreach (var r in this.Relations)
                {
                    if (r.SurrounderType == res) yield return r.View;
                }
            }
        }

        public new Criteria Current
        {
            get { return base.Current as Criteria; }
            set { base.Current = value; }
        }

        /// <summary>
        /// 为这个查询面板构造并使用一个新的查询对象。
        /// </summary>
        /// <returns></returns>
        public void AttachNewQueryObject()
        {
            var criteria = RF.Create(this.EntityType).New() as Criteria;
            if (criteria == null) throw new InvalidProgramException("导航类需要继承自 Criteria 类。");

            this.Data = criteria;
        }

        /// <summary>
        /// 命令这个查询面板执行查询命令
        /// </summary>
        public virtual void TryExecuteQuery()
        {
            if (this.CanQuery())
            {
                foreach (var resultView in this.ResultViews)
                {
                    this.FireQuery(resultView);
                }
            }
        }

        /// <summary>
        /// 主动为某个特定的视图使用本导航面板来执行查询。
        /// </summary>
        /// <param name="resultView"></param>
        public void TryExecuteQuery(ObjectView resultView)
        {
            if (this.CanQuery())
            {
                this.FireQuery(resultView);
            }
        }

        private void FireQuery(ObjectView resultView)
        {
            //导航面板的查询使用隐式查询。
            resultView.DataLoader.LoadDataAsync(
                () => RF.Create(resultView.EntityType).__GetListImplicitly(this.Current)
                );
        }

        private bool CanQuery()
        {
            return this.Current.ValidationRules.Validate().Count == 0;
        }

        /// <summary>
        /// 使用这个查询面板中的查询对象数据，
        /// 给 newEntity 的外键设置值。
        /// </summary>
        /// <param name="newEntity"></param>
        public void SetReferenceEntity(Entity newEntity)
        {
            if (newEntity == null) throw new ArgumentNullException("newEntity");

            var criteria = this.Current;
            if (criteria != null)
            {
                var destIndicators = newEntity.FindRepository().GetAvailableIndicators();

                //对每一个导航的实体引用属性，都给 referenceEntity 赋相应的值
                //只有导航查询实体中的引用实体ID属性名和被查询实体的引用实体ID属性名相同时，才能设置
                foreach (var naviProperty in this.Meta.EntityProperties)
                {
                    //naviProperty 是一个引用实体属性
                    var refMeta = naviProperty.ReferenceViewInfo;
                    if (refMeta != null)
                    {
                        //被查询实体的引用实体ID属性名与 naviProperty 的名称相同时，才能设置。
                        foreach (var mp in destIndicators)
                        {
                            var destRefProperty = mp as IRefProperty;
                            if (destRefProperty != null)
                            {
                                var idProperty = destRefProperty.GetMeta(newEntity).IdProperty;
                                if (idProperty == naviProperty.Name)
                                {
                                    //读值
                                    var lazyRef = criteria.GetLazyRef(naviProperty.PropertyMeta.ManagedProperty.CastTo<IRefProperty>());
                                    var value = lazyRef.Entity;

                                    //写值到新对象中
                                    newEntity.GetLazyRef(destRefProperty).Entity = value;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}