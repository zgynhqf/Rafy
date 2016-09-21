using System;
using System.IO;
using System.Text;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.LicenseManager.Entities;
using Rafy.Security;

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

        //internal static void GeneratorPublicPrivateKey()
        //{
        //    var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        //    if(!Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);

        //    var privateKeyPath = Path.Combine(dir, "private.bin");
        //    var publicKeyPath = Path.Combine(dir, "public.bin");

        //    if(File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
        //        return;

        //    var keys = RSACryptoService.GenerateKeys();

        //    File.WriteAllText(privateKeyPath, keys[0], Encoding.UTF8);
        //    File.WriteAllText(publicKeyPath, keys[1], Encoding.UTF8);
        //}
    }
}