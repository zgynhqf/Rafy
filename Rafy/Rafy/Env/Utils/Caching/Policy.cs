/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：缓存策略
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 缓存策略
    /// </summary>
    public class Policy
    {
        /// <summary>
        /// 一个空策略。
        /// </summary>
        public static readonly Policy Empty = new Policy();

        /// <summary>
        /// 缓存使用的实时检测器
        /// </summary>
        public ChangeChecker Checker { get; set; }
    }
}
