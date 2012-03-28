// -- FILE ------------------------------------------------------------------
// name       : Command.cs
// created    : Jani Giannoudis - 2008.04.10
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;
using System.Windows.Input;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public abstract class Command : RoutedUICommand
    {
        //执行时忽略判断IsExecuting
        public virtual bool IngoreIsExecuting
        {
            get { return false; }
        }

        // ----------------------------------------------------------------------
        protected Command(string name, Type ownerType, CommandDescription description) :
            this(description.Text, name, ownerType, description)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            this.description = description;
        } // Command

        // ----------------------------------------------------------------------
        protected Command(string text, string name, Type ownerType, CommandDescription description) :
            base(text, name, ownerType, description.Gestures)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            this.description = description;
        } // Command

        // ----------------------------------------------------------------------
        public CommandDescription Description
        {
            get { return this.description; }
        } // Description

        // ----------------------------------------------------------------------
        public object Target
        {
            get { return this.target; }
            set { this.target = value; }
        } // Target

        // ----------------------------------------------------------------------

        // ----------------------------------------------------------------------
        public virtual bool HasRequirements
        {
            get { return true; }
        } // HasRequirements

        // ----------------------------------------------------------------------
        public virtual bool HasImage
        {
            get { return true; }
        } // HasImage

        // ----------------------------------------------------------------------
        public virtual Uri ImageUri
        {
            get { return null; }
        } // ImageUri

        // ----------------------------------------------------------------------
        public CommandContext CreateCanExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return OnCreateCanExecuteContext(bindingSource, commandSource, commandParameter);
        } // Execute

        // ----------------------------------------------------------------------
        public bool CanExecute(CommandContext commandContext)
        {
            return OnCanExecute(commandContext);
        } // CanExecute

        // ----------------------------------------------------------------------
        public CommandContext CreateExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return OnCreateExecuteContext(bindingSource, commandSource, commandParameter);
        } // CreateExecuteContext

        // ----------------------------------------------------------------------
        public void Execute(CommandContext commandContext)
        {
            OnExecute(commandContext);
        } // Execute

        // ----------------------------------------------------------------------
        protected virtual CommandContext OnCreateCanExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return OnCreateContext(bindingSource, commandSource, commandParameter);
        } // OnCreateCanExecuteContext

        // ----------------------------------------------------------------------
        protected virtual bool OnCanExecute(CommandContext commandContext)
        {
            return false;
        } // OnCanExecute

        // ----------------------------------------------------------------------
        protected virtual CommandContext OnCreateExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return OnCreateContext(bindingSource, commandSource, commandParameter);
        } // OnCreateExecuteContext

        // ----------------------------------------------------------------------
        protected virtual CommandContext OnCreateContext(object bindingSource, object commandSource, object commandParameter)
        {
            return new CommandContext(bindingSource, commandSource, commandParameter);
        } // OnCreateContext

        // ----------------------------------------------------------------------
        protected abstract void OnExecute(CommandContext commandContext);

        // ----------------------------------------------------------------------
        // members
        private readonly CommandDescription description;
        private object target;
    } // class Command

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
