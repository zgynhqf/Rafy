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

        /// <summary>
        /// 判断当前的这个操作是否和指定的禁用操作是同一个操作。
        /// </summary>
        /// <param name="deny"></param>
        /// <returns></returns>
        public bool IsSame(OrgPositionOperationDeny deny)
        {
            //模块名、功能名、所属界面块同时相同
            return deny.ModuleKey == this.ModuleAC.KeyLabel && deny.OperationKey == this.OperationKey &&
                                    (string.IsNullOrEmpty(deny.BlockKey) && this.ScopeKeyLabel == OperationAC.ModuleScope || deny.BlockKey == this.ScopeKeyLabel);
        }
    }

    public class OperationACList : EntityList { }

    /// <summary>
    /// OperationAc 这个类并不映射数据库，所以所有的查询方法都是在 Repository 中实现的。
    /// </summary>
    public class OperationACRepository : MemoryEntityRepository
    {
        protected OperationACRepository() { }

        protected override string GetRealKey(Entity entity)
        {
            var op = entity as OperationAC;
            if (op.ModuleAC == null) return string.Empty;
            return op.ModuleAC.KeyLabel + "_" + op.ScopeKeyLabel + "_" + op.OperationKey;
        }

        /// <summary>
        /// 获取某个模块下的所有可用功能列表（WPF 模式下会调用此方法。）
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="withCache"></param>
        /// <returns></returns>
        public override EntityList GetByParent(Entity parent, bool withCache = true)
        {
            var list = this.GetByModule(parent as ModuleAC);

            list.SetParentEntity(parent);

            this.NotifyLoaded(list);

            return list;
        }

        /// <summary>
        /// 获取某个模块下的所有可用功能列表（Web 模式下会调用此方法。）
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public override EntityList GetByParentId(int parentId)
        {
            var moduleAC = RF.Create<ModuleAC>().GetById(parentId);

            return this.GetByParent(moduleAC, false);
        }

        protected override EntityList GetBy(object criteria)
        {
            if (criteria is OperationAC_GetDenyListCriteria)
            {
                return this.GetBy(criteria as OperationAC_GetDenyListCriteria);
            }

            return base.GetBy(criteria);
        }

        /// <summary>
        /// 查找某个岗位下某个指定模块的禁用功能。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private EntityList GetBy(OperationAC_GetDenyListCriteria c)
        {
            var opId = c.OrgPositionId;

            var moduleAC = RF.Create<ModuleAC>().GetById(c.ModuleACId);
            var operations = this.GetByParent(moduleAC, false);

            //把所有已经禁用的功能都加入到列表中去。并把这个返回
            var list = this.NewList();
            var op = RF.Create<OrgPosition>().GetById(opId) as OrgPosition;
            var denyList = op.OrgPositionOperationDenyList;
            list.AddRange(operations.Cast<OperationAC>().Where(item =>
            {
                foreach (OrgPositionOperationDeny deny in denyList)
                {
                    if (item.IsSame(deny))
                    {
                        return true;
                    }
                }

                return false;
            }));
            return list;
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

    [Criteria, Serializable]
    public class OperationAC_GetDenyListCriteria : Criteria
    {
        public static readonly Property<int> ModuleACIdProperty = P<OperationAC_GetDenyListCriteria>.Register(e => e.ModuleACId);
        public int ModuleACId
        {
            get { return this.GetProperty(ModuleACIdProperty); }
            set { this.SetProperty(ModuleACIdProperty, value); }
        }

        public static readonly Property<int> OrgPositionIdProperty = P<OperationAC_GetDenyListCriteria>.Register(e => e.OrgPositionId);
        public int OrgPositionId
        {
            get { return this.GetProperty(OrgPositionIdProperty); }
            set { this.SetProperty(OrgPositionIdProperty, value); }
        }
    }

    internal class OperationACConfig : EntityConfig<OperationAC>
    {
        protected override void ConfigView()
        {
            View.DisableEditing();

            if (IsWeb)
            {
                View.WithoutPaging();
            }

            View.DomainName("权限控制项")
                .HasDelegate(OperationAC.OperationKeyProperty)
                .GroupBy(OperationAC.ScopeKeyLabelProperty);

            View.Property(OperationAC.LabelProperty).HasLabel("名称").ShowIn(ShowInWhere.List);
        }
    }
}