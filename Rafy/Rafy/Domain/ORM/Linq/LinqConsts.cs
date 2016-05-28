/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130427 16:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Linq
{
    class LinqConsts
    {
        public const string QueryableMethod_Where = "Where";
        public const string QueryableMethod_OrderBy = "OrderBy";
        public const string QueryableMethod_OrderByDescending = "OrderByDescending";
        public const string QueryableMethod_ThenBy = "ThenBy";
        public const string QueryableMethod_ThenByDescending = "ThenByDescending";
        public const string QueryableMethod_Count = "Count";
        public const string QueryableMethod_LongCount = "LongCount";

        public const string EnumerableMethod_Contains = "Contains";
        public const string EnumerableMethod_Any = "Any";
        public const string EnumerableMethod_All = "All";

        public const string ListGenericMethod_Contains = "Contains";

        public const string StringMethod_Contains = "Contains";
        public const string StringMethod_StartWith = "StartsWith";
        public const string StringMethod_EndWith = "EndsWith";
        public const string StringMethod_IsNullOrEmpty = "IsNullOrEmpty";
    }
}
