using System;
using System.Collections;

namespace TShirtCannon.Util.Command
{
    abstract class Command
    {
        protected internal bool IsCancellable = true;

        internal readonly HashSet Requirements = new HashSet();

        public virtual void Initialize()
        {
        }

        public abstract bool Run();

        public virtual void End()
        {
        }

        public void Start()
        {
            Scheduler.GetInstance().AddCommand(this);
        }

        protected void Requires(Subsystem subsystem)
        {
            Requirements.Add(subsystem);
        }
    }
}
