﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 23:25
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 23:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.WPF;

namespace Rafy.MultiLanguages.WPF
{
    public class LanguageModule : CallbackTemplate
    {
        protected override void OnSaveSuccessed()
        {
            base.OnSaveSuccessed();

            DbTranslator.Instance.InvalidateItems();
        }
    }
}