using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// Serializer
    /// </summary>
    public interface IStateFormatter
    {
        /// <summary>
        /// Deserialize a string which is storing the state of a object to the original object.
        /// </summary>
        /// <param name="serializedState"></param>
        /// <returns></returns>
        object Deserialize(string serializedState);

        /// <summary>
        /// Serialize a object to a string.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        string Serialize(object state);
    }
}
