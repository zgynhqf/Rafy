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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Command;
using Rafy.MetaModel.Attributes;
using Rafy.Reflection;
using Rafy.Utils;
using Rafy.ManagedProperty;

namespace Rafy.WPF
{
    /// <summary>
    /// 导出某一类模型列表到Excel的命令。
    /// </summary>
    /// <typeparam name="TModel">
    /// 被导出的模型的类型
    /// </typeparam>
    [Command(Label = "导出Excel", GroupType = CommandGroupType.System, ImageName = "Excel.bmp")]
    public class ExportToExcelCommand : ListViewCommand
    {
        /// <summary>
        /// 要显示的所有属性。
        /// </summary>
        private List<WPFEntityPropertyViewMeta> _properties;

        private Entity _currentRow;

        /// <summary>
        /// 当前正在读取的行对象。
        /// </summary>
        protected Entity CurrentRow
        {
            get { return this._currentRow; }
        }

        public override bool CanExecute(ListLogicalView view)
        {
            if (base.CanExecute(view))
            {
                var data = this.FindModels(view);
                return data != null && data.Count > 0;
            }
            return false;
        }

        public override void Execute(ListLogicalView view)
        {
            //据说这个也不错，有时间的时候可以尝试下：http://npoi.codeplex.com/

            this._properties = new List<WPFEntityPropertyViewMeta>();
            this.DefineTable(this._properties, view.Meta);

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
                        App.MessageBox.Show("扩展名不对！".Translate());
                        return;
                    }

                    var table = this.FindTableData(view);

                    var saver = ExcelHelper.CreateSaver();
                    saver.SaveToFile(table, fileName);

                    Process.Start(fileName);
                }
                catch (Exception ex)
                {
                    App.MessageBox.Show("保存出错。".Translate() + Environment.NewLine + ex.Message.Translate());
                }
            }
        }

        /// <summary>
        /// 从view中把所有实体对象的数据转换为需要导出Excel的表格数据。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected DataTable FindTableData(ListLogicalView view)
        {
            DataTable table = new DataTable();

            var tableColumns = new Dictionary<string, WPFEntityPropertyViewMeta>();

            //使用定义来定义表的列。
            foreach (var property in this._properties)
            {
                var label = property.Label;

                //使用空格来防止出现空列名及同名。
                if (string.IsNullOrWhiteSpace(label)) { label = " "; }
                while (tableColumns.ContainsKey(label))
                {
                    label += " ";
                }

                tableColumns[label] = property;

                table.Columns.Add(label);
            }

            //导出实体数据。
            var list = this.FindModels(view);
            foreach (var model in list)
            {
                this._currentRow = model;
                var row = table.NewRow();

                foreach (var kv in tableColumns)
                {
                    row[kv.Key] = this.ReadProperty(kv.Value);
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// 定义列到属性的映射。
        /// </summary>
        /// <param name="properties">
        /// Key:列名，将会使用这个列名来显示。
        /// Value:列名在对象中所对应的属性名。
        /// </param>
        protected virtual void DefineTable(List<WPFEntityPropertyViewMeta> properties, EntityViewMeta evm)
        {
            foreach (WPFEntityPropertyViewMeta property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(ShowInWhere.List))
                {
                    properties.Add(property);
                }
            }
        }
        /// <summary>
        /// 从view中找出所有的模型。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual IList<Entity> FindModels(ListLogicalView view)
        {
            return view.Data;
        }

        protected virtual string ReadProperty(WPFEntityPropertyViewMeta property)
        {
            var data = string.Empty;

            try
            {
                if (property.PropertyMeta.ManagedProperty is IRefEntityProperty)
                {
                    var displayPath = property.DisplayPath();
                    var value = ObjectHelper.GetPropertyValue(this._currentRow, displayPath);
                    if (value != null) data = value.ToString();
                }
                else
                {
                    var mp = property.PropertyMeta.ManagedProperty;
                    if (mp != null)
                    {
                        var value = this._currentRow.GetProperty(mp);
                        if (value != null)
                        {
                            if (TypeHelper.IgnoreNullable(mp.PropertyType).IsEnum)
                            {
                                data = EnumViewModel.EnumToLabel((Enum)value).Translate();
                            }
                            else
                            {
                                data = value.ToString();
                            }
                        }
                    }
                }
            }
            catch
            {
                //尽量尝试读取值，读取出错，则忽略错误。
            }

            return data;
        }
    }
}
