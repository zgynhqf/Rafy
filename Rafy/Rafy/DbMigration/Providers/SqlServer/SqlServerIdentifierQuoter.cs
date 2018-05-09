/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170920
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170920 20:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.SqlServer
{
    class SqlServerIdentifierQuoter : DbIdentifierQuoter
    {
        public static readonly SqlServerIdentifierQuoter Instance = new SqlServerIdentifierQuoter();

        private SqlServerIdentifierQuoter() { }

        public override char QuoteStart => '[';

        public override char QuoteEnd => ']';
    }
}
