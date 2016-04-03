/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 14:05
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 14:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy
{
    /// <summary>
    /// 语言提供器
    /// </summary>
    public abstract class Translator
    {
        /// <summary>
        /// 当前文化
        /// </summary>
        private string _currentCulture;

        /// <summary>
        /// 是否直接使用开发语言
        /// </summary>
        private bool _isDevCulture = true;

        private bool _enabled;

        /// <summary>
        /// 是否正在收集所有开发语言。
        /// </summary>
        private bool _autoCollect;

        private List<string> _ignoredCollect = new List<string>();

        /// <summary>
        /// 收集到的所有语言。
        /// </summary>
        private List<string> _collected = new List<string>(100);

        /// <summary>
        /// 是否启用整个多语言功能。
        /// </summary>
        internal virtual bool Enabled
        {
            get { return this._enabled; }
            set { this._enabled = value; }
        }

        /// <summary>
        /// 是否启动语言收集功能
        /// </summary>
        public bool AutoCollect
        {
            get
            {
                return this.Enabled && this._autoCollect;
            }
            set { this._autoCollect = value; }
        }

        /// <summary>
        /// 系统当前的文化标识。
        /// </summary>
        public string CurrentCulture
        {
            get { return this._currentCulture; }
            internal set
            {
                if (this._currentCulture != value)
                {
                    this._currentCulture = value;
                    this._isDevCulture = IsDevCulture(this._currentCulture);

                    this.OnCurrentCultureChanged();
                }
            }
        }

        /// <summary>
        /// 获取收集到的所有字符串。
        /// </summary>
        protected IList<string> CollectedList
        {
            get { return _collected; }
        }

        /// <summary>
        /// 收集某个开发语言。
        /// </summary>
        /// <param name="devCulture"></param>
        protected void Collect(string devCulture)
        {
            if (this.AutoCollect)
            {
                //如果该值在 _ignoredCollect 列表中，则不收集这个值。
                var index = this._ignoredCollect.IndexOf(devCulture);
                if (index >= 0)
                {
                    //忽略一次后，马上删除。
                    this._ignoredCollect.RemoveAt(index);
                }
                else
                {
                    if (!this._collected.Contains(devCulture))
                    {
                        this._collected.Add(devCulture);

                        this.OnCollected(devCulture);
                    }
                }
            }
        }

        /// <summary>
        /// 某个开发语言被收集后发生。
        /// </summary>
        /// <param name="devCulture"></param>
        protected virtual void OnCollected(string devCulture) { }

        /// <summary>
        /// 忽略某个字符串的收集。
        /// 
        /// 一般使用在动态拼接字符串的情况下。
        /// </summary>
        /// <param name="words">一个不希望被翻译引擎收集的字符串。</param>
        internal void IgnoreCollect(string words)
        {
            if (this.AutoCollect)
            {
                this._ignoredCollect.Add(words);
            }
        }

        /// <summary>
        /// 通过代码中直接编写的语言，翻译为对应当前语言。
        /// 
        /// 实现时，注意，此方法与 TranslateReverse 互为可逆的操作。
        /// </summary>
        /// <param name="devCulture">代码中直接编写的语言。</param>
        /// <returns></returns>
        public string Translate(string devCulture)
        {
            if (!this._isDevCulture && this.Enabled && !string.IsNullOrEmpty(devCulture))
            {
                devCulture = devCulture.Trim();

                //需要处理回车换行
                if (devCulture.Contains(Environment.NewLine))
                {
                    //由于 Baidu 使用回车来作为每一个翻译项的分隔符，而 Bing 翻译时需要使用 Url，
                    //而 Url 中也不能有回车，所以两大引擎都不支持回车。
                    //那么，程序中如果某个字符串中包含回车，而这个字符串需要被翻译时，
                    //则应该转换为多个单行字符串分开翻译。

                    var res = string.Empty;
                    var rows = devCulture.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0, c = rows.Length; i < c; i++)
                    {
                        var row = rows[i];
                        if (i > 0)
                        {
                            res += Environment.NewLine;
                        }

                        string rowResult = null;
                        if (!this.TranslateCore(devCulture, out rowResult))
                        {
                            rowResult = devCulture;
                            this.Collect(devCulture);
                        }

                        res += rowResult;
                    }

                    return res;
                }
                else
                {
                    string result = null;
                    if (this.TranslateCore(devCulture, out result))
                    {
                        return result;
                    }

                    this.Collect(devCulture);
                }
            }

            return devCulture;
        }

        /// <summary>
        /// 通过代码中直接编写的语言，翻译为对应当前语言。
        /// 
        /// 实现时，注意，此方法与 TranslateReverseCore 互为可逆的操作。
        /// </summary>
        /// <param name="devCulture">代码中直接编写的语言。</param>
        /// <param name="result">翻译后的结果。</param>
        /// <returns>是否成功翻译。如果翻译失败，基类会把结果收集起来。</returns>
        protected abstract bool TranslateCore(string devCulture, out string result);

        /// <summary>
        /// 通过当前语言，翻译为对应代码中直接编写的语言。
        /// 
        /// 实现时，注意，此方法与 Translate 互为可逆的操作。
        /// </summary>
        /// <param name="currentCulture">当前语言对应的语句。</param>
        /// <returns></returns>
        public string TranslateReverse(string currentCulture)
        {
            if (this._isDevCulture || !this.Enabled) { return currentCulture; }

            return this.TranslateReverse(currentCulture);
        }

        /// <summary>
        /// 通过当前语言，翻译为代码中直接编写的语言。
        /// 
        /// 实现时，注意，此方法与 TranslateCore 互为可逆的操作。
        /// </summary>
        /// <param name="currentCulture"></param>
        /// <returns></returns>
        protected abstract string TranslateReverseCore(string currentCulture);

        /// <summary>
        /// 所有支持的语言。
        /// </summary>
        /// <returns></returns>
        public abstract IList<string> GetSupportCultures();

        /// <summary>
        /// 当前文化项发生改变时的事件。
        /// </summary>
        protected virtual void OnCurrentCultureChanged() { }

        /// <summary>
        /// 判断指定的语言是否为开发语言。
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static bool IsDevCulture(string language)
        {
            return string.IsNullOrEmpty(language)
                || language.EqualsIgnoreCase(RafyEnvironment.Configuration.DevCulture);
        }
    }
}