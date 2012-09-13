/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110928
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110928
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Server;
using System.Windows;
using System.Windows.Threading;
using OEA.MetaModel;
using OEA;
using Common;

namespace OEAUnitTest
{
    public class TestApp : AppImplementationBase
    {
        internal void Start()
        {
            OEAEnvironment.Provider.IsDebuggingEnabled = true;

            this.OnAppStartup();
        }

        protected override void StartMainProcess() { }
    }
}