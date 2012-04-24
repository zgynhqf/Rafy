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
    /// 注意，如果该服务要被使用到 B/S 上，输入和输出参数都应该是基本的数据类型。
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
        internal void ExecuteInternal()
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
            if (this.RunAtLocal)
            {
                this.Execute();
            }
            else
            {
                var res = DataPortal.Update(this) as IService;

                //使用反射把返回结果的值修改到当前对象上。
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
        }
    }

    /// <summary>
    /// 一种过程化服务的基类
    /// 
    /// 过程化简单地指：进行一系列操作，返回是否成功以及相应的提示消息。
    /// </summary>
    [Serializable]
    public abstract class FlowService : Service
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