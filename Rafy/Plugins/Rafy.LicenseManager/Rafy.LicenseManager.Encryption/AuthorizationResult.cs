﻿/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20160921 11:45
 * 
*******************************************************/


namespace Rafy.LicenseManager.Encryption
{
    public class AuthorizationResult
    {
        public bool Success => AuthorizationState == AuthorizationState.Success;
        public AuthorizationState AuthorizationState { get; set; }
    }

    public enum AuthorizationState
    {
        //授权通过
        Success = 0,
        //网卡错误
        MacError = 1,
        //授权过期
        Expire = 2
    }
}