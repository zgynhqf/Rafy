using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace UT
{
    /// <summary>
    /// 这个类可以写在 721 中 2 的包中。
    /// </summary>
    [CompiledPropertyDeclarer]
    public static class TestUserExt
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        public static ManagedProperty<string> UserCodeProperty =
            P<TestUser>.RegisterExtension("TestUserExt_UserCode", typeof(TestUserExt), "DefaultUserCode");
        public static string GetUserCode(TestUser entity)
        {
            return entity.GetProperty(UserCodeProperty);
        }
        public static void SetUserCode(TestUser entity, string value)
        {
            entity.SetProperty(UserCodeProperty, value);
        }

        public static ManagedProperty<string> ReadOnlyUserCodeProperty =
            P<TestUser>.RegisterExtensionReadOnly("TestUserExt_ReadOnlyUserCode", typeof(TestUserExt), ReadOnlyUserCodeProperty_GetValue, UserCodeProperty);
        public static string GetReadOnlyUserCode(TestUser entity)
        {
            return entity.GetProperty(ReadOnlyUserCodeProperty);
        }
        private static string ReadOnlyUserCodeProperty_GetValue(TestUser user)
        {
            return GetUserCode(user) + " ReadOnly!";
        }

        public static ManagedProperty<string> ReadOnlyUserCodeShadowProperty =
            P<TestUser>.RegisterExtensionReadOnly("TestUserExt_ReadOnlyUserCodeShadow", typeof(TestUserExt), ReadOnlyUserCodeShadow_GetValue, ReadOnlyUserCodeProperty);
        public static string GetReadOnlyUserCodeShadow(TestUser entity)
        {
            return entity.GetProperty(ReadOnlyUserCodeShadowProperty);
        }
        private static string ReadOnlyUserCodeShadow_GetValue(TestUser user)
        {
            return GetReadOnlyUserCode(user);
        }

        #region TestUserLogList TestUserLogList (用户行为日志)

        /// <summary>
        /// 用户行为日志 扩展属性。
        /// </summary>
        public static ListProperty<TestUserLogList> TestUserLogListProperty =
            P<TestUser>.RegisterListExtension<TestUserLogList>("TestUserLogList", typeof(TestUserExt));
        /// <summary>
        /// 获取 用户行为日志 属性的值。
        /// </summary>
        /// <param name="me">要获取扩展属性值的对象。</param>
        public static TestUserLogList GetTestUserLogList(this TestUser me)
        {
            return me.GetLazyList(TestUserLogListProperty) as TestUserLogList;
        }

        #endregion

        #region string ANameExt (扩展的 AName)

        /// <summary>
        /// 扩展的 AName 扩展属性。
        /// </summary>
        public static Property<string> ANameExtProperty = P<B>.RegisterRedundancyExtension<string>("ANameExt", typeof(TestUserExt),
            new RedundantPath(B.AProperty, A.NameProperty));
        /// <summary>
        /// 获取 扩展的 AName 属性的值。
        /// </summary>
        /// <param name="me">要获取扩展属性值的对象。</param>
        public static string GetANameExt(this B me)
        {
            return me.GetProperty(ANameExtProperty);
        }

        #endregion
    }

    internal class TestUserExtConfig : EntityConfig<TestUser>
    {
        protected override void ConfigMeta()
        {
            Meta.Property(TestUserExt.UserCodeProperty).MapColumn();
        }
    }
}
