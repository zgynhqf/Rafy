using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.WPF.Command;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Data;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 导出某一类模型列表到Excel的命令。
    /// </summary>
    /// <typeparam name="TModel">
    /// 被导出的模型的类型
    /// </typeparam>
    public abstract class ExportToExcelCommand<TModel> : ListViewCommand
    {
        private IDictionary<string, string> _columnsToProperties;

        private TModel _currentModel;

        protected TModel CurrentModel
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
            this._columnsToProperties = new Dictionary<string, string>();
            this.DefineTable(this._columnsToProperties, view.Meta);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "excel files (*.xls)|*.xls";
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    string fileName = dialog.FileName;
                    if (string.Compare(Path.GetExtension(fileName), ".xls", true) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("扩展名不对！");
                        return;
                    }

                    var table = this.FindTableData(view);

                    var saver = ExcelHelper.CreateSaver();
                    saver.SaveToFile(table, fileName);

                    Process.Start(fileName);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("保存出错。");
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
            var modelType = typeof(TModel);
            foreach (var property in evm.EntityProperties)
            {
                if (property.CanShowIn(ShowInWhere.List))
                {
                    string propertyName = property.Name;

                    //处理外键关系
                    var rvi = property.ReferenceViewInfo;
                    if (rvi != null)
                    {
                        var ri = rvi.ReferenceInfo;
                        if (ri.Type == ReferenceType.Normal)
                        {
                            propertyName = ri.RefEntityProperty + '.'
                                + rvi.RefTypeDefaultView.TitleProperty.Name;
                        }
                    }
                    columnPropertyMappings.Add(property.Label, propertyName);
                }
            }
        }
        /// <summary>
        /// 从view中找出所有的模型。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual IList FindModels(ListObjectView view)
        {
            return view.Data as IList;
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

            //the first title row
            //var titleRow = table.NewRow();
            //foreach (var column in columns)
            //{
            //    titleRow[column] = column;
            //}
            //table.Rows.Add(titleRow);

            //models' data
            var list = this.FindModels(view);
            foreach (TModel model in list)
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
