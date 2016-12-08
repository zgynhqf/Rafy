using Rafy.ComponentModel;
using Rafy.Domain;

namespace Rafy.RBAC.UserRoleManagement
{
    public class UserRoleManagementPlugin : DomainPlugin
    {
        private static string _dbSettingName;
        /// <summary>
        /// 本插件中所有实体对应的连接字符串的配置名。
        /// 如果没有设置，则默认使用 <see cref="DbSettingNames.RafyPlugins"/>。
        /// </summary>
        public static string DbSettingName
        {
            get { return _dbSettingName ?? DbSettingNames.RafyPlugins; }
            set { _dbSettingName = value; }
        }

        public override void Initialize(IApp app)
        {
        }
    }
}
