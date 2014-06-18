using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RafyUnitTest
{
    [TestClass]
    public class SimpleTest
    {
        [ClassInitialize]
        public static void SimpleTest_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        //[TestMethod]
        //public void EntityTest_TestClone()
        //{
        //    var p = EM<Project>.GetById(new Guid("93369894-5182-4E5A-BC49-91198CE1F092"));

        //    var __watch = new System.Diagnostics.Stopwatch();
        //    __watch.Start();
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        var serializerClone = (p as ICloneable).Clone() as Project;
        //    }
        //    __watch.Stop();
        //    var __usedToBreak = __watch.Elapsed;


        //    __watch.Reset();
        //    __watch.Start();
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        var CSLAPropertyClone = p.MemberwiseClone() as Project;
        //    }
        //    __watch.Stop();
        //    __usedToBreak = __watch.Elapsed;


        //    //__watch.Reset();
        //    //__watch.Start();
        //    //for (int i = 0; i < 1000; i++)
        //    //{
        //    //    var systemClone = p.MemberwiseClone() as Project;
        //    //}
        //    //__watch.Stop();
        //    //__usedToBreak = __watch.Elapsed;
        //}

        //[TestMethod]
        //public void EntityTest_FastGetChild()
        //{
        //    var __watch = new System.Diagnostics.Stopwatch();
        //    __watch.Start();
        //    for (int i = 0; i < 100000; i++)
        //    {
        //        var pbs = Activator.CreateInstance(typeof(PBS), true);
        //    }
        //    __watch.Stop();
        //    var __usedToBreak = __watch.Elapsed;

        //    __watch.Reset();
        //    __watch.Start();
        //    for (int i = 0; i < 100000; i++)
        //    {
        //        var pbs = MethodCaller.CreateInstance(typeof(PBS));
        //    }
        //    __watch.Stop();
        //    __usedToBreak = __watch.Elapsed;
        //}

        //[TestMethod]
        //public void SimpleTest_Type_FullName_Name()
        //{
        //    var t = typeof(GIX4.Library.Project);
        //    var t2 = typeof(Estimate.Library.Project);

        //    var __watch = new System.Diagnostics.Stopwatch();
        //    __watch.Start();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        var n = t.Name;
        //        n = t2.Name;
        //    }
        //    __watch.Stop();
        //    var __usedToBreak = __watch.Elapsed;

        //    __watch.Reset();
        //    __watch.Start();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        var n = t.FullName;
        //        n = t2.FullName;
        //    }
        //    __watch.Stop();
        //    __usedToBreak = __watch.Elapsed;
        //}
    }
}
