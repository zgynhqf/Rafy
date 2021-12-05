/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211205
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211205 14:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.DataPortal.WCF
{
    public class WCFSettings
    {
        public static bool EnableCompacting { get; set; } = true;

        public static bool EnableBinarySerialization { get; set; } = true;
    }
}