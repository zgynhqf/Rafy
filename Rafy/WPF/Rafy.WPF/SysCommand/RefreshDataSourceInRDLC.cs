/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120905 16:06
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120905 16:06
 * 
*******************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.Reflection;

namespace Rafy.WPF.Command
{
    [Command(Label = "刷新 RDLC 字段", ToolTip = "根据当前数据生成或刷新 RDLC 文件中的数据源", GroupType = CommandGroupType.System, ImageName = "Refresh.bmp")]
    public class RefreshDataSourceInRDLC : ClientCommand<ReportLogicalView>
    {
        #region 常量定义

        public static readonly string ReportDataSouceName = "Rafy";
        private static readonly XNamespace n = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";
        private static readonly XNamespace rd = "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner";
        private static readonly XNamespace cl = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition";
        private static readonly string EmptyRDLC = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition"" xmlns:cl=""http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition"" xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"">
  <AutoRefresh>0</AutoRefresh>
  <ReportSections>
    <ReportSection>
      <Body>
        <Height>3.20833in</Height>
        <Style />
      </Body>
      <Width>6.47917in</Width>
      <Page>
        <PageHeight>29.7cm</PageHeight>
        <PageWidth>21cm</PageWidth>
        <LeftMargin>2cm</LeftMargin>
        <RightMargin>2cm</RightMargin>
        <TopMargin>2cm</TopMargin>
        <BottomMargin>2cm</BottomMargin>
        <ColumnSpacing>0.13cm</ColumnSpacing>
        <Style />
      </Page>
    </ReportSection>
  </ReportSections>
</Report>
";

        #endregion

        public override bool CanExecute(ReportLogicalView view)
        {
            return view.Data != null;
        }

        public override void Execute(ReportLogicalView view)
        {
            var btn = App.MessageBox.Show("确定生成或刷新当前报表的数据源？".Translate(), MessageBoxButton.YesNo);
            if (btn != MessageBoxResult.Yes) return;

            var reportPath = view.Control.ReportPath;

            XDocument rdlc = FindOrCreateDocument(reportPath);
            var root = rdlc.Root;

            AddReportDataSources(root);

            var dataSets = FindOrCreate(root, n + "DataSets");

            foreach (var ds in view.CustomDataSources)
            {
                AddDataSet(dataSets, ds.Value.GetRepository().EntityType);
            }

            var data = view.Data;
            var em = data.GetRepository().EntityMeta;
            AddDataSet(dataSets, em.EntityType);

            //如果数据是一个单一聚合实体，则把它及它的第一层聚合子对象作为数据源加入。
            if (data is Entity)
            {
                foreach (var childProperty in em.ChildrenProperties)
                {
                    AddDataSet(dataSets, childProperty.ChildType.EntityType);
                }
            }

            try
            {
                rdlc.Save(reportPath);
                view.RefreshControl();

                reportPath = Path.GetFullPath(reportPath);
                var res = App.MessageBox.Show(@"刷新数据源成功，是否拷贝路径到剪贴板（方便在设计器中直接打开）：".Translate() + reportPath, MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        Clipboard.SetText(reportPath);
                    }
                    catch (Exception ex)
                    {
                        App.MessageBox.Show("复制到剪贴板出错：\r\n".Translate() + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                App.MessageBox.Show("保存出错：\r\n".Translate() + ex.Message);
            }
        }

        private static XDocument FindOrCreateDocument(string reportPath)
        {
            XDocument rdlc = null;
            if (File.Exists(reportPath))
            {
                rdlc = XDocument.Load(reportPath);
            }
            else
            {
                rdlc = XDocument.Parse(EmptyRDLC);
            }
            return rdlc;
        }

        /// <summary>
        /// 为报表文件添加 DataSource 节点。
        /// </summary>
        /// <param name="root"></param>
        private static void AddReportDataSources(XElement root)
        {
            var dataSources = FindOrCreate(root, n + "DataSources");

            var reportDataSources = dataSources.Elements(n + "DataSource")
                .FirstOrDefault(e => e.Attribute("Name").Value == ReportDataSouceName);
            if (reportDataSources == null)
            {
                reportDataSources = new XElement(n + "DataSource", new XAttribute("Name", ReportDataSouceName),
                    new XElement(n + "ConnectionProperties",
                        new XElement(n + "DataProvider", "System.Data.DataSet"),
                        new XElement(n + "ConnectString", "/* Local Connection */")
                        ),
                    new XElement(rd + "DataSourceID", Guid.NewGuid().ToString())
                    );
                dataSources.Add(reportDataSources);
            }
        }

        private static void AddDataSet(XElement dataSets, Type entityType)
        {
            //删除现有的 DataSet
            var entitySet = dataSets.Elements(n + "DataSet")
                .FirstOrDefault(e => e.Attribute("Name").Value == entityType.Name);
            if (entitySet != null) { entitySet.Remove(); }

            entitySet = new XElement(n + "DataSet", new XAttribute("Name", entityType.Name),
                new XElement(n + "Query",
                    new XElement(n + "DataSourceName", ReportDataSouceName),
                    new XElement(n + "CommandText", "/* Local Query */")
                ));
            dataSets.Add(entitySet);
            var fields = new XElement(n + "Fields");
            entitySet.Add(fields);
            entitySet.Add(new XElement(rd + "DataSetInfo",
                new XElement(rd + "DataSetName", ReportDataSouceName),
                new XElement(rd + "TableName", entityType.Name),
                new XElement(rd + "ObjectDataSourceType", entityType.AssemblyQualifiedName)
                ));

            var meta = UIModel.Views.CreateBaseView(entityType);

            AddFields(fields, meta);
        }

        private static void AddFields(XElement fields, EntityViewMeta meta)
        {
            foreach (WPFEntityPropertyViewMeta property in meta.EntityProperties)
            {
                var label = FormatPropertyLabel(property);
                var propertyType = property.PropertyMeta.Runtime.PropertyType;

                if (TypeHelper.IgnoreNullable(propertyType).IsEnum) propertyType = typeof(string);

                fields.Add(new XElement(n + "Field", new XAttribute("Name", label),
                    new XElement(n + "DataField", property.Name),
                    new XElement(rd + "TypeName", propertyType.FullName)
                    ));
            }
        }

        private static string FormatPropertyLabel(WPFEntityPropertyViewMeta property)
        {
            //var label = property.Label;
            //if (!string.IsNullOrWhiteSpace(label))
            //{
            //    //尝试把一些已知的字符替换掉。
            //    label = Regex.Replace(label, @"[\(\)（）｛｝“”""\\/-]", string.Empty);
            //    if (label.Length > 0)
            //    {
            //        return property.Name + "__" + label;
            //    }
            //}

            return property.Name;
        }

        private static XElement FindOrCreate(XElement parent, XName name)
        {
            var node = parent.Element(name);
            if (node == null)
            {
                node = new XElement(name);
                parent.Add(node);
            }

            return node;
        }
    }
}