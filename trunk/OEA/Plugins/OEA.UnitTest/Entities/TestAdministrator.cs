using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;

using OEA.MetaModel.View;

namespace OEA.Library._Test
{
    [Serializable]
    [RootEntity, Label("单元测试 - 管理员")]
    [Table("User")]
    public class TestAdministrator : TestUser
    {
        public static readonly Property<int?> LevelProperty = P<TestAdministrator>.Register(e => e.Level);
        [EntityProperty]
        public int? Level
        {
            get { return this.GetProperty(LevelProperty); }
            set { this.SetProperty(LevelProperty, value); }
        }
    }

    [Serializable]
    public class TestAdministratorList : TestUserList { }

    internal class TestAdministratorConfig : EntityConfig<TestAdministrator>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.HasColumns(TestAdministrator.LevelProperty);
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.Property(TestAdministrator.LevelProperty).ShowIn(ShowInWhere.All).HasLabel("管理员级别");
        }
    }
}