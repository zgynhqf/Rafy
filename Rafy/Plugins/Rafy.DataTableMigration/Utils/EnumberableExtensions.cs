/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy.Domain;

namespace Rafy.DataTableMigration.Utils
{
    internal static class EnumberableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> callback) where  T : Entity
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            foreach (var item in items)
            {
                callback(item);
            }
        }
    }
}