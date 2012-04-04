using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using OEA.MetaModel;

using OEA.MetaModel.View;

namespace OEA.Library._Test
{
    /// <summary>
    /// 这个类可以写在 721 中 2 的包中。
    /// </summary>
    public class TestUserExt : IManagedPropertyDeclarer
    {
        public static ManagedProperty<string> UserCodeProperty = P<TestUser>.RegisterExtension("UserCode", null, "DefaultUserCode");
        public static string GetUserCode(TestUser entity)
        {
            return entity.GetProperty(UserCodeProperty);
        }
        public static void SetUserCode(TestUser entity, string value)
        {
            entity.SetProperty(UserCodeProperty, value);
        }

        public static ManagedProperty<string> ReadOnlyUserCodeProperty = P<TestUser>.RegisterExtensionReadOnly("ReadOnlyUserCode", ReadOnlyUserCodeProperty_GetValue, null, UserCodeProperty);
        public static string GetReadOnlyUserCode(TestUser entity)
        {
            return entity.GetProperty(ReadOnlyUserCodeProperty);
        }
        private static string ReadOnlyUserCodeProperty_GetValue(TestUser user)
        {
            return GetUserCode(user) + " ReadOnly!";
        }

        public static ManagedProperty<string> ReadOnlyUserCodeShadowProperty = P<TestUser>.RegisterExtensionReadOnly("ReadOnlyUserCodeShadow", ReadOnlyUserCodeShadow_GetValue, null, ReadOnlyUserCodeProperty);
        public static string GetReadOnlyUserCodeShadow(TestUser entity)
        {
            return entity.GetProperty(ReadOnlyUserCodeShadowProperty);
        }
        private static string ReadOnlyUserCodeShadow_GetValue(TestUser user)
        {
            return GetReadOnlyUserCode(user);
        }
    }

    internal class TestUserExtConfig : EntityConfig<TestUser>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.Property(TestUserExt.UserCodeProperty)
                .MapColumn(true);
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.Property(TestUserExt.UserCodeProperty).ShowIn(ShowInWhere.List).HasLabel("用户扩展编码");
        }
    }
}
