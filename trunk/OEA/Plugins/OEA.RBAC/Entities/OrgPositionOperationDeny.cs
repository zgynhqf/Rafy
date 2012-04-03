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
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace OEA.RBAC
{
    /// <summary>
    /// 岗位下的被禁用的每一个功能（命令）的权限
    /// </summary>
    [ChildEntity, Serializable]
    public class OrgPositionOperationDeny : Entity
    {
        /// <summary>
        /// 对应的岗位
        /// </summary>
        public static readonly RefProperty<OrgPosition> OrgPositionRefProperty =
            P<OrgPositionOperationDeny>.RegisterRef(e => e.OrgPosition, ReferenceType.Parent);
        public int OrgPositionId
        {
            get { return this.GetRefId(OrgPositionRefProperty); }
            set { this.SetRefId(OrgPositionRefProperty, value); }
        }
        public OrgPosition OrgPosition
        {
            get { return this.GetRefEntity(OrgPositionRefProperty); }
            set { this.SetRefEntity(OrgPositionRefProperty, value); }
        }

        /// <summary>
        /// 对应的模块
        /// </summary>
        public static readonly Property<string> ModuleKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.ModuleKey);
        public string ModuleKey
        {
            get { return this.GetProperty(ModuleKeyProperty); }
            set { this.SetProperty(ModuleKeyProperty, value); }
        }

        /// <summary>
        /// 对应某个界面块。
        /// 
        /// 此属性如果为空字符串，表示当前的操作是针对整个模块的。
        /// </summary>
        public static readonly Property<string> BlockKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.BlockKey);
        public string BlockKey
        {
            get { return this.GetProperty(BlockKeyProperty); }
            set { this.SetProperty(BlockKeyProperty, value); }
        }

        /// <summary>
        /// 对应的功能的主键
        /// </summary>
        public static readonly Property<string> OperationKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.OperationKey);
        public string OperationKey
        {
            get { return this.GetProperty(OperationKeyProperty); }
            set { this.SetProperty(OperationKeyProperty, value); }
        }
    }

    [Serializable]
    public class OrgPositionOperationDenyList : EntityList { }

    public class OrgPositionOperationDenyRepository : EntityRepository
    {
        protected OrgPositionOperationDenyRepository() { }
    }

    internal class OrgPositionOperationDenyConfig : EntityConfig<OrgPositionOperationDeny>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                OrgPositionOperationDeny.OrgPositionRefProperty,
                OrgPositionOperationDeny.ModuleKeyProperty,
                OrgPositionOperationDeny.BlockKeyProperty,
                OrgPositionOperationDeny.OperationKeyProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("功能权限").ClearWPFCommands()
                .UseWPFCommands("RBAC.Command.ExpandAllModules");
        }
    }
}