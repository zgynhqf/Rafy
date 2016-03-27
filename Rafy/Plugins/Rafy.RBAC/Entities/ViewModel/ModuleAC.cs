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
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.ManagedProperty;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old;
using Rafy.Domain.ORM;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// （Module Access Control）
    /// 模块元数据的显示模型
    /// </summary>
    [RootEntity]
    public partial class ModuleAC : IntEntity
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
            return this.Core.KeyLabel.Translate();
        }

        public static readonly ListProperty<OperationACList> OperationACListProperty = P<ModuleAC>.RegisterList(e => e.OperationACList);
        public OperationACList OperationACList
        {
            get { return this.GetLazyList(OperationACListProperty); }
        }
    }

    public partial class ModuleACList : EntityList { }

    public partial class ModuleACRepository : MemoryEntityRepository
    {
        protected ModuleACRepository() { }

        protected override string GetRealKey(Entity entity)
        {
            return (entity as ModuleAC).KeyLabel;
        }
    }

    [DataProviderFor(typeof(ModuleACRepository))]
    public partial class ModuleACDataProvider : MemoryEntityRepository.MemoryRepositoryDataProvider
    {
        /// <summary>
        /// 重写此方法，直接把模块元数据读取到界面上
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Entity> LoadAll()
        {
            var list = new List<Entity>();

            foreach (var root in CommonModel.Modules.Roots)
            {
                this.AddItemRecur(list, root);
            }

            return list;
        }

        protected override void MemoryClone(Entity src, Entity dst)
        {
            base.MemoryClone(src, dst);

            (dst as ModuleAC).Core = (src as ModuleAC).Core;
            //dst.CastTo<ModuleAC>().Core = src.CastTo<ModuleAC>().Core;
        }

        private ModuleAC AddItemRecur(IList<Entity> list, ModuleMeta module)
        {
            var item = new ModuleAC();
            item.Core = module;

            //这句会生成 Id。
            item.Id = RafyEnvironment.NewLocalId();

            list.Add(item);

            foreach (var child in module.Children)
            {
                var childModule = this.AddItemRecur(list, child);
                childModule.TreeParent = item;
            }

            item.PersistenceStatus = PersistenceStatus.Unchanged;

            return item;
        }
    }

    internal class ModuleACConfig : EntityConfig<ModuleAC>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();
        }
    }
}