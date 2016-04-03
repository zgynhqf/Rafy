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
using System.Collections.Generic;
using System.ComponentModel;
using Rafy;
using Rafy.Reflection;
using Rafy.Domain.DataPortal;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 跨 C/S，B/S 的服务基类
    /// 
    /// 注意，如果该服务要被使用到 B/S 上，输入和输出参数都应该是基本的数据类型、EntityList 类型。
    /// </summary>
    [Serializable]
    public abstract class Service : IService
    {
        [NonSerialized]
        private DataPortalLocation _dataPortalLocation;

        public Service()
        {
            this._dataPortalLocation = RafyEnvironment.IsOnServer() ? DataPortalLocation.Local : DataPortalLocation.Dynamic;
        }

        /// <summary>
        /// 当前服务是否需要在本地运行。（有时需要设置此值来强制服务在客户端运行。）
        /// 
        /// 当在服务端时，默认值为 Local，表示直接在服务端运行。
        /// </summary>
        public DataPortalLocation DataPortalLocation
        {
            get { return this._dataPortalLocation; }
            set { this._dataPortalLocation = value; }
        }

        /// <summary>
        /// 数据门户会调用此方法来实现执行逻辑。
        /// </summary>
        internal void ExecuteByDataPortal()
        {
            this.Execute();

            //清除不必要的引用，减少数据传输。
            if (this.DataPortalLocation == DataPortalLocation.Dynamic)
            {
                foreach (var contractType in this.GetContractType())
                {
                    var properties = contractType.GetProperties();
                    foreach (var property in properties)
                    {
                        if (!ServiceHelper.IsOutput(property))
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
            }
        }

        /// <summary>
        /// 子类重写此方法实现具体的业务逻辑
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// 调用服务并把返回值转换为指定的类型。
        /// </summary>
        /// <returns></returns>
        public void Invoke()
        {
            this.OnInvoking();

            if (this.DataPortalLocation == DataPortalLocation.Local)
            {
                //由于在本地，所以没有必须调用 ExecuteByDataPortal 来清除不用的属性。
                this.Execute();
            }
            else
            {
                var res = DataPortalApi.Update(this) as IService;

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
            foreach (var contractType in this.GetContractType())
            {
                var properties = contractType.GetProperties();
                foreach (var property in properties)
                {
                    if (ServiceHelper.IsOutput(property))
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

        /// <summary>
        /// 在服务被调用前发生。
        /// </summary>
        protected virtual void OnInvoking() { }

        /// <summary>
        /// 在服务被调用后发生。
        /// </summary>
        protected virtual void OnInvoked() { }

        private IEnumerable<Type> GetContractType()
        {
            var serviceType = this.GetType();

            var list = serviceType.GetCustomAttributes(typeof(ContractImplAttribute), false);
            foreach (ContractImplAttribute attri in list)
            {
                yield return attri.ContractType ?? serviceType;
            }
        }
    }
}