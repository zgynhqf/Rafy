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
using Rafy.Utils;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Web;

namespace Rafy.RBAC
{
    [Serializable]
    [JsonService]
    [Contract, ContractImpl]
    public class ClearLogService : Service
    {
        protected override void Execute()
        {
            using (var dba = DbAccesserFactory.Create(DbSettingNames.RafyPlugins))
            {
                dba.RawAccesser.ExecuteText("DELETE FROM AUDITITEM");
            }
        }
    }
}