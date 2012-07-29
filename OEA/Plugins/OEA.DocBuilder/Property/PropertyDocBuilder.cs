/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:49
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OEA.DocBuilder.Property
{
    class PropertyDocBuilder
    {
        public Assembly PluginAssembly { get; set; }

        public string CodeDirectory { get; set; }

        ///// <summary>
        ///// 要生成的类型列表。
        ///// 
        ///// 如果为空，表示完全生成。
        ///// </summary>
        //public string[] Classes { get; set; }
    }
}
