/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161012
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161012 11:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性对象接口
    /// </summary>
    public interface IManagedPropertyObject : INotifyPropertyChanged, ICustomTypeDescriptor
    {
        /// <summary>
        /// 重设属性为默认值
        /// </summary>
        /// <param name="property"></param>
        void ResetProperty(IManagedProperty property);

        /// <summary>
        /// 获取某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        object GetProperty(IManagedProperty property);

        /// <summary>
        /// 获取某个托管属性的值。
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        TPropertyType GetProperty<TPropertyType>(ManagedProperty<TPropertyType> property);

        /// <summary>
        /// 设置某个 bool 类型托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>返回最终使用的值。</returns>
        object SetProperty(ManagedProperty<bool> property, bool value);

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>返回最终使用的值。</returns>
        object SetProperty(IManagedProperty property, object value);

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="resetDisabledStatus">如果本字段处于禁用状态，那么是否在设置新值时，将禁用状态解除？</param>
        /// <returns>返回最终使用的值。</returns>
        object SetProperty(IManagedProperty property, object value, bool resetDisabledStatus);

        /// <summary>
        /// LoadProperty 以最快的方式直接加载值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void LoadProperty(IManagedProperty property, object value);
    }
}
