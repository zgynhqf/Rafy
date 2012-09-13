/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120326
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;
using OEA.MetaModel.View;
using OEA.RBAC;

namespace OEA.RBAC
{
    /// <summary>
    /// （Module Access Control）
    /// 模块元数据的显示模型
    /// </summary>
    [RootEntity]
    public class ModuleAC : Entity
    {
        public ModuleMeta Core { get; set; }

        public static readonly Property<string> KeyLabelProperty = P<ModuleAC>.RegisterReadOnly(e => e.KeyLabel, e => (e as ModuleAC).GetKeyLabel());
        public string KeyLabel
        {
            get { return this.GetProperty(KeyLabelProperty); }
        }
        private string GetKeyLabel()
        {
            if (this.Core == null) return string.Empty;
            return this.Core.KeyLabel;
        }

        public static readonly ListProperty<OperationACList> OperationACListProperty = P<ModuleAC>.RegisterList(e => e.OperationACList);
        public OperationACList OperationACList
        {
            get { return this.GetLazyList(OperationACListProperty); }
        }
    }

    public class ModuleACList : EntityList { }

    public class ModuleACRepository : MemoryEntityRepository
    {
        protected ModuleACRepository() { }

        protected override string GetRealKey(Entity entity)
        {
            return (entity as ModuleAC).KeyLabel;
        }

        /// <summary>
        /// 重写此方法，直接把模块元数据读取到界面上
        /// </summary>
        /// <returns></returns>
        protected override EntityList GetAllCore()
        {
            var list = new ModuleACList();

            var roots = UIModel.Modules.GetRoots();
            foreach (var root in UIModel.Modules.GetRoots())
            {
                this.AddItemRecur(list, root);
            }

            this.NotifyLoaded(list);

            return list;
        }

        private ModuleAC AddItemRecur(EntityList list, ModuleMeta module)
        {
            var item = new ModuleAC();
            item.Core = module;

            //这句会生成 Id。
            this.NotifyLoaded(item);

            list.Add(item);

            foreach (var child in module.Children)
            {
                var childModule = this.AddItemRecur(list, child);
                childModule.TreeParent = item;
            }

            item.MarkUnchanged();

            return item;
        }
    }

    internal class ModuleACConfig : EntityConfig<ModuleAC>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasDelegate(ModuleAC.KeyLabelProperty).DomainName("界面模块");

            View.Property(ModuleAC.KeyLabelProperty).HasLabel("模块").ShowIn(ShowInWhere.ListDropDown);
        }
    }
}