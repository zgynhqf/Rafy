/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 导出某一类模型列表到Excel的命令。
    /// </summary>
    /// <typeparam name="TModel">
    /// 被导出的模型的类型
    /// </typeparam>
    [Command(Label = "导出Excel", GroupType = CommandGroupType.View)]
    public class ExportToExcelCommand : ListViewCommand
    {
        /// <summary>
        /// key: 列显示名称
        /// value：对应的属性名称。可能是跨引用实体对象的。
        /// </summary>
        private IDictionary<string, string> _columnsToProperties;

        private Entity _currentModel;

        protected Entity CurrentModel
        {
            get { return this._currentModel; }
        }

        public override bool CanExecute(ListObjectView view)
        {
            if (base.CanExecute(view))
            {
                var data = this.FindModels(view);
                return data != null && data.Count > 0;
            }
            return false;
        }

        public override void Execute(ListObjectView view)
        {
            //据说这个也不错，有时间的时候可以尝试下：http://npoi.codeplex.com/

            this._columnsToProperties = new Dictionary<string, string>();
            this.DefineTable(this._columnsToProperties, view.Meta);

            var dialog = new SaveFileDialog
            {
                FileName = view.Meta.Label + ".xls",
                Filter = "excel files (*.xls)|*.xls"
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    string fileName = dialog.FileName;
                    if (string.Compare(Path.GetExtension(fileName), ".xls", true) != 0)
                    {
                        App.MessageBox.Show("扩展名不对！");
                        return;
                    }

                    var table = this.FindTableData(view);

                    var saver = ExcelHelper.CreateSaver();
                    saver.SaveToFile(table, fileName);

                    Process.Start(fileName);
                }
                catch
                {
                    App.MessageBox.Show("保存出错。");
                }
            }
        }

        /// <summary>
        /// 定义列到属性的映射。
        /// </summary>
        /// <param name="columnPropertyMappings">
        /// Key:列名，将会使用这个列名来显示。
        /// Value:列名在对象中所对应的属性名。
        /// </param>
        protected virtual void DefineTable(IDictionary<string, string> columnPropertyMappings, EntityViewMeta evm)
        {
            foreach (var property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(ShowInWhere.List))
                {
                    columnPropertyMappings[property.Label] = property.DisplayPath();
                }
            }
        }
        /// <summary>
        /// 从view中找出所有的模型。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual IList<Entity> FindModels(ListObjectView view)
        {
            return view.Data;
        }

        protected virtual string ReadDataByColumn(string columnName)
        {
            var propertyName = this._columnsToProperties[columnName];
            return this.ReadDataByProperty(propertyName);
        }

        protected virtual string ReadDataByProperty(string propertyName)
        {
            try
            {
                var data = this._currentModel.GetPropertyValue(propertyName).ToString();
                return data;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 从view中把所有实体对象的数据转换为需要导出Excel的表格数据。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected DataTable FindTableData(ListObjectView view)
        {
            DataTable table = new DataTable();

            //define table columns
            var columns = this._columnsToProperties.Keys.ToArray();
            foreach (var column in columns)
            {
                table.Columns.Add(column);
            }

            //models' data
            var list = this.FindModels(view);
            foreach (var model in list)
            {
                this._currentModel = model;
                var row = table.NewRow();

                foreach (var column in columns)
                {
                    row[column] = this.ReadDataByColumn(column);
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
