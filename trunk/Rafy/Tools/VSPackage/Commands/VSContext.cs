/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 16:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Rafy.VSPackage
{
    public abstract class VSContext
    {
        protected internal Package Package { get; internal set; }

        protected internal IServiceProvider ServiceProvider
        {
            get { return this.Package; }
        }

        protected DTE DTE
        {
            get { return this.GetService(typeof(DTE)) as DTE; }
        }

        protected object GetService(Type type)
        {
            return this.ServiceProvider.GetService(type);
        }
    }
}
