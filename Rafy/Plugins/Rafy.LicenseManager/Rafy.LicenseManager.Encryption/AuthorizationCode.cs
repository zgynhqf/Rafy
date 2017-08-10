/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20160921 11:04
 * 
*******************************************************/


using System;

namespace Rafy.LicenseManager.Encryption
{
    /// <summary>
    /// 授权信息
    /// </summary>
    public class AuthorizationCode
    {
        /// <summary>
        /// 校验码，替换之前的网卡物理地址
        /// </summary>
        public string CheckCode { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 授权类别 0=开发 1=产品
        /// </summary>
        public int Category { get; set; }
    }
}