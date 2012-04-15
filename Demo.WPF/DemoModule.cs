using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF;
using OEA;
using OEA.MetaModel;

namespace Demo.WPF
{
    internal class DemoModule : IModule
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IClientApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                CommonModel.Modules["书籍查询"].UseCustomModule<BookQueryModule>();
            };
        }
    }
}