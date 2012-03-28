using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OEA.Utils;
using Microsoft.Practices.Unity;

namespace OEA.Library.DogSensor
{
    [Serializable]
    public class DogSensorCommand : Csla.CommandBase
    {
        private bool _opend;

        public bool Opend
        {
            get
            {
                return this._opend;
            }
            set
            {
                this._opend = value;
            }
        }

        protected override void DataPortal_Execute()
        {
            var sensor = OEAEnvironment.UnityContainer.Resolve<IDogSensor>();
            Debug.Assert(sensor != null, "还没有注册这个对象。");

            this._opend = sensor.IsDogOpend();
        }
    }
}