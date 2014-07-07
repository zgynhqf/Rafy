using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.ComponentModel;
using Rafy.UnitTest;

namespace RafyUnitTest.ClientTest
{
    [TestClass]
    public class EnvironmentTest_Unity : EnvTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ClientTestHelper.ClassInitialize(context);
        }

        #region IOC Container

        [TestMethod]
        public override void EnvTest_IOC_CtorNeedInterface()
        {
            base.EnvTest_IOC_CtorNeedInterface();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface()
        {
            base.EnvTest_IOC_Interface();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface_ErrorWithoutRegister()
        {
            base.EnvTest_IOC_Interface_ErrorWithoutRegister();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface_RegisterByType()
        {
            base.EnvTest_IOC_Interface_RegisterByType();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface_RegisterByType_Dependency()
        {
            base.EnvTest_IOC_Interface_RegisterByType_Dependency();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface_RegisterByType_Dependency_Error()
        {
            base.EnvTest_IOC_Interface_RegisterByType_Dependency_Error();
        }

        [TestMethod]
        public override void EnvTest_IOC_ResolveAll()
        {
            base.EnvTest_IOC_ResolveAll();
        }

        [TestMethod]
        public override void EnvTest_IOC_TypeBySelf()
        {
            base.EnvTest_IOC_TypeBySelf();
        }

        [TestMethod]
        public override void EnvTest_IOC_TypeByType()
        {
            base.EnvTest_IOC_TypeByType();
        }

        [TestMethod]
        public override void EnvTest_IOC_TypeWithoutRegister()
        {
            base.EnvTest_IOC_TypeWithoutRegister();
        }

        [TestMethod]
        public override void EnvTest_IOC_Register_ReplaceByAfter()
        {
            base.EnvTest_IOC_Register_ReplaceByAfter();
        }

        #endregion
    }
}
