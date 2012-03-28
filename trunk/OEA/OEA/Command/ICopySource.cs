using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Command
{
    /// <summary>
    /// 复制添加命令的“观察者”。
    /// 
    /// 此接口用于约定。
    /// 所有实现了这个接口的对象，在被调用复制添加命令时，都会相应的调用它的BeforeCopy和EndCopy方法。
    /// </summary>
    public interface ICopySource
    {
        /// <summary>
        /// 通知本复制源，马上要对本对象进行复制操作了。
        /// </summary>
        void NotifyCopying();
    }
}
