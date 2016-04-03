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

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 实体类、实体属性的注释查找器。
    /// </summary>
    class CommentFinder
    {
        private Dictionary<Assembly, XDocument> _store = new Dictionary<Assembly, XDocument>();

        public string TryFindComment(Type type)
        {
            return this.TryFindComment(type.Assembly, "T:" + type.FullName);
        }

        public string TryFindComment(Type type, string property)
        {
            var key = "P:" + type.FullName + "." + property;
            return this.TryFindComment(type.Assembly, key);
        }

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
    }
}