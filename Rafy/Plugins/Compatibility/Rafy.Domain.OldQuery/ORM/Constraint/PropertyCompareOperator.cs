/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 18:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 属性对比操作
    /// </summary>
    internal enum PropertyCompareOperator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,

        Like,
        Contains,
        StartWith,
        EndWith,

        In,
        NotIn,
    }
}