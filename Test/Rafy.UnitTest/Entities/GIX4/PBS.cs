/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 15:45
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 15:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    [ChildEntity]
    public partial class PBS : UnitTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty PBSTypeIdProperty =
            P<PBS>.RegisterRefId(e => e.PBSTypeId, ReferenceType.Parent);
        public int PBSTypeId
        {
            get { return this.GetRefId(PBSTypeIdProperty); }
            set { this.SetRefId(PBSTypeIdProperty, value); }
        }
        public static readonly RefEntityProperty<PBSType> PBSTypeProperty =
            P<PBS>.RegisterRef(e => e.PBSType, PBSTypeIdProperty);
        public PBSType PBSType
        {
            get { return this.GetRefEntity(PBSTypeProperty); }
            set { this.SetRefEntity(PBSTypeProperty, value); }
        }

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<PBS>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    public partial class PBSList : UnitTestEntityList { }

    public partial class PBSRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected PBSRepository() { }
    }

    internal class PBSConfig : UnitTestEntityConfig<PBS>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            //大于 1000，使用的是 ScopeByRoot 缓存。
            Meta.EnableClientCache(2000);

            Meta.EnableServerCache();
        }
    }
}