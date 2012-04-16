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

namespace OEA
{
    /// <summary>
    /// 跨 C/S，B/S 的服务基类
    /// </summary>
    [Serializable]
    public abstract class Service : IService
    {
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
        public IService Invoke()
        {
            return DataPortal.Update(this) as IService;
        }

        /// <summary>
        /// 调用服务并把返回值转换为指定的类型。
        /// 
        /// （out 参数是为了简化接口调用，编译器直接隐式推断。）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="svcReturn"></param>
        public void Invoke<T>(out T svcReturn)
            where T : IService
        {
            svcReturn = (T)this.Invoke();
        }
    }
}