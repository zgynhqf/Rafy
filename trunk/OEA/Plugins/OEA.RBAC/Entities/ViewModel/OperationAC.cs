/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201101
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201101
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
using OEA.MetaModel.View;

namespace OEA.RBAC
{
    /// <summary>
    /// （Operation Access Control）
    /// 功能元数据的显示模型。表示某个模块下的某个功能。
    /// 
    /// 功能主要有两种：
    /// 主要是命令，其次是一些自定义功能。
    /// 
    /// 由 Module + Scope + OperationKey 构成主键
    /// </summary>
    [ChildEntity]
    public class OperationAC : Entity
    {
        public const string ModuleScope = "模块功能";

        /// <summary>
        /// 所属模块
        /// </summary>
        public static readonly RefProperty<ModuleAC> ModuleRefProperty =
            P<OperationAC>.RegisterRef(e => e.ModuleAC, ReferenceType.Parent);
        public int ModuleACId
        {
            get { return this.GetRefId(ModuleRefProperty); }
            set { this.SetRefId(ModuleRefProperty, value); }
        }
        public ModuleAC ModuleAC
        {
            get { return this.GetRefEntity(ModuleRefProperty); }
            set { this.SetRefEntity(ModuleRefProperty, value); }
        }

        /// <summary>
        /// 当前功能所属的范围。
        /// 如果是模块，则这个属性应该是 OperationAC.ModuleScope。
        /// 如果是某个界面块，则这个属性应该界面块的键。
        /// </summary>
        public static readonly Property<string> ScopeKeyLabelProperty = P<OperationAC>.Register(e => e.ScopeKeyLabel);
        public string ScopeKeyLabel
        {
            get { return this.GetProperty(ScopeKeyLabelProperty); }
            set { this.SetProperty(ScopeKeyLabelProperty, value); }
        }

        /// <summary>
        /// 功能名称
        /// </summary>
        public static readonly Property<string> OperationKeyProperty = P<OperationAC>.Register(e => e.OperationKey);
        public string OperationKey
        {
            get { return this.GetProperty(OperationKeyProperty); }
            set { this.SetProperty(OperationKeyProperty, value); }
        }

        public static readonly Property<string> LabelProperty = P<OperationAC>.Register(e => e.Label);
        public string Label
        {
            get { return this.GetProperty(LabelProperty); }
            set { this.SetProperty(LabelProperty, value); }
        }
    }

    public class OperationACList : EntityList { }

    public class OperationACRepository : MemoryEntityRepository
    {
        protected OperationACRepository() { }

        protected override string GetRealKey(Entity entity)
        {
            var op = entity as OperationAC;
            if (op.ModuleAC == null) return string.Empty;
            return op.ModuleAC.KeyName + "_" + op.ScopeKeyLabel + "_" + op.OperationKey;
        }

        public override EntityList GetByParent(Entity parent, bool withCache = true)
        {
            var list = this.GetByModule(parent as ModuleAC);

            list.SetParentEntity(parent);

            this.NotifyLoaded(list);

            return list;
        }

        public override EntityList GetByParentId(int parentId)
        {
            var moduleAC = RF.Create<ModuleAC>().GetById(parentId);

            return this.GetByParent(moduleAC, false);
        }

        /// <summary>
        /// 获取整个模块可用的的所有功能列表
        /// </summary>
        /// <param name="boType"></param>
        /// <returns></returns>
        private OperationACList GetByModule(ModuleAC module)
        {
            var list = new OperationACList();

            var m = module.Core;
            foreach (var op in m.CustomOpertions)
            {
                list.Add(new OperationAC
                {
                    ScopeKeyLabel = OperationAC.ModuleScope,
                    OperationKey = op.Name,
                    Label = op.Label
                });
            }

            //模块的查看功能
            list.Add(new OperationAC
            {
                ScopeKeyLabel = OperationAC.ModuleScope,
                OperationKey = SystemOperationKeys.Read,
                Label = SystemOperationKeys.Read
            });

            //系统生成的界面，迭归生成功能列表
            if (!m.IsCustomUI)
            {
                var blocks = UIModel.AggtBlocks.GetModuleBlocks(m);
                this.GetByBlocksRecur(blocks, list);
            }

            return list;
        }

        private void GetByBlocksRecur(AggtBlocks blocks, OperationACList list)
        {
            var mainBlock = blocks.MainBlock;

            //查看，编辑
            list.Add(new OperationAC
            {
                ScopeKeyLabel = mainBlock.KeyLabel,
                OperationKey = SystemOperationKeys.Read,
                Label = SystemOperationKeys.Read
            });
            list.Add(new OperationAC
            {
                ScopeKeyLabel = mainBlock.KeyLabel,
                OperationKey = SystemOperationKeys.Edit,
                Label = SystemOperationKeys.Edit
            });

            if (OEAEnvironment.IsWeb)
            {
                //功能按钮权限
                foreach (var cmd in mainBlock.ViewMeta.WebCommands)
                {
                    list.Add(new OperationAC
                    {
                        ScopeKeyLabel = mainBlock.KeyLabel,
                        OperationKey = cmd.Name,
                        Label = cmd.Label
                    });
                }
            }
            else
            {
                //功能按钮权限
                foreach (var cmd in mainBlock.ViewMeta.WPFCommands)
                {
                    list.Add(new OperationAC
                    {
                        ScopeKeyLabel = mainBlock.KeyLabel,
                        OperationKey = cmd.Name,
                        Label = cmd.Label
                    });
                }
            }

            foreach (var surrounder in blocks.Surrounders)
            {
                this.GetByBlocksRecur(surrounder, list);
            }

            foreach (var child in blocks.Children)
            {
                this.GetByBlocksRecur(child, list);
            }
        }
    }

    internal class OperationACConfig : EntityConfig<OperationAC>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("权限控制项")
                .HasDelegate(OperationAC.OperationKeyProperty)
                .GroupBy(OperationAC.ScopeKeyLabelProperty);

            View.Property(OperationAC.LabelProperty).HasLabel("名称").ShowIn(ShowInWhere.List);
        }
    }
}