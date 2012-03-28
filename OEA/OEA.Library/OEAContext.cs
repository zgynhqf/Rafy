/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110317
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100317
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace OEA.Library
{
    public class OEAContext : DbContextBase
    {
        public OEAContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection) { }
    }
}