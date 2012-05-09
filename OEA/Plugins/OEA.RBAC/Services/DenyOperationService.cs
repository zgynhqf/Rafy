/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120504
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120504
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hxy.Common;
using OEA.Library;
using OEA.Web;

namespace OEA.RBAC
{
    /// <summary>
    /// 为某个指定岗位禁用一系列操作列表的服务
    /// </summary>
    public class DenyOperationService : FlowService
    {
        /// <summary>
        /// 需要为这个岗位禁用指定的操作列表。
        /// </summary>
        [ServiceInput]
        public int OrgPositionId { get; set; }

        [ServiceInput]
        public int ModuleId { get; set; }

        /// <summary>
        /// 需要被禁用的操作对象的 id  列表
        /// </summary>
        [ServiceInput]
        public string DenyOperationIds { get; set; }

        protected override Result ExecuteCore()
        {
            var orgPosition = RF.Create<OrgPosition>().GetById(this.OrgPositionId) as OrgPosition;
            if (orgPosition == null) { return "没有找到对应的岗位。"; }

            var curModule = RF.Create<ModuleAC>().GetById(this.ModuleId) as ModuleAC;
            var moduleKey = curModule.KeyLabel;

            var ids = this.DenyOperationIds.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(strId => Convert.ToInt32(strId)).ToArray();

            //清空当前模块在 denyList 中的数据
            var denyList = orgPosition.OrgPositionOperationDenyList;
            var moduleOperations = denyList.Cast<OrgPositionOperationDeny>().Where(d => d.ModuleKey == moduleKey).ToArray();
            foreach (var item in moduleOperations) { denyList.Remove(item); }

            //根据当前的选择项把数据重新加入到 denyList 中。
            var toDeny = ids.Select(id => curModule.OperationACList.FirstOrDefault(op => op.Id == id))
                .Where(op => op != null).ToArray();
            foreach (OperationAC item in toDeny)
            {
                var deny = new OrgPositionOperationDeny
                {
                    ModuleKey = moduleKey,
                    OperationKey = item.OperationKey
                };
                if (item.ScopeKeyLabel != OperationAC.ModuleScope)
                {
                    deny.BlockKey = item.ScopeKeyLabel;
                }

                denyList.Add(deny);
            }

            //保存到数据库中。
            RF.Save(orgPosition);

            return new Result(true, "权限保存成功。");
        }
    }
}
