/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Rafy.Data
{
    public class LongTimeDbAccesser : DbAccesser
    {
        public LongTimeDbAccesser(DbSetting setting) : base(setting) { }

        protected override IDbCommand PrepareCommand(string strSql, CommandType type, IDbDataParameter[] parameters)
        {
            var cmd = base.PrepareCommand(strSql, type, parameters);
            cmd.CommandTimeout = 500;
            return cmd;
        }
    }
}
