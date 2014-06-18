/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140113
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140113 12:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.UnitTest
{
    [Contract]
    public interface ITestAddService : IService
    {
        int A { get; set; }
        int B { get; set; }

        [ServiceOutput]
        int Result { get; set; }
    }

    [Serializable]
    [ContractImpl(typeof(ITestAddService))]
    public class TestAddService : Service, ITestAddService
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Result { get; set; }

        protected override void Execute()
        {
            Result = A + B;
        }
    }
}
