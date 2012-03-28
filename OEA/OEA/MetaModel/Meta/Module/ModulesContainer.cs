/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 模块容器
    /// </summary>
    public class ModulesContainer
    {
        private List<ModuleMeta> _roots = new List<ModuleMeta>();

        internal ModulesContainer() { }

        /// <summary>
        /// 获取所有根模块
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModuleMeta> GetRoots()
        {
            return this._roots;
        }

        public IEnumerable<ModuleMeta> GetRootsWithPermission()
        {
            return this._roots.Where(PermissionMgr.Provider.CanShowModule);
        }

        /// <summary>
        /// 添加一个根模块
        /// </summary>
        /// <param name="module"></param>
        public ModuleMeta AddRoot(ModuleMeta module)
        {
            this._roots.Add(module);

            return module;
        }

        /// <summary>
        /// 根据唯一的名称来查找某个模块
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public ModuleMeta FindModule(string keyName)
        {
            foreach (var root in this._roots)
            {
                var m = this.FindModule(root, keyName);
                if (m != null) return m;
            }

            return null;
        }

        /// <summary>
        /// 冻结所有的元数据
        /// </summary>
        internal void Freeze()
        {
            foreach (var v in this._roots) { v.Freeze(); }
        }

        private ModuleMeta FindModule(ModuleMeta module, string keyName)
        {
            if (module.KeyLabel == keyName) { return module; }

            foreach (var child in module.Children)
            {
                var m = this.FindModule(child, keyName);
                if (m != null) return m;
            }

            return null;
        }
    }
}