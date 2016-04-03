/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.Domain
{
    /// <summary>
    /// 一种过程化服务的基类
    /// 
    /// 过程化简单地指：进行一系列操作，返回是否成功以及相应的提示消息。
    /// </summary>
    [Serializable]
    public abstract class FlowService : Service, IFlowService
    {
        [ServiceOutput]
        public Result Result { get; set; }

        protected override sealed void Execute()
        {
            this.Result = this.ExecuteCore();
        }

        protected abstract Result ExecuteCore();
    }
}