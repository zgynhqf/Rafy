/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Web;

namespace OEA
{
    /// <summary>
    /// 客户端实体的序列化器
    /// </summary>
    public static class ClientEntityConverter
    {
        public static Type ToClientType(string clientName)
        {
            if (OEAEnvironment.IsWeb)
            {
                return ClientEntities.Find(clientName).EntityType;
            }

            return Type.GetType(clientName);
        }

        public static string ToClientName(Type clientType)
        {
            if (OEAEnvironment.IsWeb)
            {
                return ClientEntities.GetClientName(clientType);
            }

            return clientType.AssemblyQualifiedName;
        }
    }
}