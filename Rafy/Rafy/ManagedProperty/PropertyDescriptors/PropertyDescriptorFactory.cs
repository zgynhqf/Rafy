/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// ManagedPropertyObject 的 PropertyDescriptor 工厂
    /// </summary>
    public class PropertyDescriptorFactory
    {
        /// <summary>
        /// 当前正在被使用的属性描述器工厂
        /// </summary>
        public static PropertyDescriptorFactory Current = new PropertyDescriptorFactory();

        private object _lock = new object();

        internal PropertyDescriptorCollection GetProperties(ConsolidatedTypePropertiesContainer container)
        {
            var result = container.PropertyDescriptors;

            if (container.PropertyDescriptors == null)
            {
                lock (_lock)
                {
                    if (container.PropertyDescriptors == null)
                    {
                        result = CreateDescriptors(container);

                        //缓存到 container 上。
                        container.PropertyDescriptors = result;

                        //当动态属性变化时，PropertyDescriptorCollection 也不可用。
                        container.RuntimePropertiesChanged -= typePropertiesContainer_RuntimePropertiesChanged;
                        container.RuntimePropertiesChanged += typePropertiesContainer_RuntimePropertiesChanged;
                    }
                }
            }

            return result;
        }

        private PropertyDescriptorCollection CreateDescriptors(ConsolidatedTypePropertiesContainer container)
        {
            //加入 ManagedProperty
            var mpList = container.GetAvailableProperties();
            var pdList = mpList.Select(mp => this.CreateDescriptor(mp)).ToList();

            //加入 CLRProperty
            //为了兼容一些直接使用 CLR 属性而没有使用托管属性编写的视图属性。
            var clrProperties = container.OwnerType.GetProperties();
            foreach (var clrProperty in clrProperties)
            {
                if (mpList.All(mp => mp.Name != clrProperty.Name))
                {
                    //索引器被反射为属性，需要过滤掉Item索引器
                    if (clrProperty.GetMethod.GetParameters().Length == 0)
                    {
                        pdList.Add(new CLRPropertyDescriptor(clrProperty));
                    }
                }
            }

            return new PropertyDescriptorCollection(pdList.ToArray());
        }

        private void typePropertiesContainer_RuntimePropertiesChanged(object sender, EventArgs e)
        {
            var container = sender as ConsolidatedTypePropertiesContainer;
            container.PropertyDescriptors = null;
        }

        /// <summary>
        /// 子类重写此方法实现新的扩展属性描述器的生成。
        /// </summary>
        /// <param name="mp"></param>
        /// <returns></returns>
        protected virtual PropertyDescriptor CreateDescriptor(IManagedProperty mp)
        {
            return new ManagedPropertyDescriptor(mp);
        }
    }
}