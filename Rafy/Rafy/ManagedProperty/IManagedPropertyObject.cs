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
        /// <param name="source">本次值设置的来源。</param>
        /// <returns>返回最终使用的值。</returns>
        object SetProperty(ManagedProperty<bool> property, bool value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source">本次值设置的来源。</param>
        /// <returns>返回最终使用的值。</returns>
        object SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        /// <summary>
        /// LoadProperty 以最快的方式直接加载值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void LoadProperty(IManagedProperty property, object value);

        /// <summary>
        /// 是否存在主动设置/加载的字段值（本地值）。
        /// </summary>
        /// <param name="property">托管属性</param>
        /// <returns></returns>
        bool FieldExists(IManagedProperty property);

        /// <summary>
        /// 获取编译期属性值集合
        /// </summary>
        /// <returns></returns>
        ManagedPropertyObject.CompiledPropertyValuesEnumerator GetCompiledPropertyValues();

        /// <summary>
        /// 获取当前对象所有非默认值的属性值集合。
        /// </summary>
        /// <returns></returns>
        ManagedPropertyObject.NonDefaultPropertyValuesEnumerator GetNonDefaultPropertyValues();
    }
}
