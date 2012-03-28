using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ServiceModel.Channels;

namespace Amib.WCF
{
    /// <summary>
    /// This class enables the user to configure the CompactMesssageEncoder with the configuration file.
    /// It's derived from ConfigurationElement
    /// </summary>
    public class CompactMessageEncodingElement : BindingElementExtensionElement
    {
        /// <summary>
        /// Stores the available properties of this ConfigurationElement.
        /// It's initialized in the Properties property.
        /// </summary>
        protected ConfigurationPropertyCollection _properties;

        /// <summary>
        /// Gets the type of the binding element. 
        /// </summary>
        public override Type BindingElementType
        {
            get
            {
                return typeof(CompactMessageEncodingElement);
            }
        }

        /// <summary>
        /// Creates an instance of the binding element
        /// </summary>
        protected override BindingElement CreateBindingElement()
        {
            CompactMessageEncodingBindingElement bindingElement = new CompactMessageEncodingBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        /// <summary>
        /// Apply the configuration file to the binding element
        /// </summary>
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            CompactMessageEncodingBindingElement element = (CompactMessageEncodingBindingElement)bindingElement;

            // Get the list of available properties for this binding element
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;

            // Make sure message encoder is defined
            if (!this.BinaryMessageEncodingElement.ElementInformation.IsPresent)
            {
                throw new ConfigurationErrorsException("An inner message encoder must be defined: 'binaryMessageEncoding'");
            }

            // If the configuration defines a BinaryMessageEncodingElement then Initialize a binary message encoder
            if (this.BinaryMessageEncodingElement.ElementInformation.IsPresent)
            {
                element.SetInnerBindingElement(new BinaryMessageEncodingBindingElement());
                this.BinaryMessageEncodingElement.ApplyConfiguration(element.InnerBindingElement);
            }
        }

        /// <summary>
        /// Gets the collection of properties. 
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("binaryMessageEncoding", typeof(BinaryMessageEncodingElement), null, null, null, ConfigurationPropertyOptions.None));
                    _properties = properties;
                }
                return _properties;
            }
        }

        /// <summary>
        /// Get the BinaryMessageEncodingElement configuration 
        /// </summary>
        [ConfigurationProperty("binaryMessageEncoding")]
        public BinaryMessageEncodingElement BinaryMessageEncodingElement
        {
            get
            {
                return (BinaryMessageEncodingElement)base["binaryMessageEncoding"];
            }
        }
    }
}