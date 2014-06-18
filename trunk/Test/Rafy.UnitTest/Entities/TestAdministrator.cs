using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace UT
{
    [Serializable]
    [RootEntity, Label("单元测试 - 管理员")]
    public partial class TestAdministrator : TestUser
    {
        #region 构造函数

        public TestAdministrator() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TestAdministrator(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<int?> LevelProperty = P<TestAdministrator>.Register(e => e.Level);
        public int? Level
        {
            get { return this.GetProperty(LevelProperty); }
            set { this.SetProperty(LevelProperty, value); }
        }
    }

    [Serializable]
    public partial class TestAdministratorList : TestUserList { }

    public partial class TestAdministratorRepository : TestUserRepository { }

    internal class TestAdministratorConfig : EntityConfig<TestAdministrator>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable("Users").MapProperties(TestAdministrator.LevelProperty);
        }
    }
}