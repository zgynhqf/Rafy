/*******************************************************
 * 
 * 作者：佛山-程序缘
 * 创建日期：20160329
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 佛山-程序缘 20160329 21:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 标记仓库的某个方法为公开的数据查询方法。（该方法必须是虚方法）
    /// <para>框架内部会拦截该方法，实现以下目的：</para>
    /// <para>1.判断是需要在本地、还是服务端来执行此方法。如果需要在服务端执行，则框架会转而调用 WCF 数据门户。（如果需要分布式调用，所有参数需要支持可序列化。）</para>
    /// <para>2.根据方法的返回值，来确定底层查询时应该返回的类型（EntityList、Entity、int、LiteDataTable）。</para>
    /// <para>3.查询完成后，调整查询结果的类型，与需要的类型一致。</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RepositoryQueryAttribute : Attribute { }
}