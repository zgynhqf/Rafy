/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 系统的功能权限 id 列表
    /// 
    /// 此些 Id 会保存到数据库中，不能更改值。
    /// </summary>
    public class SystemOperationKeys
    {
        /// <summary>
        /// 是否可查看某对象的功能的权限标记。
        /// 模块根对象对应为打开模块功能，子对象对应为显示子对象功能。
        /// </summary>
        public static readonly string Read = "系统权限 - 查看";

        ///// <summary>
        ///// 是否可编辑某对象的功能的权限标记。
        ///// </summary>
        //public static readonly string Edit = "系统权限 - 编辑";
    }
}