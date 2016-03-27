/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130523
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 18:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy;
using Rafy.Web;

namespace Rafy.RBAC.Old
{
    [Serializable, JsonService]
    [Contract, ContractImpl]
    class SaveUserService : FlowService, ISaveListService
    {
        [ServiceInput, ServiceOutput]
        public EntityList EntityList { get; set; }

        protected override Result ExecuteCore()
        {
            var repo = RF.Concrete<UserRepository>();

            foreach (User item in EntityList)
            {
                if (item.IsDirty)
                {
                    var exsits = repo.GetByCode(item.Code);
                    if (exsits != null && exsits.Id != item.Id)
                    {
                        return "已经有这个编码的用户了。";
                    }
                }
            }

            repo.Save(EntityList);
            return true;
        }
    }
}