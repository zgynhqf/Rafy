﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 16:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    /// <summary>
    /// 使用 IQuery 进行实体查询的参数。
    /// </summary>
    internal interface IORMQueryArgs : IEntityQueryArgs
    {
        /// <summary>
        /// 对应的查询条件定义。
        /// </summary>
        IQuery Query { get; }
    }
}
