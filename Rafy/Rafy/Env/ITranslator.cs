/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211027
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211027 09:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy
{
    public interface ITranslator
    {
        /// <summary>
        /// 把程序中编写的字符串翻译为当前语言。
        /// 
        /// 直接扩展在字符串上的翻译方法，方便使用
        /// </summary>
        /// <param name="embadedValue"></param>
        /// <returns></returns>
        string Translate(string embadedValue);

        /// <summary>
        /// 把当前语言翻译为程序中编写的字符串。
        /// 
        /// 直接扩展在字符串上的翻译方法，方便使用
        /// </summary>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        string TranslateReverse(string translatedValue);
    }
}