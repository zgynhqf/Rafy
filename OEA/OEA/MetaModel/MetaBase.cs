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
using System.Linq;
using System.Text;
using System.ComponentModel;
using OEA.Reflection;

namespace OEA.MetaModel
{
    /// <summary>
    /// 职责与支持：
    /// SetFieldValue
    /// ICustomParamsHolder
    /// INotifyPropertyChanged（由于有时元数据直接需要被绑定到 WPF 界面中，所以需要实现这个接口）
    /// </summary>
    public abstract class MetaBase : Freezable, ICustomParamsHolder, INotifyPropertyChanged
    {
        #region 扩充的命令参数

        [NonSerialized]
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
                return (T)TypeHelper.CoerceValue(typeof(T), result);
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
            this.CheckUnFrozen();

            this._customParams[paramName] = value;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllCustomParams()
        {
            return this._customParams;
        }

        #endregion

        protected void SetValue<T>(ref T field, T value, string propertyName)
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

        protected override void CloneValues(Freezable target, CloneOption option)
        {
            base.CloneValues(target, option);

            this.CopyParams(target as MetaBase);
        }
    }
}
