/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 19:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rafy;
using Rafy.ManagedProperty;

namespace iWS.Web.Http
{
    public static class Extensions
    {
        public static IManagedProperty Find(this ManagedPropertyList list, string property, bool ignoreCase)
        {
            if (ignoreCase)
            {
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var item = list[i];
                    if (item.Name.EqualsIgnoreCase(property))
                    {
                        return item;
                    }
                }
                return null;
            }

            return list.Find(property);
        }
    }
}