using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 用于类似新增、修改弹出按钮的调用
    /// 该类按钮在弹出前先创建一临时实体，并对当前实体执行直接读取数据的Clone操作，操作后再将临时实体的数据Clone到当前实体上
    /// </summary>
    public interface IAddOrEditCommandCallback
    {
        /// <summary>
        /// 确定修改后，在执行Clone操作之前调用
        /// </summary>
        /// <param name="source"></param>
        void BeforeClone(Entity source);

        /// <summary>
        /// 确定修改后，在执行Clone操作之前调用
        /// </summary>
        /// <param name="source"></param>
        void AfterClone(Entity source);
    }
}
