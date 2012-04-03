/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120403
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120403
 * 
*******************************************************/

using System;
using OEA;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.RBAC
{
    /// <summary>
    /// Criteria class for passing a
    /// username/password pair to a
    /// custom identity class.
    /// </summary>
    [Serializable]
    public class UsernameCriteria : Criteria
    {
        public static readonly Property<string> UsernameProperty = P<UsernameCriteria>.Register(e => e.Username);
        public string Username
        {
            get { return this.GetProperty(UsernameProperty); }
            set { this.SetProperty(UsernameProperty, value); }
        }

        public static readonly Property<string> PasswordProperty = P<UsernameCriteria>.Register(e => e.Password);
        public string Password
        {
            get { return this.GetProperty(PasswordProperty); }
            set { this.SetProperty(PasswordProperty, value); }
        }
    }
}