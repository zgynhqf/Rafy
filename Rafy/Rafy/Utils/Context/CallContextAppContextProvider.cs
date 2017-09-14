/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170914
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170914 14:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Rafy
{
    /// <summary>
    /// 使用 ExecutionContext 中的 CallContext 来实现跨线程数据共享的 ContextProvider。
    /// 
    /// CallContext 相关的文档见：
    /// http://blog.csdn.net/wsxqaz/article/details/9083093
    /// 
    /// 注意：
    /// 目前此类存在以下问题，所以暂时不使用：
    /// 使用后，单元测试无法正常运行了。详见：http://www.cnblogs.com/artech/archive/2010/08/29/1811683.html
    /// </summary>
    public class CallContextAppContextProvider : IAppContextProvider
    {
        private static readonly string CurrentPrincipalName = "Rafy.CallContextAppContextProvider.CurrentPrincipal";
        private static readonly string DataContainerName = "Rafy.CallContextAppContextProvider.DataContainer";

        public IPrincipal CurrentPrincipal
        {
            get => CallContext.LogicalGetData(CurrentPrincipalName) as IPrincipal;
            set => CallContext.LogicalSetData(CurrentPrincipalName, value);
        }

        public IDictionary<string, object> DataContainer
        {
            get => CallContext.LogicalGetData(DataContainerName) as IDictionary<string, object>;
            set => CallContext.LogicalSetData(DataContainerName, value);
        }
    }
}
