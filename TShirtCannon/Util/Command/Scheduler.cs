using System;
using System.Collections;

namespace TShirtCannon.Util.Command
{
    class Scheduler
    {
        private static Scheduler instance;
        private readonly Stack newCommands = new Stack();
        private readonly Stack completedCommands = new Stack();
        private readonly HashSet activeCommands = new HashSet();
        private readonly HashSet subsystems = new HashSet();

        private Scheduler() {}

        public static Scheduler GetInstance()
        {
            if (instance == null)
            {
                instance = new Scheduler();
            }
            return instance;
        }

        
        public void Run()
        {
            while (newCommands.Count != 0)
            {
                var command = (Command)newCommands.Pop();
                InitCommand(command);
            }
            
            foreach (Command command in activeCommands)
            {
                if (command.Run()) completedCommands.Push(command);
            }

            while (completedCommands.Count != 0)
            {
                var command = (Command)completedCommands.Pop();
                EndCommand(command);
            }

            foreach (Subsystem subsystem in subsystems)
            {
                if (!subsystem.Initialized)
                {
                    subsystem.DefaultCommand = subsystem.InitDefaultCommand();
                    subsystem.Initialized = true;
                }

                if (subsystem.ActiveCommand == null && subsystem.DefaultCommand != null)
                {
                    InitCommand(subsystem.DefaultCommand);
                }
            }
        }

        private void InitCommand(Command command)
        {
            foreach (Subsystem requirement in command.Requirements)
            {
                var conflictCommand = requirement.ActiveCommand;

                if (conflictCommand == null) continue;

                if (!conflictCommand.IsCancellable) return;

                EndCommand(conflictCommand);
            }

            activeCommands.Add(command);
            foreach (Subsystem requirement in command.Requirements) requirement.ActiveCommand = command;

            command.Initialize();
        }

        private void EndCommand(Command command)
        {
            command.End();
            activeCommands.Remove(command);
            foreach (Subsystem requirement in command.Requirements)
            {
                requirement.ActiveCommand = null;
            }
        }

        internal void AddCommand(Command command)
        {
            newCommands.Push(command);
        }

        internal void RegisterSubsystem(Subsystem subsystem)
        {
            subsystems.Add(subsystem);
        }

    }
}
