/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120226
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy;
using Rafy.Reflection;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain;
using System.ComponentModel;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 从代码中读取 EntityViewMeta
    /// </summary>
    class CodeEVMReader
    {
        /// <summary>
        /// 从一个实体类型读取它所对应的视图模型。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        internal EntityViewMeta Read(EntityMeta meta)
        {
            var evm = this.CreateEntityViewMeta(meta);

            return evm;
        }

        /// <summary>
        /// 创建某个实体类的视图元数据
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        private EntityViewMeta CreateEntityViewMeta(EntityMeta entityMeta)
        {
            var entityType = entityMeta.EntityType;

            var vm = RafyEnvironment.Location.IsWebUI ? new WebEntityViewMeta() : new WPFEntityViewMeta() as EntityViewMeta;
            vm.EntityMeta = entityMeta;

            this.CreatePropertiesViewMeta(vm);

            return vm;
        }

        /// <summary>
        /// 加载所有属性元数据
        /// </summary>
        /// <param name="viewMeta"></param>
        private void CreatePropertiesViewMeta(EntityViewMeta viewMeta)
        {
            foreach (var property in EntityMetaHelper.GetEntityProperties(viewMeta.EntityMeta))
            {
                this.CreateEntityPropertyViewMeta(property, viewMeta);
            }
        }

        private EntityPropertyViewMeta CreateEntityPropertyViewMeta(IManagedProperty mp, EntityViewMeta evm)
        {
            var item = evm.CreatePropertyViewMeta();
            item.Owner = evm;

            item.PropertyMeta = evm.EntityMeta.Property(mp);

            //如果这个托管属性有对应的 CLR 属性，并且这个 CLR 属性上标记了 Label 标签，则设置元数据的 Label 属性。
            var runtimeProperty = item.PropertyMeta.CLRProperty;
            if (runtimeProperty != null)
            {
                var labelAttri = runtimeProperty.GetSingleAttribute<DisplayNameAttribute>();
                if (labelAttri != null) item.Label = labelAttri.DisplayName;
            }

            item.Readonly(mp.IsReadOnly || mp == EntityConvention.Property_TreeIndex);

            //如果是引用实体的属性，创建 SelectionViewMeta
            if (item.IsReference)
            {
                item.SelectionViewMeta = new SelectionViewMeta();
            }

            evm.EntityProperties.Add(item);

            return item;
        }
    }
}