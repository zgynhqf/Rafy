using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Configuration;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.MetaModel;
using Rafy.UnitTest;
using Rafy.UnitTest.DataProvider;
using Rafy.UnitTest.RuntimeLoad;

namespace RafyUnitTest
{
    [TestClass]
    public class EnvironmentTest : EnvTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region RuntimePlugins

        [TestMethod]
        public void EnvTest_Plugin_LazyLoad()
        {
            if (TestDbGenerator.ForceAllPluginsLoaded) return;

            var plugin = RafyEnvironment.AllPlugins.Find(typeof(DiskCachingPlugin));
            Assert.IsNull(plugin, "该插件是按需加载，此时应该还没有加载");

            var em = CommonModel.Entities.FirstOrDefault(e => e.EntityType == typeof(ScopeVersion));
            Assert.IsNull(em, "该插件是按需加载，此时应该还没有加载");
            Assert.IsNull(VersionSyncMgr.Repository, "该插件是按需加载，此时应该还没有加载");

            RafyEnvironment.LoadPlugin(typeof(DiskCachingPlugin).Assembly);

            plugin = RafyEnvironment.AllPlugins.Find(typeof(DiskCachingPlugin));
            Assert.IsNotNull(plugin, "可以通过实体类，按需加载其对应的插件");

            em = CommonModel.Entities.Find(typeof(ScopeVersion));
            Assert.IsNotNull(em, "该插件是按需加载，此时应该加载");
            Assert.IsNotNull(VersionSyncMgr.Repository, "按需加载的插件的 Initialize 方法也需要被调用。");

            //本单元测试只用于演示。其实 DiskCachingPlugin 在使用时，必须启动时加载。
        }

        [TestMethod]
        public void EnvTest_Plugin_LazyLoad2()
        {
            if (TestDbGenerator.ForceAllPluginsLoaded) return;

            var plugin = RafyEnvironment.AllPlugins.Find(typeof(RuntimeLoadPlugin));
            Assert.IsNull(plugin, "该插件是按需加载，此时应该还没有加载");

            var configurations = RafyEnvironment.FindConfigurations(typeof(Invoice));
            Assert.IsFalse(configurations.Any(c => c is InvoiceConfig), "该插件的实体配置，是没有加载的。");
            Assert.IsFalse(InvoiceConfig.RunnedAnyTime, "该插件的实体配置，是没有加载的，所以也没有运行过。");

            RafyEnvironment.LoadPlugin(typeof(Invoice).Assembly);

            plugin = RafyEnvironment.AllPlugins.Find(typeof(RuntimeLoadPlugin));
            Assert.IsNotNull(plugin, "可以通过实体类，按需加载其对应的插件");

            var em = CommonModel.Entities.Find(typeof(Invoice));
            Assert.IsTrue(InvoiceConfig.RunnedAnyTime, "该插件的实体配置，已经可以被加载并运行。");
            configurations = RafyEnvironment.FindConfigurations(typeof(Invoice));
            Assert.IsTrue(configurations.Any(c => c is InvoiceConfig), "该插件的实体配置被加载。");
        }

        #endregion

        #region IOC Container

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
        public override void EnvTest_IOC_Interface()
        {
            base.EnvTest_IOC_Interface();
        }

        [TestMethod]
        public override void EnvTest_IOC_CtorNeedInterface()
        {
            base.EnvTest_IOC_CtorNeedInterface();
        }

        [TestMethod]
        public override void EnvTest_IOC_Interface_ErrorWithoutRegister()
        {
            base.EnvTest_IOC_Interface_ErrorWithoutRegister();
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

        #region EventBus

        [TestMethod]
        public void EnvTest_EventBus()
        {
            string name = null;
            Composer.EventBus.Subscribe<EventBusArgs>(this, e =>
            {
                name = e.Name;
            });

            Composer.EventBus.Publish(new EventBusArgs { Name = "HaHa" });
            Assert.AreEqual(name, "HaHa");
        }

        [TestMethod]
        public void EnvTest_EventBus_EventSubscribers()
        {
            string name = null;
            Composer.EventBus.Subscribe<EventBusArgs>(this, e =>
            {
                name = e.Name;
            });

            var subscribers = Composer.EventBus.GetSubscribers<EventBusArgs>();
            if (subscribers.Count > 0)
            {
                subscribers.Publish(new EventBusArgs { Name = "HaHa" });
                Assert.AreEqual(name, "HaHa");
            }
        }

        private class EventBusArgs
        {
            public string Name { get; set; }
        }

        #endregion
    }
}
