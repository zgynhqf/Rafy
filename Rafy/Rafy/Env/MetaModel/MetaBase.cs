/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110317
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100317
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Rafy;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 职责与支持：
    /// SetFieldValue
    /// ICustomParamsHolder
    /// INotifyPropertyChanged（由于有时元数据直接需要被绑定到 WPF 界面中，所以需要实现这个接口）
    /// </summary>
    public abstract class MetaBase : Freezable, INotifyPropertyChanged
    {
        protected void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            this.CheckUnFrozen();

            if (propertyName != null)
            {
                if (!object.Equals(field, value))
                {
                    field = value;

                    this.NotifyPropertyChanged(propertyName);
                }
            }
            else
            {
                field = value;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var hander = this.PropertyChanged;
            if (hander != null) hander(this, e);
        }

        #endregion

        protected override void CloneValues(Freezable target, FreezableCloneOptions option)
        {
            base.CloneValues(target, option);

            this.CopyExtendedProperties(target as MetaBase);
        }
    }
}
