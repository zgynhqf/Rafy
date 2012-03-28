// -- FILE ------------------------------------------------------------------
// name       : CommandContext.cs
// created    : Jani Giannoudis - 2008.04.15
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;
using System.Windows.Input;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public class CommandContext : IDisposable
    {

        // ----------------------------------------------------------------------
        public CommandContext()
        {
        } // CommandContext

        // ----------------------------------------------------------------------
        public CommandContext(object bindingSource, object commandSource, object commandParameter)
        {
            this.commandSource = commandSource;
            this.bindingSource = bindingSource;
            this.commandParameter = commandParameter;
        } // CommandContext

        // ----------------------------------------------------------------------
        ~CommandContext()
        {
            Dispose(false);
        } // ~CommandContext

        // ----------------------------------------------------------------------
        public object BindingSource
        {
            get { return this.bindingSource; }
            set { this.bindingSource = value; }
        } // BindingSource

        // ----------------------------------------------------------------------
        public object CommandSource
        {
            get { return this.commandSource; }
            set { this.commandSource = value; }
        } // CommandSource

        // ----------------------------------------------------------------------
        public object CommandParameter
        {
            get { return this.commandParameter; }
            set { this.commandParameter = value; }
        } // CommandParameter

        // ----------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
        } // Dispose

        // ----------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
            }
        } // Dispose

        // ----------------------------------------------------------------------
        // members
        private object commandSource;
        private object bindingSource;
        private object commandParameter;
        private bool disposed = false;

    } // class CommandContext

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
