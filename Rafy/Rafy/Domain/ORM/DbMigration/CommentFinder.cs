/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150926
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150926 19:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rafy.ManagedProperty;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 实体类、实体属性的注释查找器。
    /// </summary>
    class CommentFinder
    {
        private Dictionary<Assembly, XDocument> _store = new Dictionary<Assembly, XDocument>();

        /// <summary>
        /// 额外的一些属性注释的字典。
        /// Key:属性名。
        /// Value:注释值。
        /// </summary>
        internal Dictionary<string, string> AdditionalPropertiesComments { get; set; }

        public string TryFindComment(Type type)
        {
            return this.TryFindComment(type.Assembly, "T:" + type.FullName);
        }

        public string TryFindComment(IManagedProperty property)
        {
            var declareType = property.DeclareType;
            var propertyKey = string.Empty;

            if (property.IsExtension)
            {
                //扩展属性的注释是写在静态属性上的。
                propertyKey = "F:" + declareType.FullName + "." + property.Name + "Property";
            }
            else
            {
                propertyKey = "P:" + declareType.FullName + "." + property.Name;
            }

            var comment = this.TryFindComment(declareType.Assembly, propertyKey);

            if (string.IsNullOrEmpty(comment))
            {
                comment = this.TryGetAdditionalComment(property);
            }

            comment = this.AppendEnumValues(comment, property);

            return comment;
        }

        //public string TryFindComment(Type type, string property)
        //{
        //    var key = "P:" + type.FullName + "." + property;
        //    return this.TryFindComment(type.Assembly, key);
        //}

        private string TryFindComment(Assembly assembly, string key)
        {
            XDocument xdoc = null;
            if (!_store.TryGetValue(assembly, out xdoc))
            {
                var assemblyPath = assembly.Location;
                var xmlDocPath = Path.Combine(Path.GetDirectoryName(assemblyPath), Path.GetFileNameWithoutExtension(assemblyPath) + ".xml");
                if (File.Exists(xmlDocPath))
                {
                    try
                    {
                        xdoc = XDocument.Load(xmlDocPath);
                        _store.Add(assembly, xdoc);
                    }
                    catch { }
                }
            }

            if (xdoc != null)
            {
                var item = xdoc.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name").Value == key);
                if (item != null)
                {
                    var summary = item.Element("summary").Value.Trim();
                    return summary;
                }
            }

            return null;
        }

        /// <summary>
        /// 枚举值的注释，需要同时序列化所有枚举的值。
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private string AppendEnumValues(string comment, IManagedProperty property)
        {
            var propertyType = TypeHelper.IgnoreNullable(property.PropertyType);
            if (propertyType.IsEnum)
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    comment += Environment.NewLine + this.FormatEnumValues(propertyType);
                }
                else
                {
                    comment = this.FormatEnumValues(propertyType);
                }
            }

            return comment;
        }

        /// <summary>
        /// Formats the enum values.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        private string FormatEnumValues(Type enumType)
        {
            var sb = new StringBuilder();

            foreach (Enum item in Enum.GetValues(enumType))
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(Convert.ToInt32(item));
                sb.Append(":(");
                sb.Append(item.ToString());

                var label = EnumViewModel.EnumToLabel(item);
                if (!string.IsNullOrEmpty(label))
                {
                    sb.Append(", ");
                    sb.Append(label);
                }

                sb.Append(")");
            }

            return sb.ToString();
        }

        private string TryGetAdditionalComment(IManagedProperty property)
        {
            string comment = null;

            var dic = this.AdditionalPropertiesComments;
            if (dic != null)
            {
                dic.TryGetValue(property.Name, out comment);
            }

            return comment;
        }
    }
}