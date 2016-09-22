/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20160921 10:27
 * 
*******************************************************/


using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Rafy.Security
{
    /// <summary>
    ///     获取网卡的mac地址
    /// </summary>
    public static class ComputerMacUtils
    {
        /// <summary>
        ///     通过NetworkInterface读取网卡Mac
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMacByNetworkInterface()
        {
            var macs = new HashSet<string>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in interfaces)
            {
                var mac = ni.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(mac))
                {
                    macs.Add(mac.Replace(":", "").Replace("-", "").ToLower());
                }
            }
            return macs.ToList();
        }
    }
}