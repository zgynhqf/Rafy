/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 所有 Web 命令的集合
    /// 
    /// 这里只存储这些命令的一个原始值。每个类型的视图 EVM 中的 Commands 是这些值的拷贝。
    /// </summary>
    public class WebCommandRepository : MetaRepositoryBase<WebCommand>
    {
        internal WebCommandRepository() { }

        public WebCommand this[string commandName]
        {
            get { return this.Get(commandName); }
        }

        public WebCommand Get(string commandName)
        {
            var jsCmd = this.Find(commandName);
            if (jsCmd == null)
            {
                var msg = string.Format("不存在客户端命令： {0} 。", commandName);
                throw new InvalidOperationException(msg);
            }
            return jsCmd;
        }

        public WebCommand Find(string commandName)
        {
            return this.FirstOrDefault(c => c.Name == commandName);
        }

        internal void AddCommand(WebCommand command)
        {
            this.AddPrime(command);
        }

        /// <summary>
        /// 把某个目录中的 jsCommand 都加入到 Repository 中。
        /// </summary>
        /// <param name="directory">
        /// 这个目录中的每一个文件是一个单独的 jsCommand。
        /// 如果要放多个 jsCommand 到一个文件中，请用以下分隔符分开每一个 jsCommand：
        /// //command end
        /// </param>
        public void AddByDirectory(string directory)
        {
            var jsFiles = Directory.GetFiles(directory, "*.js");
            foreach (var jsFile in jsFiles)
            {
                var js = File.ReadAllText(jsFile);
                this.AddByJs(js);
            }
        }

        /// <summary>
        /// 添加程序集中 Commands 文件夹下的 js Resource。
        /// </summary>
        /// <param name="assembly"></param>
        public void AddByAssembly(Assembly assembly)
        {
            var resources = assembly.GetManifestResourceNames()
                .Where(r => r.ToLower().Contains("commands.") && r.ToLower().Contains(".js"))
                .ToArray();
            foreach (var resource in resources)
            {
                var stream = assembly.GetManifestResourceStream(resource);
                using (var sr = new StreamReader(stream))
                {
                    var js = sr.ReadToEnd();
                    this.AddByJs(js);
                }
            }
        }

        /// <summary>
        /// 匹配 js 中定义的类名
        /// </summary>
        private static readonly Regex ClassRegex = new Regex(@"Ext\.define\(('|"")(?<className>[\w\.]+)('|"").+extend\s*:\s*('|"")(?<extend>[\w\.]+)('|"")", RegexOptions.Singleline);
        /// <summary>
        /// 匹配 js 中 config 定义中 meta 中的 text 作为标签
        /// </summary>
        private static readonly Regex TextRegex = new Regex(@"\{\s*meta\s*\:.*?text.*?\:\s*('|"")(?<text>[^'""]+)('|"")");

        private void AddByJs(string js)
        {
            var codes = js.Split(new string[] { "//command end" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var code in codes)
            {
                var jsCode = code.Trim();

                var match = ClassRegex.Match(jsCode);
                if (match.Success)
                {
                    var className = match.Groups["className"].Value;
                    var extend = match.Groups["extend"].Value;
                    if (string.IsNullOrEmpty(className)) throw new InvalidOperationException();
                    var command = new WebCommand
                    {
                        Name = className,
                        JavascriptCode = jsCode,
                        Extend = extend
                    };

                    var textMatch = TextRegex.Match(jsCode);
                    if (textMatch.Success)
                    {
                        command.Label = textMatch.Groups["text"].Value;
                    }

                    this.AddCommand(command);
                }
            }
        }

        /// <summary>
        /// 返回整个 javascript 的合集。
        /// </summary>
        /// <returns></returns>
        public string CombineAll()
        {
            //需要保证父子关系的顺序。
            if (!this._sorted)
            {
                this._sorted = true;
                var list = this.GetInnerList();

                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var a = list[i];
                    for (int j = i + 1; j < c; j++)
                    {
                        var b = list[j];
                        if (b.Name == a.Extend)
                        {
                            list[j] = a;
                            list[i] = b;
                            i--;
                            break;
                        }
                    }
                }
            }

            var buffer = new StringBuilder();

            foreach (var command in this)
            {
                buffer.Append(command.JavascriptCode);

                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        private bool _sorted = false;

        //public void SortByName(params string[] commandNames)
        //{
        //    Helper.Sort(this.GetInnerList(), c => c.Name, commandNames);
        //}

        //public void SortByLabel(params string[] labels)
        //{
        //    Helper.Sort(this.GetInnerList(), c => c.Label, labels);
        //}
    }
}