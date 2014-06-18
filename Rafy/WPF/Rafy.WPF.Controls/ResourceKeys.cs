/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121031 17:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121031 17:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 暂未使用。
    /// 
    /// 这里只是约定了，使用哪些字符串作为资源的键，以达到动态皮肤的功能。
    /// </summary>
    public static class ResourceKeys
    {
        public static readonly string Color1 = "Color1";
        public static readonly string Color2 = "Color2";
        public static readonly string Color3 = "Color3";
        public static readonly string Color4 = "Color4";
        public static readonly string Color5 = "Color5";

        public static readonly string Brush1 = "Brush1";
        public static readonly string Brush2 = "Brush2";
        public static readonly string Brush3 = "Brush3";
        public static readonly string Brush4 = "Brush4";
        public static readonly string Brush5 = "Brush5";

        public static readonly string PrimaryColor = "PrimaryColor";
        public static readonly string SecondaryColor = "SecondaryColor";
        public static readonly string ThirdColor = "ThirdColor";
        public static readonly string FourthColor = "FourthColor";

        public static readonly string PrimaryBrush = "PrimaryBrush";
        public static readonly string SecondaryBrush = "SecondaryBrush";
        public static readonly string ThirdBrush = "ThirdBrush";
        public static readonly string FourthBrush = "FourthBrush";

        //public static readonly ResourceKey ThirdColorKey = new StaticResourceKey(typeof(ResourceKeys), "ThirdColorKey");
    }
}
