/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 23:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.DbMigration.Oracle
{
    [DebuggerDisplay("CreateTableSequence : {SequenceName}")]
    internal class TryCreateTableSequenceRun : MigrationRun
    {
        public string SequenceName { get; set; }

        protected override void RunCore(IDbAccesser db)
        {
            var count = Convert.ToInt32(db.QueryValue(
                "SELECT COUNT(0) FROM ALL_SEQUENCES WHERE SEQUENCE_NAME = {0} AND SEQUENCE_OWNER = {1}",
                this.SequenceName,
                DbConnectionSchema.GetOracleUserId(db.ConnectionSchema).ToUpper()
                ));

            if (count <= 0)
            {
                db.ExecuteText(string.Format(
@"CREATE SEQUENCE {0}
MINVALUE 1
MAXVALUE 99999999999999999
START WITH 1
INCREMENT BY 1
NOCACHE
ORDER", this.SequenceName));
            }

            //            var sql = string.Format(@"DECLARE T_COUNT NUMBER;
            //BEGIN
            //    SELECT COUNT(*) INTO T_COUNT FROM DUAL WHERE EXISTS(SELECT * FROM ALL_SEQUENCES WHERE SEQUENCE_NAME='{0}');
            //    IF T_COUNT = 0 THEN
            //        EXECUTE IMMEDIATE '
            //        CREATE SEQUENCE {0}
            //        MINVALUE 1
            //        MAXVALUE 99999999999999999
            //        START WITH 1
            //        INCREMENT BY 1
            //        NOCACHE
            //        ORDER
            //        ';
            //    END IF;
            //END;", this.SEQName(op));

        }
    }
}
