namespace Clide
{
    using System;
    
    public interface IMonitorSelectionService
    {
        event EventHandler<MonitorSelectionEventArgs> DocumentChanged;
        event EventHandler<MonitorSelectionEventArgs> DocumentWindowChanged;
        event EventHandler<MonitorSelectionEventArgs> SelectionChanged;
        event EventHandler<MonitorSelectionEventArgs> UndoManagerChanged;
        event EventHandler<MonitorSelectionEventArgs> WindowChanged;

        object CurrentDocument { get; }
        object CurrentDocumentView { get; }
        object CurrentSelectionContainer { get; }
        object CurrentUndoManager { get; }
        object CurrentWindow { get; }
        object CurrentWindowFrame { get; }
    }
}
