using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.ComponentModel;

namespace RafyUnitTest
{
    [TestClass]
    public class EnvironmentTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region IOC Container

        [TestMethod]
        public void EnvTest_IOC_Interface()
        {
            var container = ObjectContainerFactory.CreateContainer();
            container.RegisterType<IAnimal, Dog>();

            var instance = container.Resolve<IAnimal>();
            Assert.IsTrue(instance is Dog);

            var cat = new Cat();
            container.RegisterInstance<IAnimal>(cat);
            instance = container.Resolve<IAnimal>();
            Assert.IsTrue(instance == cat);
        }

        [TestMethod]
        public void EnvTest_IOC_Interface_ErrorWithoutRegister()
        {
            var container = ObjectContainerFactory.CreateContainer();
            bool success = true;
            try
            {
                container.Resolve<IAnimal>();
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void EnvTest_IOC_TypeByType()
        {
            var container = ObjectContainerFactory.CreateContainer();
            container.RegisterType<Dog, BigDog>();

            var instance = container.Resolve<Dog>();
            Assert.IsTrue(instance is BigDog);

            var smallDog = new SmallDog(instance as BigDog);
            container.RegisterInstance<Dog>(smallDog);
            instance = container.Resolve<Dog>();
            Assert.IsTrue(instance == smallDog);
        }

        [TestMethod]
        public void EnvTest_IOC_TypeBySelf()
        {
            var container = ObjectContainerFactory.CreateContainer();
            container.RegisterType<Dog, Dog>();

            var instance = container.Resolve<Dog>();
            Assert.AreEqual(instance.GetType(), typeof(Dog));
        }

        [TestMethod]
        public void EnvTest_IOC_TypeWithoutRegister()
        {
            var container = ObjectContainerFactory.CreateContainer();
            var dog = container.Resolve<Dog>();
            Assert.AreEqual(dog.GetType(), typeof(Dog));
            var bigDog = container.Resolve<BigDog>();
            Assert.AreEqual(bigDog.GetType(), typeof(BigDog));
        }

        [TestMethod]
        public void EnvTest_IOC_CtorNeedInterface()
        {
            var container = ObjectContainerFactory.CreateContainer();
            var smallDog = container.Resolve<SmallDog>();
            Assert.IsNotNull(smallDog.Parent);

            var bigDog = new BigDog();
            container.RegisterInstance(bigDog);
            var smallDog2 = container.Resolve<SmallDog>();
            Assert.AreEqual(bigDog, smallDog2.Parent);
        }

        [TestMethod]
        public void EnvTest_IOC_ResolveAll()
        {
            var container = ObjectContainerFactory.CreateContainer();
            container.RegisterType<IAnimal, Dog>();
            container.RegisterType<IAnimal, BigDog>("BigDog");
            container.RegisterType<IAnimal, SmallDog>("SmallDog");
            container.RegisterInstance<IAnimal>(new Cat());
            container.RegisterInstance<IAnimal>(new Cat(), "another");

            var instances = container.ResolveAll<IAnimal>();
            Assert.AreEqual(instances.Count(), 5);
        }

        #region Classes

        interface IAnimal { }

        class Cat : IAnimal { }

        class Dog : IAnimal { }

        class BigDog : Dog { }

        class SmallDog : Dog
        {
            public BigDog Parent { get; set; }

            public SmallDog(BigDog parent)
            {
                this.Parent = parent;
            }
        }

        #endregion

        #endregion

        #region EventBus

        [TestMethod]
        public void EnvTest_EventBus()
        {
            string name = null;
            Composer.EventBus.Subscribe<EventBusArgs>(e =>
            {
                name = e.Name;
            });

            Composer.EventBus.Publish(new EventBusArgs { Name = "HaHa" });
            Assert.AreEqual(name, "HaHa");
        }

        private class EventBusArgs
        {
            public string Name { get; set; }
        }

        #endregion
    }
}
