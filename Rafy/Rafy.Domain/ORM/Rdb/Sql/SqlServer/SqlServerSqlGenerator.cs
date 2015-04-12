/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 15:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    class SqlServerSqlGenerator : SqlGenerator
    {
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("[").Append(identifier).Append("]");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }
    }
}
