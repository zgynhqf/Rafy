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
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy
{
    /// <summary>
    /// 权限提供程序。
    /// </summary>
    public class PermissionProvider
    {
        protected internal PermissionProvider() { }

        /// <summary>
        /// 是否能显示某个模块
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        internal protected virtual bool CanShowModule(ModuleMeta module)
        {
            return true;
        }

        /// <summary>
        /// 控制某一个块是否可以显示
        /// </summary>
        /// <param name="module"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        internal protected virtual bool CanShowBlock(ModuleMeta module, Block block)
        {
            return true;
        }

        /// <summary>
        /// 是否有某个操作的权限
        /// </summary>
        /// <param name="module"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal protected virtual bool HasOperation(ModuleMeta module, ModuleOperation operation)
        {
            return true;
        }

        /// <summary>
        /// 是否能执行某个命令
        /// </summary>
        /// <param name="module"></param>
        /// <param name="block"></param>
        /// <param name="commandName"></param>
        /// <returns></returns>
        internal protected virtual bool HasCommand(ModuleMeta module, Block block, string commandName)
        {
            return true;
        }
    }
}
