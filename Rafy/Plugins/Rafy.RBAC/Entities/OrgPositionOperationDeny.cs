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
    /// 岗位下的被禁用的每一个功能（命令）的权限
    /// </summary>
    [ChildEntity, Serializable]
    public partial class OrgPositionOperationDeny : IntEntity
    {
        #region 构造函数

        public OrgPositionOperationDeny() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OrgPositionOperationDeny(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty OrgPositionIdProperty =
            P<OrgPositionOperationDeny>.RegisterRefId(e => e.OrgPositionId, ReferenceType.Parent);
        public int OrgPositionId
        {
            get { return (int)this.GetRefId(OrgPositionIdProperty); }
            set { this.SetRefId(OrgPositionIdProperty, value); }
        }
        public static readonly RefEntityProperty<OrgPosition> OrgPositionProperty =
            P<OrgPositionOperationDeny>.RegisterRef(e => e.OrgPosition, OrgPositionIdProperty);
        /// <summary>
        /// 对应的岗位
        /// </summary>
        public OrgPosition OrgPosition
        {
            get { return this.GetRefEntity(OrgPositionProperty); }
            set { this.SetRefEntity(OrgPositionProperty, value); }
        }

        public static readonly Property<string> ModuleKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.ModuleKey);
        /// <summary>
        /// 对应的模块
        /// </summary>
        public string ModuleKey
        {
            get { return this.GetProperty(ModuleKeyProperty); }
            set { this.SetProperty(ModuleKeyProperty, value); }
        }

        public static readonly Property<string> BlockKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.BlockKey);
        /// <summary>
        /// 对应某个界面块。
        /// 
        /// 此属性如果为空字符串，表示当前的操作是针对整个模块的。
        /// </summary>
        public string BlockKey
        {
            get { return this.GetProperty(BlockKeyProperty); }
            set { this.SetProperty(BlockKeyProperty, value); }
        }

        public static readonly Property<string> OperationKeyProperty = P<OrgPositionOperationDeny>.Register(e => e.OperationKey);
        /// <summary>
        /// 对应的功能的主键
        /// </summary>
        public string OperationKey
        {
            get { return this.GetProperty(OperationKeyProperty); }
            set { this.SetProperty(OperationKeyProperty, value); }
        }
    }

    [Serializable]
    public partial class OrgPositionOperationDenyList : EntityList { }

    public partial class OrgPositionOperationDenyRepository : EntityRepository
    {
        protected OrgPositionOperationDenyRepository() { }
    }

    internal class OrgPositionOperationDenyConfig : EntityConfig<OrgPositionOperationDeny>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                OrgPositionOperationDeny.OrgPositionIdProperty,
                OrgPositionOperationDeny.ModuleKeyProperty,
                OrgPositionOperationDeny.BlockKeyProperty,
                OrgPositionOperationDeny.OperationKeyProperty
                );
        }
    }
}