/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Rafy.DbMigration
{
    [Serializable]
    public class DbMigrationException : Exception
    {
        public DbMigrationException() { }

        public DbMigrationException(string message) : base(message) { }

        public DbMigrationException(string message, Exception inner) : base(message, inner) { }

        protected DbMigrationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
