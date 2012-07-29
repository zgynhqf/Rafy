using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace JXC
{
    [Serializable]
    public abstract class JXCEntity : Entity
    {
        public static string ConnectionString = "JXC";

        protected override string ConnectionStringSettingName
        {
            get { return ConnectionString; }
        }
    }

    [Serializable]
    public abstract class JXCEntityList : EntityList { }

    public class JXCEntityConfig : EntityConfig<JXCEntity>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.UseDetailLayoutMode(DetailLayoutMode.AutoGrid);
        }
    }
}