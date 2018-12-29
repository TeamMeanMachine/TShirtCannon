using System;
using Microsoft.SPOT;

namespace TShirtCannon.Util
{
    class Listener
    {
        private bool state;

        public Listener(bool initialState)
        {
            state = initialState;
        }

        public bool Get(bool state)
        {
            bool result = state && !this.state;
            this.state = state;
            return result;
        }
    }
}
