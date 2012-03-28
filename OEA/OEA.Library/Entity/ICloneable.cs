using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    public interface ICloneable<T>
    {
        /// <summary>
        /// 从目的数据源中拷贝所有数据。
        /// </summary>
        /// <param name="target"></param>
        void Clone(T target);
    }
}
