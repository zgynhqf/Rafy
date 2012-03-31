﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla.Properties;

namespace SimpleCsla.Server
{
    /// <summary>
    /// Class containing the default implementation for
    /// the FactoryLoader delegate used by the
    /// Silverlight data portal host.
    /// </summary>
    public class ObjectFactoryLoader : IObjectFactoryLoader
    {
        /// <summary>
        /// Gets the type of the factory.
        /// </summary>
        /// <param name="factoryName">
        /// Type assembly qualified type name for the 
        /// object factory class as
        /// provided from the ObjectFactory attribute
        /// on the business object.
        /// </param>
        public Type GetFactoryType(string factoryName)
        {
            return Type.GetType(factoryName);
        }

        /// <summary>
        /// Creates an instance of an object factory
        /// object for use by the data portal.
        /// </summary>
        /// <param name="factoryName">
        /// Type assembly qualified type name for the 
        /// object factory class as
        /// provided from the ObjectFactory attribute
        /// on the business object.
        /// </param>
        /// <returns>
        /// An instance of the type specified by the
        /// type name parameter.
        /// </returns>
        public object GetFactory(string factoryName)
        {
            var ft = Type.GetType(factoryName);
            if (ft == null)
                throw new InvalidOperationException(
                  string.Format("Factory type or assembly could not be loaded ({0})", factoryName));
            return Activator.CreateInstance(ft);
        }
    }
}