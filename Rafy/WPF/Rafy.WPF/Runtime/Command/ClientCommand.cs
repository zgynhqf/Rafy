/*******************************************************
 * 
 * 作者：周金根
 * 创建时间：20100101
 * 说明：运行时命令
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 20100101
 * 统一扩展参数的形式 胡庆访 20101216
 * 把元数据从此类中分离出去 胡庆访 20110215
 * 命名从CommandBase修改为ClientCommand，防止冲突。 胡庆访 20110308
 * 添加 Executed、ExecuteFailed 两个事件。 胡庆访 20110311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy;
using Rafy.Reflection;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 所有命令的基类
    /// </summary>
    public abstract class ClientCommand : Extendable, IClientCommand, INotifyPropertyChanged
    {
        #region 私有字段

        private WPFCommand _meta;

        private string _label;

        private bool _isVisible = true;

        private UICommand _uiCommand;

        private List<FrameworkElement> _uiElements = new List<FrameworkElement>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 对应的 WPF 路由命令。
        /// 
        /// <remarks>
        /// ClientCommand 的快捷键依赖于 RoutedUICommand 的路由机制。
        /// </remarks>
        /// </summary>
        public UICommand UICommand
        {
            get { return _uiCommand; }
            internal set { _uiCommand = value; }
        }

        /// <summary>
        /// 为这个命令生成的所有集合控件。
        /// </summary>
        public IList<FrameworkElement> UIElements
        {
            get { return _uiElements.AsReadOnly(); }
        }

        /// <summary>
        /// 本命令对应的元数据。
        /// </summary>
        public WPFCommand Meta
        {
            get
            {
                if (this._meta == null)
                {
                    this._meta = UIModel.WPFCommands[this.GetType()].CloneMutable();
                }

                return this._meta;
            }
            internal set
            {
                if (value == null) throw new ArgumentNullException("value");

                this._meta = value;
            }
        }

        /// <summary>
        /// 本命令对应的参数。
        /// </summary>
        public object Parameter { get; internal set; }

        /// <summary>
        /// 命令在界面上显示的标签。
        /// </summary>
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label != value)
                {
                    _label = value;

                    this.NotifyPropertyChanged("Label");
                }
            }
        }

        /// <summary>
        /// 获取或设置这个命令在界面上是否显示。
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (this._isVisible != value)
                {
                    this._isVisible = value;

                    this.NotifyPropertyChanged("IsVisible");
                }
            }
        }

        #endregion

        #region UIElements

        internal IList<FrameworkElement> UIElementsInternal
        {
            get { return _uiElements; }
        }

        /// <summary>
        /// 调用此方法将生成好的控件添加到命令的 UIElements 集合中。
        /// </summary>
        /// <param name="value"></param>
        internal void AddUIElement(FrameworkElement value)
        {
            if (value == null) throw new ArgumentNullException("value");

            _uiElements.Add(value);

            this.OnUIElementGenerated(value);
        }

        /// <summary>
        /// 当 UIElement 属性变更时，调用此方法。
        /// </summary>
        protected virtual void OnUIElementGenerated(FrameworkElement value) { }

        #endregion

        internal void NotifyCreated()
        {
            this.OnCreatedCore();
        }

        /// <summary>
        /// 当本对象被创建时，调用此方法。
        /// </summary>
        /// <param name="cmdArg"></param>
        protected virtual void OnCreatedCore() { }

        /// <summary>
        /// 尝试执行这个命令。
        /// </summary>
        /// <param name="param"></param>
        public bool TryExecute()
        {
            if (this.CanExecute())
            {
                this.Execute();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从指定的输入元素位置开始，路由执行这个命令。
        /// </summary>
        /// <param name="param"></param>
        public bool TryExecute(IInputElement target)
        {
            if (this.UICommand.CanExecute(this.Parameter, target))
            {
                this.UICommand.Execute(this.Parameter, target);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检测这个命令当前是否可以被执行。
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool CanExecute()
        {
            try
            {
                return this.CanExecuteCore();
            }
            catch
            {
                //CanExecute 中的代码不应该抛出异常，否则界面会一直弹出错误信息。
                return false;
            }
        }

        /// <summary>
        /// 执行这个命令
        /// </summary>
        /// <param name="param"></param>
        internal void Execute()
        {
            try
            {
                this.OnExecuting();

                this.ExecuteCore();

                this.OnExecuted(new CommandExecutedArgs(this.Parameter));
            }
            catch (Exception ex)
            {
                var args = new CommandExecuteFailedArgs(ex, this.Parameter);

                this.OnExecuteFailed(args);

                if (!args.Cancel) throw;
            }
        }

        /// <summary>
        /// 子类重写此方法来返回是否可执行的逻辑。
        /// 默认返回 true。
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected virtual bool CanExecuteCore() { return true; }

        /// <summary>
        /// 子类重写此方法来执行具体的逻辑。
        /// </summary>
        /// <param name="param"></param>
        protected abstract void ExecuteCore();

        #region 事件

        /// <summary>
        /// 执行前的事件。
        /// </summary>
        public event EventHandler Executing;

        protected virtual void OnExecuting()
        {
            var handler = this.Executing;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 执行成功后的事件。
        /// </summary>
        public event EventHandler<CommandExecutedArgs> Executed;

        protected virtual void OnExecuted(CommandExecutedArgs e)
        {
            var handler = this.Executed;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// 执行发生异常后的事件。
        /// </summary>
        public event EventHandler<CommandExecuteFailedArgs> ExecuteFailed;

        protected virtual void OnExecuteFailed(CommandExecuteFailedArgs e)
        {
            var handler = this.ExecuteFailed;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region 属性变更

        /// <summary>
        /// 属性变更事件。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override sealed void OnExtendedPropertyChanged(string property)
        {
            base.OnExtendedPropertyChanged(property);

            this.OnPropertyChanged(property);
        }

        #endregion

        /// <summary>
        /// 编程时可使用的名字，
        /// “由于 Name 可能包含一些奇怪的字符，所以不能直接赋值给控件的 Name”。
        /// </summary>
        internal static string GetProgrammingName(ClientCommand command)
        {
            string result = Regex.Replace(command.Meta.Name, @"[^a-zA-Z_]", "_");
            result = result.Insert(0, "cmd_");
            return result;
        }

        void IClientCommand.Execute()
        {
            this.Execute();
        }
    }

    /// <summary>
    /// 泛型的客户端命令类型。
    /// </summary>
    /// <typeparam name="TParamater"></typeparam>
    public abstract class ClientCommand<TParamater> : ClientCommand
        where TParamater : class
    {
        /// <summary>
        /// 本命令对应的参数。
        /// </summary>
        public new TParamater Parameter
        {
            get { return base.Parameter as TParamater; }
        }

        protected override sealed bool CanExecuteCore()
        {
            return this.CanExecute(Parameter);
        }

        protected override sealed void ExecuteCore()
        {
            this.Execute(Parameter);
        }

        protected override sealed void OnCreatedCore()
        {
            base.OnCreatedCore();

            this.OnCreated(Parameter);
        }

        /// <summary>
        /// 当本对象被创建时，调用此方法。
        /// </summary>
        /// <param name="cmdArg">为这个参数创建的本对象</param>
        protected virtual void OnCreated(TParamater param) { }

        /// <summary>
        /// 子类重写此方法来返回是否可执行的逻辑。
        /// 默认返回 true。
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual bool CanExecute(TParamater param) { return true; }

        /// <summary>
        /// 子类重写此方法来执行具体的逻辑。
        /// </summary>
        /// <param name="param"></param>
        public abstract void Execute(TParamater param);
    }
}