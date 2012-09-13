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
using OEA.Library;
using OEA.MetaModel.View;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 报表视图
    /// </summary>
    public class ReportObjectView : WPFObjectView
    {
        internal protected ReportObjectView(EntityViewMeta meta) : base(meta) { }

        public new ReportHost Control
        {
            get { return base.Control as ReportHost; }
        }

        public new IEntityOrList Data
        {
            get { return base.Data as IEntityOrList; }
            set { base.Data = value; }
        }

        #region 数据源

        private Dictionary<string, EntityList> _customDataSources = new Dictionary<string, EntityList>();

        internal Dictionary<string, EntityList> CustomDataSources
        {
            get { return this._customDataSources; }
        }

        /// <summary>
        /// 为报表添加指定的数据源。
        /// </summary>
        /// <param name="dataSourceName">对应在报表中的数据源名称</param>
        /// <param name="data">实体数据。</param>
        public void AddReportDataSource(string dataSourceName, EntityList data)
        {
            this._customDataSources[dataSourceName] = data;
        }

        /// <summary>
        /// 为报表生成数据源。
        /// </summary>
        public void GenerateReportDataSource()
        {
            var dataSources = this.Control.DataSources;
            var data = this.Data;

            //清空数据源，
            dataSources.Clear();

            //先加入自定义数据源。
            foreach (var item in this._customDataSources)
            {
                dataSources.Add(new ReportDataSource(item.Key, item.Value));
            }

            var em = this.Meta.EntityMeta;

            //如果数据是一个单一聚合实体，则把它及它的第一层聚合子对象作为数据源加入。
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
            var entityList = data as EntityList;
            if (entityList != null)
            {
                dataSources.Add(new ReportDataSource(em.EntityType.Name, entityList));
            }
        }

        #endregion

        protected override void OnDataChanged()
        {
            base.OnDataChanged();

            this.GenerateReportDataSource();

            this.RefreshControl();
        }

        public override Entity Current
        {
            get { return null; }
            set { }
        }

        public override void RefreshCurrentEntity() { }

        protected override void RefreshControlCore()
        {
            this.Control.RefreshReportIfReady();
        }
    }
}