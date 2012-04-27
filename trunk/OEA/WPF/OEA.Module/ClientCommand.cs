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
using System.ComponentModel;
using System.Text.RegularExpressions;
using OEA.MetaModel;
using OEA.MetaModel.View;
using Common;
using System.Data.SqlClient;

namespace OEA.Module
{
    /// <summary>
    /// 所有命令的基类
    /// </summary>
    public abstract class ClientCommand : IClientCommand, ICustomParamsHolder, INotifyPropertyChanged
    {
        #region 私有字段

        private WPFCommand _commandInfo;

        private string _label;

        private bool _isVisible = true;

        #endregion

        #region 公共属性

        public WPFCommand CommandInfo
        {
            get
            {
                if (this._commandInfo == null)
                {
                    this._commandInfo = UIModel.WPFCommands[this.GetType()].CloneMutable();
                }

                return this._commandInfo;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value.RuntimeType != this.GetType()) throw new InvalidOperationException("value.RuntimeType != this.GetType() must be false.");

                this._commandInfo = value;

                this.SyncCustomParamsFromMetaModel();
            }
        }

        public string Id
        {
            get { return this.CommandInfo.Name; }
        }

        public string Label
        {
            get
            {
                if (this._label == null)
                {
                    this._label = this.CommandInfo.Label;
                }

                return this._label;
            }
            set
            {
                if (this._label != value)
                {
                    this._label = value;

                    this.NotifyPropertyChanged("Label");
                }
            }
        }

        /// <summary>
        /// 编程时可使用的名字，
        /// “由于Name可能包含一些奇怪的字符，所以不能直接赋值给控件的Name”
        /// </summary>
        public string ProgramingName
        {
            get
            {
                string result = Regex.Replace(this.Id, @"[^\w_]", "_");
                if (Regex.Match(result, @"^\d").Success)
                {
                    result = result.Insert(0, "name_");
                }
                return result;
            }
        }

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

        /// <summary>
        /// 是否需要触发命令失败的事件。
        /// 默认为 false
        /// </summary>
        protected bool CommandFailedEventEnabled { get; set; }

        #endregion

        #region 扩充的命令参数

        private Dictionary<string, object> _customParams = new Dictionary<string, object>();

        /// <summary>
        /// 获取指定参数的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public T TryGetCustomParams<T>(string paramName)
        {
            object result;

            if (_customParams.TryGetValue(paramName, out result))
            {
                return (T)result;
            }

            return default(T);
        }

        /// <summary>
        /// 设置自定义参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        public void SetCustomParams(string paramName, object value)
        {
            this._customParams[paramName] = value;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllCustomParams()
        {
            return this._customParams;
        }

        private void SyncCustomParamsFromMetaModel()
        {
            this.CopyParams(this.CommandInfo);
        }

        #endregion

        /// <summary>
        /// 尝试执行这个命令
        /// </summary>
        /// <param name="param"></param>
        public bool TryExecute(object cmdArg)
        {
            if (this.CanExecute(cmdArg))
            {
                this.Execute(cmdArg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否这个命令所对应的按钮可以被执行
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool CanExecute(object param)
        {
            try
            {
                return this.CanExecuteCore(param);
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
        public void Execute(object param)
        {
            //调试阶段如果接住了异常，会比较难以调试，所以调试期关闭异常事件。。
            if (!OEAEnvironment.IsDebuggingEnabled)
            {
                try
                {
                    this.OnExecuting();

                    this.ExecuteCore(param);

                    this.OnExecuted(new CommandExecutedArgs(param));
                }
                catch (Exception ex)
                {
                    var args = new CommandExecuteFailedArgs(ex, param);

                    this.OnExecuteFailed(args);

                    if (!args.Cancel) throw ex;
                }
            }
            else
            {
                this.OnExecuting();

                this.ExecuteCore(param);

                this.OnExecuted(new CommandExecutedArgs(param));
            }
        }

        protected virtual bool CanExecuteCore(object param) { return true; }

        protected abstract void ExecuteCore(object param);

        #region 事件

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

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}