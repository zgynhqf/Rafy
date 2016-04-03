using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;

namespace Rafy.WCF
{
    internal class CompactMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        /// <summary>
        /// Stores the inner binding element
        /// </summary>
        private MessageEncodingBindingElement _innerBindingElement;

        /// <summary>
        /// Returns the inner binding element
        /// </summary>
        internal MessageEncodingBindingElement InnerBindingElement
        {
            get { return _innerBindingElement; }
            set { _innerBindingElement = value; }
        }

        /// <summary>
        /// Get/Set the message version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return this._innerBindingElement.MessageVersion; }
            set { this._innerBindingElement.MessageVersion = value; }
        }

        /// <summary>
        /// Builds the channel factory stack on the client that creates a specified type of channel for a specified context. 
        /// </summary>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            // Add the CompactMessageEncodingBindingElement to the channel so it can use it
            context.BindingParameters.Add(this);
            return base.BuildChannelFactory<TChannel>(context);
        }

        /// <summary>
        /// Builds the channel listener on the service that accepts a specified type of channel for a specified context.
        /// </summary>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return base.BuildChannelListener<TChannel>(context);
        }

        /// <summary>
        /// Create the CompactMessageEncoderFactory
        /// </summary>
        /// <returns></returns>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new CompactMessageEncoderFactory(_innerBindingElement.CreateMessageEncoderFactory());
        }

        /// <summary>
        /// Returns a typed object requested, if present, from the appropriate layer in the binding element stack. 
        /// </summary>
        public override T GetProperty<T>(BindingContext context)
        {
            T result = _innerBindingElement.GetProperty<T>(context) ?? context.GetInnerProperty<T>();
            return result;
        }

        #region Clone

        /// <summary>
        /// Stores the base binding element so clones can use it.
        /// </summary>
        private CompactMessageEncodingBindingElement _baseBindingElement;

        /// <summary>
        /// Clone the CompactMessageEncodingBindingElement
        /// </summary>
        /// <returns></returns>
        public override BindingElement Clone()
        {
            return new CompactMessageEncodingBindingElement(this);
        }

        /// <summary>
        /// Constructor, used with the Clone method
        /// </summary>
        /// <param name="originalBindingElement"></param>
        private CompactMessageEncodingBindingElement(CompactMessageEncodingBindingElement originalBindingElement)
        {
            _innerBindingElement = originalBindingElement._innerBindingElement;

            // The purpose of this code is to avoid the nesting of the same encoder within itself
            if (originalBindingElement._baseBindingElement == null)
            {
                _baseBindingElement = originalBindingElement;
            }
            else
            {
                _baseBindingElement = originalBindingElement._baseBindingElement;
            }
        }

        internal CompactMessageEncodingBindingElement() { }

        #endregion
    }
}
