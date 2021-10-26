/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121105 11:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121105 11:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rafy.MetaModel;
using Rafy.WPF;
using Rafy.Domain.ORM;
using Rafy;
using Rafy.Domain;

namespace Rafy.DevTools.CodeGenerator
{
    /// <summary>
    /// 本窗口用于通过查询 SQL 生成相应的实体类。
    /// </summary>
    public partial class QueryEntityGenerator : UserControl
    {
        public QueryEntityGenerator()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string error = null;
            var entityName = txtEntityName.Text;
            var dbName = txtDbName.Text;
            var sql = txtSql.Text;
            if (string.IsNullOrEmpty(entityName))
            {
                error = "实体名称没有填写";
            }
            if (string.IsNullOrEmpty(dbName))
            {
                error = "数据库没有填写";
            }
            if (string.IsNullOrEmpty(sql))
            {
                error = "查询 SQL 没有填写";
            }
            if (!string.IsNullOrEmpty(error))
            {
                App.MessageBox.Show(error.Translate());
                return;
            }

            var properties = ParseProperties(sql, dbName);

            txtEntity.Text = ConvertEntityString(properties, entityName);
            txtViewConfig.Text = ConvertViewConfigString(properties, entityName);
        }

        private static List<PropertyInfo> ParseProperties(string sql, string dbName)
        {
            var properties = new List<PropertyInfo>();

            using (var dba = DbAccesserFactory.Create(dbName))
            {
                var table = dba.QueryDataTable(sql);
                var columns = table.Columns;
                foreach (DataColumn column in columns)
                {
                    if (!column.ColumnName.EqualsIgnoreCase(Entity.IdProperty.Name))
                    {
                        var property = new PropertyInfo
                        {
                            Name = column.ColumnName,
                            Type = column.DataType
                        };
                        properties.Add(property);
                    }
                }
            }

            var matches = Regex.Matches(sql, @"(?<name>[\w_]+)\s*,?\s*--(?<comment>.+)$", RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                var name = match.Groups["name"].Value;
                var comment = match.Groups["comment"].Value;
                var property = properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(name));
                if (property != null)
                {
                    property.Comment = comment.Trim();
                }
            }

            return properties;
        }

        private string ConvertEntityString(List<PropertyInfo> properties, string entityName)
        {
            var res = new StringBuilder();

            foreach (var property in properties)
            {
                var propertyString = ConvertToPropertyString(entityName, property);

                if (res.Length > 0)
                {
                    res.AppendLine();
                }

                res.AppendLine(propertyString);
            }

            return res.ToString();
        }

        private static string ConvertToPropertyString(string entityName, PropertyInfo property)
        {
            string typeName = ConvertTypeName(property);

            var propertyString = string.Format(
@"public static readonly Property<{0}> {1}Property = P<{2}>.Register(e => e.{1});
",
typeName, property.Name, entityName);
            if (!string.IsNullOrEmpty(property.Comment))
            {
                propertyString += string.Format(
@"/// <summary>
/// {0}
/// </summary>
", property.Comment);
            }
            propertyString += string.Format(
@"public {0} {1}
{{
    get {{ return this.GetProperty({1}Property); }}
    set {{ this.SetProperty({1}Property, value); }}
}}", typeName, property.Name);

            return propertyString;
        }

        private static string ConvertTypeName(PropertyInfo property)
        {
            Type type = property.Type;
            if (type == typeof(decimal))
            {
                type = typeof(double);
            }

            string typeName = type.Name;
            if (type != typeof(string))
            {
                if (type == typeof(int))
                {
                    typeName = "int";
                }
                else if (type == typeof(double))
                {
                    typeName = "double";
                }
                else if (type == typeof(bool))
                {
                    typeName = "bool";
                }
                typeName += "?";
            }
            else
            {
                typeName = "string";
            }
            return typeName;
        }

        private string ConvertViewConfigString(List<PropertyInfo> properties, string entityName)
        {
            var res = new StringBuilder();

            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(property.Comment))
                {
                    var propertyString = string.Format(
@"View.Property({0}.{1}Property).HasLabel(""{2}"").ShowIn(ShowInWhere.List);"
, entityName, property.Name, property.Comment);

                    res.AppendLine(propertyString);
                }
            }

            return res.ToString();
        }

        private class PropertyInfo
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public string Comment { get; set; }
        }
    }
}