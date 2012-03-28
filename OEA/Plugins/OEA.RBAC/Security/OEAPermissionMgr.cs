/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.RBAC.Security
{
    /// <summary>
    /// 为底层的 OEA 提供权限实现方案
    /// </summary>
    internal class OEAPermissionMgr : PermissionMgr
    {
        public override IUser CurrentUser
        {
            get { return OEAIdentity.Current ?? base.CurrentUser; }
        }

        public override bool CanShowModule(ModuleMeta module)
        {
            return HasModuleOperation(OEAIdentity.Current, module.KeyLabel, string.Empty, SystemOperationKeys.Read);
        }

        public override bool CanShowBlock(ModuleMeta module, Block block)
        {
            return HasModuleOperation(OEAIdentity.Current, module.KeyLabel, block.KeyLabel, SystemOperationKeys.Read);
        }

        public override bool HasCommand(ModuleMeta module, Block block, string commandKey)
        {
            return HasModuleOperation(OEAIdentity.Current, module.KeyLabel, block.KeyLabel, commandKey);
        }

        public override bool HasOperation(ModuleMeta module, ModuleOperation operation)
        {
            return HasModuleOperation(OEAIdentity.Current, module.KeyLabel, string.Empty, operation.Name);
        }

        /// <summary>
        /// 检查是否拥有某个模块下某个实体某个操作的权限。
        /// </summary>
        /// <param name="module"></param>
        /// <param name="block">
        /// 该参数可以为 null
        /// </param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static bool HasModuleOperation(OEAIdentity user, string module, string block, string operation)
        {
            if (user == null) { return true; }

            var res = user.Roles.Cast<OrgPosition>()
                .Any(role =>
                {
                    //所有的都不相等，则表示有这个权限
                    var hasPermission = role.OrgPositionOperationDenyList.Cast<OrgPositionOperationDeny>()
                        .All(i => i.ModuleKey != module || i.BlockKey != block || i.OperationKey != operation);
                    return hasPermission;
                });
            return res;
        }
    }
}