/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130318 15:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130318 15:35
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
    [RootEntity, Serializable]
    public partial class SectionOwner : UnitTestEntity
    {
        #region 构造函数

        public SectionOwner() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected SectionOwner(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<SectionOwner>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public partial class SectionOwnerList : UnitTestEntityList { }

    public partial class SectionOwnerRepository : UnitTestEntityRepository
    {
        protected SectionOwnerRepository() { }

        #region 数据访问

        #endregion
    }

    internal class SectionOwnerConfig : UnitTestEntityConfig<SectionOwner>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}