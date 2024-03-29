﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 所有查询实体对象都应该标记这个属性。
    /// 
    /// 如果是查询实体，那么这个类是没有对应的仓库的。但是查询实体会有自己的实体元数据、界面元数据等元数据。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryEntityAttribute : EntityAttribute { }
}