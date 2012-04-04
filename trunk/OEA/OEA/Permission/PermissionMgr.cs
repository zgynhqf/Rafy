/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using System.Security.Principal;
using OEA.MetaModel;

namespace OEA
{
    /// <summary>
    /// 系统权限的抽象定义
    /// </summary>
    public class PermissionMgr
    {
        private static PermissionMgr _Provider;
        public static PermissionMgr Provider
        {
            get
            {
                if (_Provider == null) { _Provider = new PermissionMgr(); }

                return _Provider;
            }
            set { _Provider = value; }
        }

        protected PermissionMgr() { }

        /// <summary>
        /// 返回当前用户
        /// </summary>
        /// <returns></returns>
        public virtual IUser CurrentUser
        {
            get { return new AnonymousIdentity(); }
        }

        /// <summary>
        /// 是否能显示某个模块
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public virtual bool CanShowModule(ModuleMeta module)
        {
            return true;
        }

        /// <summary>
        /// 控制某一个块是否可以显示
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public virtual bool CanShowBlock(ModuleMeta module, Block block)
        {
            return true;
        }

        /// <summary>
        /// 是否有某个操作的权限
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public virtual bool HasOperation(ModuleMeta module, ModuleOperation operation)
        {
            return true;
        }

        /// <summary>
        /// 是否能执行某个命令
        /// </summary>
        /// <param name="block"></param>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public virtual bool HasCommand(ModuleMeta module, Block block, string commandName)
        {
            return true;
        }
    }
}