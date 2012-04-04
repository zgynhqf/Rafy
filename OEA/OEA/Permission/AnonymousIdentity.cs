/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace OEA
{
    public class AnonymousIdentity : GenericIdentity, IUser
    {
        public static readonly AnonymousIdentity Instance = new AnonymousIdentity();

        private AnonymousIdentity() : base(string.Empty) { }
    }
}