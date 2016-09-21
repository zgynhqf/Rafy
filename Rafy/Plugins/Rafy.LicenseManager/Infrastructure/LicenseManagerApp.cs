using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.LicenseManager.Entities;

namespace Rafy.LicenseManager.Infrastructure
{
    internal class LicenseManagerApp : DomainApp
    {
        protected override void InitEnvironment()
        {
            RafyEnvironment.Provider.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("IsDebuggingEnabled", false);

            RafyEnvironment.DomainPlugins.Add(new LicenseManagerPlugin());

            base.InitEnvironment();
        }
    }
}