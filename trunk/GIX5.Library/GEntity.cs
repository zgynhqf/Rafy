using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace GIX5.Library
{
    [Serializable]
    public abstract class GEntity : Entity
    {
        public static readonly string ConnectionString = "GIX5";

        protected override string ConnectionStringSettingName
        {
            get { return ConnectionString; }
        }
    }

    [Serializable]
    public abstract class GEntityList : EntityList { }
}