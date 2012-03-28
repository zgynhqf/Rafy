/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101122
 * 说明：实体DLL的初始化器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101122
 * 修改名字为ILibrary 胡庆访 20110421
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 实体DLL的初始化器
    /// </summary>
    public interface ILibrary : IPlugin
    {
        /// <summary>
        /// 两个职责：
        /// 1.依赖注入
        /// 2.注册 app 的一些事件，进行额外的初始化
        /// </summary>
        /// <param name="app"></param>
        void Initialize(IApp app);
    }
}
