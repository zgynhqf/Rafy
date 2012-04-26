// -- FILE ------------------------------------------------------------------
// name       : CommandRepository.cs
// created    : Jani Giannoudis - 2008.04.15
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Diagnostics;
using OEA.WPF.Command;

using OEA;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;
using OEA.Module;

namespace Itenso.Windows.Input
{
    // ------------------------------------------------------------------------
    public sealed class CommandRepository
    {
        #region OEA
        //这里只是把代码放在这个类中，与本类中的其它成员没有一点关系……

        public static CommandAdapter NewCommand(Type cmdType)
        {
            var cmdInfo = UIModel.WPFCommands.Find(cmdType);
            return NewCommand(cmdInfo);
        }

        public static CommandAdapter NewCommand(string name)
        {
            var cmdInfo = UIModel.WPFCommands.Find(name);
            return NewCommand(cmdInfo);
        }

        /// <summary>
        /// CreateRuntimeCommand
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <returns></returns>
        public static CommandAdapter NewCommand(WPFCommand commandInfo)
        {
            ClientCommand coreCommand = Activator.CreateInstance(commandInfo.RuntimeType) as ClientCommand;

            coreCommand.CommandInfo = commandInfo;

            var des = new CommandDescription(commandInfo.Label ?? string.Empty, string.Empty, string.Empty, commandInfo.ToolTip, string.Empty);
            var command = new CommandAdapter(coreCommand, typeof(CommandRepository), des);

            OnCommandCreated(coreCommand);

            return command;
        }

        public static event EventHandler<InstanceCreatedEventArgs<ClientCommand>> CommandCreated;

        private static void OnCommandCreated(ClientCommand cmd)
        {
            var handler = CommandCreated;
            if (handler != null)
            {
                handler(null, new InstanceCreatedEventArgs<ClientCommand>(cmd));
            }
        }

        public static CommandBinding CreateCommandBinding(Command cmd)
        {
            return new CommandBinding(
                cmd,
                new ExecutedRoutedEventHandler(CommandBindingExecuted),
                new CanExecuteRoutedEventHandler(CommandBindingCanExecute));
        }

        public static bool TryExecuteCommand(Type cmdId, object cmdArg)
        {
            var cmdInfo = UIModel.WPFCommands.Find(cmdId);
            return TryExecuteCommand(cmdInfo, cmdArg);
        }

        public static bool TryExecuteCommand(WPFCommand cmdInfo, object cmdArg)
        {
            if (cmdInfo == null) throw new ArgumentNullException("cmdInfo");

            var runtimeCmd = CommandRepository.NewCommand(cmdInfo).CoreCommand;

            return runtimeCmd.TryExecute(cmdArg);
        }

        #endregion

        #region NotUsed

        //public static CommandAdapter NewCommand(string name)
        //{
        //    Command cmd = Commands[name];
        //    Debug.Assert(cmd is CommandAdapter, "只有OEA的扩展Command才运行通过Repository新增");
        //    CommandBase coreCommand = Activator.CreateInstance((cmd as CommandAdapter).CoreCommand.GetType()) as CommandBase;
        //    var command = new CommandAdapter(coreCommand, typeof(CommandRepository), cmd.Description);

        //    return command;
        //}

        // ----------------------------------------------------------------------
        private CommandRepository()
        {
        } // CommandRepository

        // 为了不让直接获取Command，而该为通过NewCommand实例化，把此方法该为私有
        private static CommandCollection Commands
        {
            get { return Instance.commands; }
        } // Commands

        public static bool Contains(string name)
        {
            return Commands.Contains(name);
        }

        // ----------------------------------------------------------------------
        private static CommandRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (mutex)
                    {
                        if (instance == null)
                        {
                            instance = new CommandRepository();
                        }
                    }
                }
                return instance;
            }
        } // Instance

        // ----------------------------------------------------------------------
        public static void AddRange(CommandCollection commands)
        {
            Instance.AddCommands(commands);
        } // AddRange

        // ----------------------------------------------------------------------
        public static void Add(Command command)
        {
            Instance.AddCommand(command);
        } // Add

        // ----------------------------------------------------------------------
        private void AddCommands(CommandCollection commands)
        {
            if (commands == null)
            {
                throw new ArgumentNullException("commands");
            }

            foreach (Command command in commands)
            {
                AddCommand(command);
            }
        } // AddCommands

        // ----------------------------------------------------------------------
        private void AddCommand(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            // command registration
            this.commands.Add(command);
        } // AddCommand

        // ----------------------------------------------------------------------
        private static void CommandBindingCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Command command = e.Command as Command;
            if (command == null)
            {
                return;
            }

            // command can executed without CanExecute check
            if (!command.HasRequirements)
            {
                e.CanExecute = true;
                e.Handled = true;
                return;
            }

            // command context
            using (CommandContext commandContext =
                command.CreateCanExecuteContext(sender, e.OriginalSource, e.Parameter))
            {
                if (commandContext == null)
                {
                    return;
                }

                // command can execute check
                e.CanExecute = command.CanExecute(commandContext);
                e.Handled = true;
            }
        } // CommandBindingCanExecute

        // ----------------------------------------------------------------------
        private static void CommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Command command = e.Command as Command;
            if (command == null)
            {
                return;
            }

            // command context
            using (CommandContext commandContext =
                command.CreateCanExecuteContext(sender, e.OriginalSource, e.Parameter))
            {
                // command execute
                command.Execute(commandContext);
                e.Handled = true;
            }
        } // CommandBindingExecuted

        // ----------------------------------------------------------------------
        // members
        private readonly CommandCollection commands = new CommandCollection();
        private readonly CommandBindingCollection bindings = new CommandBindingCollection();

        private static readonly object mutex = new object();
        private static CommandRepository instance;

        #endregion

    } // class CommandRepository

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------