/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130125 11:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130125 11:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Configuration;

namespace Rafy.Configuration
{
    /// <summary>
    /// 用于配置中的一些值。
    /// 可以明确指明 Yes/No，如果不指明，则与当前是否为调试状态一致。
    /// </summary>
    public enum DynamicBoolean
    {
        Yes,
        No,
        IsDebugging
    }
}

namespace Rafy
{
    public static class DynamicBooleanExtension
    {
        public static bool ToBoolean(this DynamicBoolean raw)
        {
            switch (raw)
            {
                case DynamicBoolean.Yes:
                    return true;
                case DynamicBoolean.No:
                    return false;
                case DynamicBoolean.IsDebugging:
                default:
                    return RafyEnvironment.IsDebuggingEnabled;
            }
        }
    }
}