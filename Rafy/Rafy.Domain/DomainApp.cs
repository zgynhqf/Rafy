/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130405 23:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130405 23:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 领域模型启动环境
    /// </summary>
    public class DomainApp : AppImplementationBase
    {
        public void Startup()
        {
            try
            {
                this.StartupApplication();
            }
            catch (Exception ex)
            {
                Logger.LogError("领域模型启动时发生异常", ex);
                throw;
            }
        }
    }
}
