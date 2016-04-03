/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100925
 * 说明：实体约束类，从Library中往下层移动到MetaModel中。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100925
 * 约定项添加RepositoryType 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Threading;
using Rafy.Utils;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体类的约定
    /// </summary>
    public static class EntityConvention
    {
        /// <summary>
        /// 实体仓库数据层查询根据参数类型进行定位时的方法的约定名称。
        /// </summary>
        public const string GetByCriteriaMethod = "GetBy";

        ///// <summary>
        ///// 目前实体使用的主键类型。Int。
        ///// </summary>
        //public static readonly Type IdType = typeof(int);

        /// <summary>
        /// 目前实体使用的主键属性的名称。Id。
        /// </summary>
        internal static IManagedProperty Property_Id;

        /// <summary>
        /// 自关联属性名
        /// </summary>
        internal static IManagedProperty Property_TreePId;

        /// <summary>
        /// 树型实体的编码
        /// </summary>
        internal static IManagedProperty Property_TreeIndex;

        /// <summary>
        /// 实体的假删除标识。
        /// 如果没有使用假删除插件，那么这个属性为 null。
        /// </summary>
        internal static IManagedProperty Property_IsPhantom;

        /// <summary>
        /// 是否添加了数据的假删除功能插件。
        /// </summary>
        internal static bool IsPhantomPluginEnabled
        {
            get { return Property_IsPhantom != null; }
        }

        internal static string IdColumnName
        {
            get { return Property_Id.Name; }
        }

        internal static string TreeChildrenPropertyName = "TreeChildren";

        ///// <summary>
        ///// 目前实体使用的主键属性的名称。Id。
        ///// </summary>
        //public const string Property_Id = "Id";

        ///// <summary>
        ///// 自关联属性名
        ///// </summary>
        //public const string Property_TreePId = "TreePId";

        ///// <summary>
        ///// 树型实体的编码
        ///// </summary>
        //public const string Property_TreeCode = "TreeCode";
    }
}
