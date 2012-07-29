/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.ComponentModel;
using OEA.ManagedProperty;
using OEA;
using hxy.Common;

namespace OEA
{
    /// <summary>
    /// 跨 C/S，B/S 的服务基类
    /// 
    /// 注意，如果该服务要被使用到 B/S 上，输入和输出参数都应该是基本的数据类型、EntityList 类型。
    /// </summary>
    [Serializable]
    public abstract class Service : IService
    {
        public Service()
        {
            //当在服务端时，默认值为 true，表示直接在服务端运行。
            this._runAtLocal = OEAEnvironment.Location.IsOnServer();
        }

        [NonSerialized]
        private bool _runAtLocal;

        /// <summary>
        /// 当前服务是否需要在本地运行。
        /// 
        /// 当在服务端时，默认值为 true，表示直接在服务端运行。
        /// </summary>
        public bool RunAtLocal
        {
            get { return this._runAtLocal; }
            set { this._runAtLocal = value; }
        }

        /// <summary>
        /// 子类重写此方法实现具体的业务逻辑
        /// </summary>
        internal void ExecuteByDataPortal()
        {
            this.Execute();

            //清除不必要的引用，减少数据传输。
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.HasMarked<ServiceInputAttribute>())
                {
                    if (property.PropertyType.IsClass)
                    {
                        try
                        {
                            property.SetValue(this, null, null);
                        }
                        catch { }
                    }
                }
            }
        }

        protected abstract void Execute();

        /// <summary>
        /// 调用服务并把返回值转换为指定的类型。
        /// </summary>
        /// <returns></returns>
        public void Invoke()
        {
            this.OnInvoking();

            if (this.RunAtLocal)
            {
                //由于在本地，所以没有必须调用 ExecuteByDataPortal 来清除不用的属性。
                this.Execute();
            }
            else
            {
                var res = DataPortal.Update(this) as IService;

                this.ReadOutput(res);
            }

            this.OnInvoked();
        }

        /// <summary>
        /// 使用反射把返回结果的值修改到当前对象上。
        /// </summary>
        /// <param name="res"></param>
        private void ReadOutput(IService res)
        {
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.HasMarked<ServiceOutputAttribute>())
                {
                    var value = property.GetValue(res, null);

                    try
                    {
                        property.SetValue(this, value, null);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 在服务被调用前发生。
        /// </summary>
        protected virtual void OnInvoking() { }

        /// <summary>
        /// 在服务被调用后发生。
        /// </summary>
        protected virtual void OnInvoked() { }

        protected bool IsWeb { get { return OEAEnvironment.IsWeb; } }
        protected bool IsWPF { get { return OEAEnvironment.IsWPF; } }
    }

    /// <summary>
    /// 一种过程化服务的基类
    /// 
    /// 过程化简单地指：进行一系列操作，返回是否成功以及相应的提示消息。
    /// </summary>
    [Serializable]
    public abstract class FlowService : Service
    {
        [ServiceOutput(OutputToWeb = false)]
        public Result Result { get; set; }

        #region Web 服务的参数
        //这两个参数与 ClientResult 类型的属性名保持一致，方便客户端使用。

        [ServiceOutput]
        public bool success { get; set; }
        [ServiceOutput]
        public string msg { get; set; }

        #endregion

        protected override sealed void Execute()
        {
            var res = this.ExecuteCore();

            this.success = res.Success;
            this.msg = res.Message;

            this.Result = res;
        }

        protected abstract Result ExecuteCore();
    }
}