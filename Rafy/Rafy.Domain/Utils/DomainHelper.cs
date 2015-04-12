/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 00:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    internal class DomainHelper
    {
        /// <summary>
        /// 判断某个值是否非空。
        /// 
        /// 如果是字符串，则检测它是否为非空字符。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(object value)
        {
            bool notNull = false;

            if (value is string)
            {
                notNull = !string.IsNullOrEmpty(value as string);
            }
            else if (value != null)
            {
                notNull = true;
            }

            return notNull;
        }
    }
}
