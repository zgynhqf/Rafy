using System;
using System.Runtime.Serialization;

namespace SimpleCsla.Server.Hosts.WcfChannel
{
    /// <summary>
    /// Request message for updating
    /// a business object.
    /// </summary>
    [DataContract]
    public class UpdateRequest
    {
        [DataMember]
        private object _object;
        [DataMember]
        private SimpleCsla.Server.DataPortalContext _context;

        /// <summary>
        /// Create new instance of object.
        /// </summary>
        /// <param name="obj">Business object to update.</param>
        /// <param name="context">Data portal context from client.</param>
        public UpdateRequest(object obj, SimpleCsla.Server.DataPortalContext context)
        {
            this.Object = obj;
            _context = context;
        }

        /// <summary>
        /// Business object to be updated.
        /// </summary>
        public object Object
        {
            get
            {
                return InnerSerializer.DeserializeObject(_object);
            }
            set
            {
                _object = InnerSerializer.SerializeObject(value);
            }
        }

        /// <summary>
        /// Data portal context from client.
        /// </summary>
        public SimpleCsla.Server.DataPortalContext Context
        {
            get { return _context; }
            set { _context = value; }
        }
    }
}