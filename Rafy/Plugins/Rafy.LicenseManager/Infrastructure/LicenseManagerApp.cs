/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20160921 10:00
 * 
*******************************************************/

using Rafy.Domain;

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