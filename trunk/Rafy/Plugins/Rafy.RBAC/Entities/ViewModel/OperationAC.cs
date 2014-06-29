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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Rafy.RBAC
{
    /// <summary>
    /// 操作控制
    /// （Operation Access Control）
    /// 功能元数据的显示模型。表示某个模块下的某个功能。
    /// 
    /// 功能主要有两种：
    /// 主要是命令，其次是一些自定义功能。
    /// 
    /// 由 Module + Scope + OperationKey 构成主键
    /// </summary>
    [DebuggerDisplay("Label:{Label}, OperationKey:{OperationKey}")]
    [ChildEntity]
    public partial class OperationAC : IntEntity
    {
        /// <summary>
        /// 模块功能的范围名称。
        /// <remarks>
        /// 由于 Web 权限在分组后会按照分组的名称做排序，为了保证模块功能在最上面，所以给模块功能以空格开头。
        /// </remarks>
        /// </summary>
        public const string ModuleScope = " 模块功能";

        public static readonly IRefIdProperty ModuleACIdProperty =
            P<OperationAC>.RegisterRefId(e => e.ModuleACId, ReferenceType.Parent);
        public int ModuleACId
        {
            get { return (int)this.GetRefId(ModuleACIdProperty); }
            set { this.SetRefId(ModuleACIdProperty, value); }
        }
        public static readonly RefEntityProperty<ModuleAC> ModuleACProperty =
            P<OperationAC>.RegisterRef(e => e.ModuleAC, ModuleACIdProperty);
        /// <summary>
        /// 所属模块
        /// </summary>
        public ModuleAC ModuleAC
        {
            get { return this.GetRefEntity(ModuleACProperty); }
            set { this.SetRefEntity(ModuleACProperty, value); }
        }

        public static readonly Property<string> ScopeKeyLabelProperty = P<OperationAC>.Register(e => e.ScopeKeyLabel);
        /// <summary>
        /// 当前功能所属的范围。
        /// 如果是模块，则这个属性应该是 OperationAC.ModuleScope。
        /// 如果是某个界面块，则这个属性应该界面块的键。
        /// </summary>
        public string ScopeKeyLabel
        {
            get { return this.GetProperty(ScopeKeyLabelProperty); }
            set { this.SetProperty(ScopeKeyLabelProperty, value); }
        }

        public static readonly Property<string> OperationKeyProperty = P<OperationAC>.Register(e => e.OperationKey);
        /// <summary>
        /// 功能名称
        /// </summary>
        public string OperationKey
        {
            get { return this.GetProperty(OperationKeyProperty); }
            set { this.SetProperty(OperationKeyProperty, value); }
        }

        public static readonly Property<string> LabelProperty = P<OperationAC>.Register(e => e.Label);
        /// <summary>
        /// 名称
        /// </summary>
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
            return deny.ModuleKey.Translate() == this.ModuleAC.KeyLabel && deny.OperationKey == this.OperationKey &&
                (string.IsNullOrEmpty(deny.BlockKey) && this.ScopeKeyLabel == OperationAC.ModuleScope.Translate() || deny.BlockKey.Translate() == this.ScopeKeyLabel);
        }
    }

    public partial class OperationACList : EntityList { }

    /// <summary>
    /// OperationAc 这个类并不映射数据库，所以所有的查询方法都是在 Repository 中实现的。
    /// </summary>
    public partial class OperationACRepository : MemoryEntityRepository
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
        protected override EntityList DoGetByParent(Entity parent)
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
        protected override EntityList DoGetByParentId(object parentId, PagingInfo pagingInfo)
        {
            var moduleAC = RF.Find<ModuleAC>().GetById(parentId);

            return this.DoGetByParent(moduleAC);
        }

        protected override EntityList DoGetBy(object criteria)
        {
            if (criteria is OperationAC_GetDenyListCriteria)
            {
                return this.GetBy(criteria as OperationAC_GetDenyListCriteria);
            }

            return base.DoGetBy(criteria);
        }

        /// <summary>
        /// 查找某个岗位下某个指定模块的禁用功能。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private EntityList GetBy(OperationAC_GetDenyListCriteria c)
        {
            var opId = c.OrgPositionId;

            var moduleAC = RF.Concrete<ModuleACRepository>().GetById(c.ModuleACId);
            var operations = this.DoGetByParent(moduleAC);

            //把所有已经禁用的功能都加入到列表中去。并把这个返回
            var list = this.NewList();
            var op = RF.Concrete<OrgPositionRepository>().GetById(opId) as OrgPosition;
            var denyList = op.OrgPositionOperationDenyList;
            list.AddRange(operations.Cast<OperationAC>().Where(item =>
            {
                foreach (var deny in denyList)
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
            var moduleScopeTranslated = OperationAC.ModuleScope.Translate();

            var list = new OperationACList();

            var m = module.Core;
            foreach (var op in m.CustomOpertions)
            {
                list.Add(new OperationAC
                {
                    ScopeKeyLabel = moduleScopeTranslated,
                    OperationKey = op.Name,
                    Label = op.Label.Translate()
                });
            }

            //模块的查看功能
            list.Add(new OperationAC
            {
                ScopeKeyLabel = moduleScopeTranslated,
                OperationKey = SystemOperationKeys.Read,
                Label = SystemOperationKeys.Read.Translate(),
            });

            //系统生成的界面，迭归生成功能列表
            if (!m.IsCustomUI)
            {
                var blocks = UIModel.AggtBlocks.GetModuleBlocks(m);
                this.GetByBlocksRecur(blocks, list);
            }

            return list;
        }

        /// <summary>
        /// 递归获取某个聚合块中所有可用的操作列表
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="list"></param>
        private void GetByBlocksRecur(AggtBlocks blocks, OperationACList list)
        {
            var mainBlock = blocks.MainBlock;

            //查看，编辑
            list.Add(new OperationAC
            {
                ScopeKeyLabel = mainBlock.KeyLabel.Translate(),
                OperationKey = SystemOperationKeys.Read,
                Label = SystemOperationKeys.Read.Translate(),
            });
            //list.Add(new OperationAC
            //{
            //    ScopeKeyLabel = mainBlock.KeyLabel.Translate(),
            //    OperationKey = SystemOperationKeys.Edit,
            //    Label = SystemOperationKeys.Edit.Translate(),
            //});

            if (RafyEnvironment.Location.IsWebUI)
            {
                //功能按钮权限
                foreach (var cmd in mainBlock.ViewMeta.AsWebView().Commands)
                {
                    list.Add(new OperationAC
                    {
                        ScopeKeyLabel = mainBlock.KeyLabel.Translate(),
                        OperationKey = cmd.Name,
                        Label = cmd.Label.Translate(),
                    });
                }
            }
            else
            {
                //功能按钮权限
                foreach (var cmd in mainBlock.ViewMeta.AsWPFView().Commands)
                {
                    list.Add(new OperationAC
                    {
                        ScopeKeyLabel = mainBlock.KeyLabel.Translate(),
                        OperationKey = cmd.Name,
                        Label = cmd.Label.Translate(),
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

    [DataProviderFor(typeof(OperationACRepository))]
    public partial class OperationACDataProvider : MemoryEntityRepository.MemoryRepositoryDataProvider
    {
        protected override IEnumerable<Entity> LoadAll()
        {
            return Enumerable.Empty<OperationAC>();
        }
    }

    [QueryEntity, Serializable]
    public partial class OperationAC_GetDenyListCriteria : Criteria
    {
        #region 构造函数

        public OperationAC_GetDenyListCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OperationAC_GetDenyListCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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
}