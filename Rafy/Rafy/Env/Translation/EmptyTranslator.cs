/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 14:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 14:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 不支持更换其它语言的翻译器
    /// </summary>
    internal class EmptyTranslator : Translator
    {
        internal override bool Enabled
        {
            get { return false; }
            set { }
        }

        protected override void OnCurrentCultureChanged()
        {
            base.OnCurrentCultureChanged();

            //不论语言怎么变，都设置回开发语言。
            this.CurrentCulture = RafyEnvironment.Configuration.DevCulture;

            //throw new NotSupportedException("不支持更换语言，请实现 ITranslator 相应接口。");
        }

        protected override bool TranslateCore(string devLanguage, out string result)
        {
            result = devLanguage;
            return true;
            //return embadedLanguage + "Translated!";//测试代码
        }

        protected override string TranslateReverseCore(string currentLanguage)
        {
            return currentLanguage;
            //return currentLanguage.Replace("Translated!", string.Empty);//测试代码
        }

        public override IList<string> GetSupportCultures()
        {
            return new string[] { RafyEnvironment.Configuration.DevCulture };
        }
    }
}
