/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211125
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211125 12:24
 * 
*******************************************************/

using Rafy.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.UnitTest
{
    public abstract class CommonTestBase
    {
        public static dynamic Assert { get; set; }

        public static void ClearData(IRepository repo)
        {
            var data = repo.GetAll();
            data.Clear();
            repo.Save(data);
        }

        public static IDisposable AutoClearRepoData(IRepository repo)
        {
            return new AutoClearRepoDataClass { repo = repo };
        }

        private class AutoClearRepoDataClass : IDisposable
        {
            public IRepository repo;

            public void Dispose()
            {
                ClearData(repo);
            }
        }
    }

    //internal abstract class Assert
    //{
    //    public static Assert Instance;

    //    public static void IsTrue(bool value)
    //    {
    //        Instance.AssertIsTrue(value);
    //    }

    //    public static void AreEqual(object value1, object value2)
    //    {
    //        Instance.AssertAreEqual(value);
    //    }

    //    protected abstract void AssertIsTrue(bool value);
    //    protected abstract void AssertAreEqual(object value1, object value2);
    //}
}