/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120418
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120418
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using System.Transactions;
using Rafy.Domain;

namespace JXC
{
    [Serializable]
    public abstract class AddService : FlowService
    {
        [ServiceInput]
        public Entity Item { get; set; }

        [ServiceOutput]
        public int NewId { get; set; }
    }

    [Serializable]
    public abstract class DeleteService : FlowService
    {
        [ServiceInput]
        public int ItemId { get; set; }
    }
}