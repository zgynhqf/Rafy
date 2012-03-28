using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Command;
using Itenso.Windows.Input;
using System.Windows;


namespace OEA.WPF.Command
{
    public class CommandAdapter : Itenso.Windows.Input.Command
    {
        private readonly ClientCommand _coreCommand;

        public CommandAdapter(ClientCommand coreCommand, Type ownerType, CommandDescription description) :
            base(coreCommand.Id, ownerType, description)
        {
            if (coreCommand == null)
            {
                throw new ArgumentNullException("coreCommand");
            }

            this._coreCommand = coreCommand;
        }

        public ClientCommand CoreCommand
        {
            get { return this._coreCommand; }
        }

        public override bool IngoreIsExecuting
        {
            get { return true; }
            //get { return _coreCommand.CommandInfo.IngoreIsExecuting; }
        }

        protected override bool OnCanExecute(CommandContext commandContext)
        {
            return this._coreCommand.CanExecute(commandContext.CommandParameter);
        }

        protected override void OnExecute(CommandContext commandContext)
        {
            this._coreCommand.Execute(commandContext.CommandParameter);
        }
    }
}
