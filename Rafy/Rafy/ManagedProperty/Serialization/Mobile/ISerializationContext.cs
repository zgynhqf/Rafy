using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Serialization.Mobile
{
    public interface ISerializationContext
    {
        #region Properties

        bool IsProcessingState { get; }

        //SerializationInfoContainer Container { get; }

        MobileFormatter RefFormatter { get; }

        Dictionary<string, int> References { get; }

        Dictionary<string, object> States { get; }

        #endregion

        #region State

        void AddState(string name, object state);

        T GetState<T>(string name);

        object GetState(string name, Type targetType);

        bool IsState(Type stateType);

        #endregion

        #region Delegate

        void AddDelegate(string name, Delegate action);

        TDelegate GetDelegate<TDelegate>(string name) where TDelegate : class;

        #endregion

        #region Ref

        void AddRef(string name, object reference);

        IMobileObject GetRef(string name);

        T GetRef<T>(string name) where T : class;

        #endregion
    }
}
