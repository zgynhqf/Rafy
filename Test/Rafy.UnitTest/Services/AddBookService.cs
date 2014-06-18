/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140107
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140107 10:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using UT;

namespace Rafy.UnitTest
{
    [Serializable]
    [Contract, ContractImpl]
    public class AddBookService : Service
    {
        [ServiceOutput]
        public int Result { get; set; }

        protected override void Execute()
        {
            this.Result = 1;
        }
    }

    [Serializable]
    [ContractImpl(typeof(AddBookService), Version = "1.0.0.2")]
    public class AddBookService_V1002 : AddBookService
    {
        protected override void Execute()
        {
            this.Result = 2;
        }
    }

    [Serializable]
    [ContractImpl(typeof(AddBookService), Version = "1.0.0.3")]
    public class AddBookService_V1003 : AddBookService
    {
        protected override void Execute()
        {
            this.Result = 3;
        }
    }
}