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

namespace OEA.ManagedProperty
{
    /// <summary>
    /// ManagedPropertyObject 的 PropertyDescriptor 容器
    /// </summary>
    internal static class PropertyDescriptorFactory
    {
        private static Dictionary<ConsolidatedTypePropertiesContainer, PropertyDescriptorCollection> _allProperties = new Dictionary<ConsolidatedTypePropertiesContainer, PropertyDescriptorCollection>();

        internal static PropertyDescriptorCollection GetProperties(ManagedPropertyObject managedObj)
        {
            PropertyDescriptorCollection result = null;

            var typePropertiesContainer = managedObj.PropertiesContainer;
            if (!_allProperties.TryGetValue(typePropertiesContainer, out result))
            {
                lock (_allProperties)
                {
                    if (!_allProperties.TryGetValue(typePropertiesContainer, out result))
                    {
                        result = FindProperties(managedObj);

                        //添加到集合中。
                        _allProperties.Add(typePropertiesContainer, result);

                        //当动态属性变化时，PropertyDescriptorCollection 也不可用。
                        typePropertiesContainer.RuntimePropertiesChanged -= typePropertiesContainer_RuntimePropertiesChanged;
                        typePropertiesContainer.RuntimePropertiesChanged += typePropertiesContainer_RuntimePropertiesChanged;
                    }
                }
            }

            return result;
        }

        private static PropertyDescriptorCollection FindProperties(ManagedPropertyObject managedObj)
        {
            //加入 ManagedProperty
            var mpList = managedObj.PropertiesContainer.GetAvailableProperties();
            var pdList = mpList.Select(mp => new ManagedPropertyDescriptor(mp) as PropertyDescriptor).ToList();

            //加入 CLRProperty
            //为了兼容一些直接使用 CLR 属性而没有使用托管属性编写的视图属性。
            var clr = managedObj.GetType().GetProperties();
            foreach (var clrProperty in clr)
            {
                if (mpList.All(mp => mp.Name != clrProperty.Name))
                {
                    pdList.Add(new CLRPropertyDescriptor(clrProperty));
                }
            }

            return new PropertyDescriptorCollection(pdList.ToArray());
        }

        private static void typePropertiesContainer_RuntimePropertiesChanged(object sender, EventArgs e)
        {
            var typePropertiesContainer = sender as ConsolidatedTypePropertiesContainer;
            _allProperties.Remove(typePropertiesContainer);
        }
    }
}