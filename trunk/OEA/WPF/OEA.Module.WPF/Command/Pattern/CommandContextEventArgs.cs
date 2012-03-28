// -- FILE ------------------------------------------------------------------
// name       : CommandContextEventArgs.cs
// created    : Jani Giannoudis - 2008.04.15
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public delegate void CommandContextEventHandler(object sender, CommandContextEventArgs e);

    // ------------------------------------------------------------------------
    public class CommandContextEventArgs : EventArgs
    {

        // ----------------------------------------------------------------------
        public CommandContextEventArgs(Command command, CommandContext commandContext)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandContext == null)
            {
                throw new ArgumentNullException("commandContext");
            }

            this.command = command;
            this.commandContext = commandContext;
        } // CommandContextEventArgs

        // ----------------------------------------------------------------------
        public Command Command
        {
            get { return this.command; }
        } // Command

        // ----------------------------------------------------------------------
        public CommandContext CommandContext
        {
            get { return this.commandContext; }
        } // CommandContext

        // ----------------------------------------------------------------------
        // members
        private readonly Command command;
        private readonly CommandContext commandContext;

    } // class CommandContextEventArgs

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
