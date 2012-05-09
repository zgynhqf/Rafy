/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Utils;
using OEA.ORM;

namespace OEA.RBAC
{
    [Serializable]
    public class ClearLogService : Service
    {
        protected override void Execute()
        {
            using (var db = Db.Create(ConnectionStringNames.OEAPlugins))
            {
                db.DBA.ExecuteTextNormal("DELETE FROM AUDITITEM");
            }
        }
    }
}