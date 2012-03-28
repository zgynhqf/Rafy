/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111201
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UITesting;
using OEA.UIA.Utils;

namespace OEA.UIA
{
    public static class SystemExtension
    {
        public static string[] SplitBy(this string title, string splitter)
        {
            return title.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}