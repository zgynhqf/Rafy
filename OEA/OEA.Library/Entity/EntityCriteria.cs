/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：一些实体类通用的条件类。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.Library;
using System.Runtime.Serialization;
using OEA.MetaModel.Attributes;

namespace OEA
{
    [Criteria, Serializable]
    public class GetByIdCriteria : Criteria { }

    [Criteria, Serializable]
    public class GetByParentIdCriteria : Criteria { }

    [Serializable]
    public class GetAllCriteria : Criteria { }

    [Serializable]
    public class GetByTreeParentCodeCriteria : Criteria
    {
        public static readonly Property<string> TreeCodeProperty = P<GetByTreeParentCodeCriteria>.Register(e => e.TreeCode);
        public override string TreeCode
        {
            get { return this.GetProperty(TreeCodeProperty); }
            set { this.SetProperty(TreeCodeProperty, value); }
        }
    }
}