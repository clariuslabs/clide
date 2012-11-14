namespace Clide
{
    using System;

    public class MonitorSelectionEventArgs : EventArgs
    {
        public MonitorSelectionEventArgs(object oldValue, object newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public object NewValue { get; private set; }
        public object OldValue { get; private set; }
    }
}