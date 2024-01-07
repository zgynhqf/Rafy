/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120903 21:27
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120903 21:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.MetaModel;

namespace Rafy.WPF
{
    /// <summary>
    /// 报表视图
    /// </summary>
    public class ReportLogicalView : LogicalView
    {
        internal protected ReportLogicalView(WPFEntityViewMeta meta) : base(meta) { }

        public new ReportHost Control
        {
            get { return base.Control as ReportHost; }
        }

        #region 数据源

        private Dictionary<string, IDomainComponent> _customDataSources = new Dictionary<string, IDomainComponent>(0);

        internal Dictionary<string, IDomainComponent> CustomDataSources
        {
            get { return this._customDataSources; }
        }

        /// <summary>
        /// 为报表添加指定的数据源。
        /// </summary>
        /// <param name="dataSourceName">对应在报表中的数据源名称</param>
        /// <param name="data">实体数据。</param>
        public void AddReportDataSource(string dataSourceName, IDomainComponent data)
        {
            this._customDataSources[dataSourceName] = data;
        }

        /// <summary>
        /// 为报表设置指定名称的数据源。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParameter(string name, string value)
        {
            //设置参数值
            this.Control.SetParameter(name, value);
        }

        /// <summary>
        /// 为报表生成数据源。
        /// </summary>
        private void GenerateReportDataSource()
        {
            var control = this.Control;

            //清空数据源，准备重新生成整个数据源。
            var dataSources = control.DataSources;
            dataSources.Clear();

            this.AddDataSource(dataSources);

            this.AddCustomDataSource(dataSources);
        }

        /// <summary>
        /// 根据后台数据来生成数据源。
        /// </summary>
        /// <param name="dataSources"></param>
        private void AddDataSource(ReportDataSourceCollection dataSources)
        {
            var em = this.Meta.EntityMeta;

            //如果数据是一个单一聚合实体，则把它及它的第一层聚合子对象作为数据源加入。
            var data = this.Data;
            var entity = data as Entity;
            if (entity != null)
            {
                dataSources.Add(new ReportDataSource(em.EntityType.Name, new BindingSource(entity, null)));
                foreach (var childProperty in em.ChildrenProperties)
                {
                    dataSources.Add(new ReportDataSource(
                        childProperty.ChildType.EntityType.Name,
                        new BindingSource(entity, childProperty.Name)
                        ));
                }
                return;
            }

            //如果数据是一个实体列表，则直接把这个列表作为报表的数据源。
            var entityList = data as IEntityList;
            if (entityList != null)
            {
                dataSources.Add(new ReportDataSource(em.EntityType.Name, entityList));
            }
        }

        /// <summary>
        /// 加入自定义数据源
        /// </summary>
        /// <param name="dataSources"></param>
        private void AddCustomDataSource(ReportDataSourceCollection dataSources)
        {
            foreach (var item in this._customDataSources)
            {
                var entityOrList = item.Value;

                var itemEntity = entityOrList as Entity;
                if (itemEntity != null)
                {
                    dataSources.Add(new ReportDataSource(itemEntity.GetType().Name, new BindingSource(itemEntity, null)));
                }

                //如果数据是一个实体列表，则直接把这个列表作为报表的数据源。
                var itemEntityList = entityOrList as IEntityList;
                if (itemEntityList != null)
                {
                    dataSources.Add(new ReportDataSource(itemEntityList.EntityType.Name, itemEntityList));
                }

                dataSources.Add(new ReportDataSource(item.Key, item.Value));
            }
        }

        #endregion

        protected override void OnDataChanged()
        {
            base.OnDataChanged();

            //实体变化时，为报表重新生成数据源。
            this.GenerateReportDataSource();

            //刷新报表控件。
            this.RefreshControl();
        }

        public override Entity Current
        {
            get { return null; }
            set { }
        }

        protected override void RefreshCurrentEntityCore() { }

        protected override void RefreshControlCore()
        {
            this.Control.RefreshReportIfReady();
        }

        protected override void SetControlReadOnly(ReadOnlyStatus value)
        {
            //报表永远只读。
        }
    }
}