/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120508
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120508
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rafy.MetaModel
{
    /// <summary>
    /// js 文件帮助类。
    /// </summary>
    internal static class WebCommandJsHelper
    {
        /// <summary>
        /// 匹配 js 中定义的类名
        /// </summary>
        private static readonly Regex ClassRegex = new Regex(@"Rafy\.defineCommand\(('|"")(?<className>[\w\.]+)(('|"").+extend\s*:\s*('|"")(?<extend>[\w\.]+)('|""))?", RegexOptions.Singleline);

        /// <summary>
        /// 匹配 js 中 config 定义中 meta 中的 text 作为标签
        /// </summary>
        internal static readonly Regex TextRegex = new Regex(@"\s*meta\s*\:.*?text.*?\:\s*('|"")(?<text>[^'""]+)('|"")");

        /// <summary>
        /// 匹配 js 中 config 定义中 meta 中的 group 作为标签
        /// </summary>
        internal static readonly Regex GroupRegex = new Regex(@"\s*meta\s*\:.*?group.*?\:\s*('|"")(?<group>[^'""]+)('|"")");

        /// <summary>
        /// 当一个 js 文件中写了多个 js 类时，需要使用此字符串来进行分隔
        /// </summary>
        private static readonly string[] JsBlocksSplitter = new string[] { "//rafy:commandEnd" };

        /// <summary>
        /// 根据分隔符加载某个 js 文件中的所有的类。
        /// </summary>
        /// <param name="jsContent"></param>
        /// <param name="list"></param>
        public static void LoadCommandJsBlocks(string jsContent, List<JsBlock> list)
        {
            //可以使用 “//rafy:commandEnd” 来分隔多个命令，方便在一个文件中定义多个命令
            var codes = jsContent.Split(JsBlocksSplitter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var code in codes)
            {
                var jsCode = code.Trim();

                var jsClass = new JsBlock(jsCode);

                //在 JS 代码中匹配出类名和基类名。
                var match = ClassRegex.Match(jsCode);
                if (match.Success)
                {
                    jsClass.ClassName = match.Groups["className"].Value;
                    jsClass.Extend = match.Groups["extend"].Value;
                }

                list.Add(jsClass);
            }
        }
    }

    /// <summary>
    /// 表明一个 js 代码块
    /// </summary>
    internal class JsBlock
    {
        internal JsBlock(string code)
        {
            this.JavascriptCode = code;
        }

        /// <summary>
        /// 代码块对应的 js 代码。
        /// </summary>
        public string JavascriptCode { get; private set; }

        /// <summary>
        /// 如果这个代码块是一个类，则这个属性表示类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 如果这个代码块是一个类，并且它有基类，则这个属性这个类的基类的类名。
        /// </summary>
        public string Extend { get; set; }

        /// <summary>
        /// 按照继承关系排序整个列表
        /// </summary>
        /// <param name="list"></param>
        public static void SortByHierachy(List<JsBlock> list)
        {
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var a = list[i];
                if (a.Extend != null)
                {
                    for (int j = i + 1; j < c; j++)
                    {
                        var b = list[j];
                        if (b.ClassName == a.Extend)
                        {
                            list[j] = a;
                            list[i] = b;
                            i--;
                            break;
                        }
                    }
                }
            }
        }
    }
}