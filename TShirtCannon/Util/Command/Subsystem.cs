using System;

namespace TShirtCannon.Util.Command
{
    abstract class Subsystem
    {
        internal Command ActiveCommand = null;
        internal bool Initialized = false;
        internal Command DefaultCommand;

        public Subsystem()
        {
            Scheduler.GetInstance().RegisterSubsystem(this);
        }

        internal virtual Command InitDefaultCommand()
        {
            return null;
        }
    }
}
