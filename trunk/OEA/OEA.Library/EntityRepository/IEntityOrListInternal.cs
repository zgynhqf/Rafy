/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 添加注释 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    internal interface IEntityOrListInternal
    {
        void NotifySaved();
    }

    /// <summary>
    /// 外界不要使用，OEA 框架自身使用。
    /// </summary>
    public interface IOEARepositoryInternal
    {
        EntityList GetListImplicitly(object criteria);
    }
}