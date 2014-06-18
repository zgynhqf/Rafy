/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120821 14:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120821 14:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;

namespace JXC
{
    /// <summary>
    /// 下拉框通用的一些数据源
    /// </summary>
    public static class EntityDataSources
    {
        public static ClientInfoList Suppliers()
        {
            return RF.Concrete<ClientInfoRepository>().GetSuppliers();
        }

        public static ClientInfoList Customers()
        {
            return RF.Concrete<ClientInfoRepository>().GetCustomers();
        }
    }
}
