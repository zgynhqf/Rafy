/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170918
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170918 14:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Rafy
{
    /// <summary>
    /// 使用 AsyncLocal 来实现跨线程的数据共享的 AppContextProvider。
    /// </summary>
    public class AsyncLocalAppContextProvider : IAppContextProvider
    {
        private AsyncLocal<IPrincipal> _currentPrincipalValue = new AsyncLocal<IPrincipal>();
        private AsyncLocal<IDictionary<string, object>> _dataContainerValue = new AsyncLocal<IDictionary<string, object>>();

        public IPrincipal CurrentPrincipal
        {
            get => _currentPrincipalValue.Value;
            set => _currentPrincipalValue.Value = value;
        }

        public IDictionary<string, object> DataContainer
        {
            get => _dataContainerValue.Value;
            set => _dataContainerValue.Value = value;
        }
    }
}
