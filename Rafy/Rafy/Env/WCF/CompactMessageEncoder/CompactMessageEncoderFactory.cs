using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;

namespace Rafy.WCF
{
    internal class CompactMessageEncoderFactory : MessageEncoderFactory
    {
        /// <summary>
        /// Stores the compact message encoder
        /// </summary>
        private readonly CompactMessageEncoder _encoder;

        /// <summary>
        /// Stores the message encoder factory of the inner encoder
        /// </summary>
        private readonly MessageEncoderFactory _innerFactory;

        /// <summary>
        /// Construct a compact message encoder factory
        /// </summary>
        /// <param name="innerFactory">The inner encoder factory</param>o
        public CompactMessageEncoderFactory(MessageEncoderFactory innerFactory)
        {
            _innerFactory = innerFactory;
            _encoder = new CompactMessageEncoder(_innerFactory.Encoder);
        }

        /// <summary>
        /// Get the compress message encoder
        /// </summary>
        public override MessageEncoder Encoder
        {
            get { return _encoder; }
        }

        /// <summary>
        /// Get the message version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return _innerFactory.MessageVersion; }
        }
    }
}
