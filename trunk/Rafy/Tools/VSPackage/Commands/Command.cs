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
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Rafy.VSPackage
{
    /// <summary>
    /// 命令基类。
    /// </summary>
    public abstract class Command : VSContext
    {
        public CommandID CommandID { get; set; }

        private RafyMenuCommand _menuCommand;

        public MenuCommand MenuCommand
        {
            get
            {
                if (_menuCommand == null)
                {
                    _menuCommand = new RafyMenuCommand(this);
                    _menuCommand.BeforeQueryStatus += (o, e) => this.OnQueryStatus();
                }

                return _menuCommand;
            }
        }

        protected virtual void OnQueryStatus() { }

        protected abstract void ExecuteCore();

        internal void OnExecute(object sender, EventArgs e)
        {
            try
            {
                this.ExecuteCore();
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作执行出现异常：" + ex.Message, "错误");
            }
        }

        internal void Initialize()
        {
            this.InitializeCore();
        }

        protected virtual void InitializeCore() { }
    }

    internal class RafyMenuCommand : OleMenuCommand
    {
        private Command _inner;

        public RafyMenuCommand(Command inner)
            : base(inner.OnExecute, inner.CommandID)
        {
            _inner = inner;
        }
    }
}