using System;
using Microsoft.SPOT;

namespace TShirtCannon.Util
{
    class Toggler
    {
        private bool state;
        private Listener listener;

        public Toggler(bool initialState)
        {
            state = initialState;
            listener = new Listener(state);
        }

        public bool Get(bool state)
        {
            if (listener.Get(state))
            {
                this.state = !this.state;
            }
            return this.state;
        }
    }
}
