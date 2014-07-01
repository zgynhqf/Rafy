using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel.View;
using Rafy.MetaModel;
using Demo.WPF;
using Rafy.ComponentModel;

namespace Demo
{
    class DemoExtLibrary : UIPlugin
    {
        protected override int SetupLevel
        {
            get { return ReuseLevel.Customized; }
        }

        public override void Initialize(IApp app)
        {
        }
    }
}