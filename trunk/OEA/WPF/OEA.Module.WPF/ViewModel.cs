using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 视图模型的基类。
    /// 
    /// 方便实现INotifyPropertyChanged接口。
    /// </summary>
    public class ViewModel : INotifyPropertyChanged
    {
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
