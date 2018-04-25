///*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20151209
// * 运行环境：.NET 4.5
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20151209 20:28
// * 
//*******************************************************/

//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Rafy.Configuration
//{
//    public class PluginsConfigurationSection
//    {
//        private IList<PluginSection> _children;

//        public IList<PluginSection> Children
//        {
//            get
//            {
//                if (_children == null)
//                {
//                    _children = this.GetChildren()
//                        .Select(s => new PluginSection
//                        {
//                            Plugin = s["plugin"]
//                        })
//                        .ToArray();
//                }
//                return _children;
//            }
//        }
//    }

//    public class PluginSection
//    {
//        internal PluginSection() { }

//        /// <summary>
//        /// 可以只填写插件程序集的全名称，也可以写出具体的插件类型的全名称。
//        /// </summary>
//        public string Plugin { get; internal set; }
//    }
//}