using Microsoft.VisualStudio.OLE.Interop;
namespace Clide
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Ole = Microsoft.VisualStudio.OLE.Interop;

    [Export(typeof(IMonitorSelectionService))]
    internal class MonitorSelectionService : IMonitorSelectionService, IDisposable, IVsSelectionEvents
    {
        private IVsWindowFrame currentDocumentFrame;
        private ISelectionContainer currentSelectionContainer;
        private Ole.IOleUndoManager currentUndoManager;
        private IVsWindowFrame currentWindowFrame;
        private IVsMonitorSelection monitorSelection;
        private uint selectionCookie;
        private IServiceProvider serviceProvider;

        public event EventHandler<MonitorSelectionEventArgs> DocumentChanged;
        public event EventHandler<MonitorSelectionEventArgs> DocumentWindowChanged;
        public event EventHandler<MonitorSelectionEventArgs> SelectionChanged;
        public event EventHandler<MonitorSelectionEventArgs> UndoManagerChanged;
        public event EventHandler<MonitorSelectionEventArgs> WindowChanged;

        [ImportingConstructor]
        public MonitorSelectionService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            this.monitorSelection = serviceProvider.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
            if (this.monitorSelection != null)
            {
                object obj2;
                ErrorHandler.ThrowOnFailure(this.monitorSelection.GetCurrentElementValue(2, out obj2));
                this.currentDocumentFrame = obj2 as IVsWindowFrame;
                ErrorHandler.ThrowOnFailure(this.monitorSelection.GetCurrentElementValue(1, out obj2));
                this.currentWindowFrame = obj2 as IVsWindowFrame;
                ErrorHandler.ThrowOnFailure(this.monitorSelection.GetCurrentElementValue(0, out obj2));
                this.currentUndoManager = obj2 as Ole.IOleUndoManager;
                ErrorHandler.ThrowOnFailure(this.monitorSelection.AdviseSelectionEvents(this, out this.selectionCookie));
                this.currentSelectionContainer = this.CurrentDocumentView as ISelectionContainer;
                if (this.currentSelectionContainer == null)
                {
                    this.currentSelectionContainer = this.CurrentWindow as ISelectionContainer;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((this.serviceProvider != null) && (this.selectionCookie != 0))
                {
                    IVsMonitorSelection service = this.serviceProvider.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
                    if (service != null)
                    {
                        ErrorHandler.ThrowOnFailure(service.UnadviseSelectionEvents(this.selectionCookie));
                        this.selectionCookie = 0;
                    }
                }
                this.monitorSelection = null;
                this.serviceProvider = null;
            }
        }

        ~MonitorSelectionService()
        {
            this.Dispose(false);
        }

        int IVsSelectionEvents.OnCmdUIContextChanged(uint cookie, int fActive)
        {
            return 0;
        }

        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object oldValue, object newValue)
        {
            switch (elementid)
            {
                case 0:
                    this.currentUndoManager = newValue as Ole.IOleUndoManager;
                    if (this.UndoManagerChanged != null)
                    {
                        this.UndoManagerChanged(this, new MonitorSelectionEventArgs(oldValue, newValue));
                    }
                    break;

                case 1:
                    this.currentWindowFrame = newValue as IVsWindowFrame;
                    if (this.WindowChanged != null)
                    {
                        this.WindowChanged(this, new MonitorSelectionEventArgs(oldValue, newValue));
                    }
                    break;

                case 2:
                    this.currentDocumentFrame = newValue as IVsWindowFrame;
                    if (this.DocumentWindowChanged != null)
                    {
                        this.DocumentWindowChanged(this, new MonitorSelectionEventArgs(oldValue, newValue));
                    }
                    if (this.DocumentChanged != null)
                    {
                        IVsWindowFrame frame = oldValue as IVsWindowFrame;
                        object pvar = null;
                        int hr = -2147467259;
                        if (frame != null)
                        {
                            hr = frame.GetProperty(-4004, out pvar);
                        }
                        if (ErrorHandler.Succeeded(hr) && (this.currentDocumentFrame != null))
                        {
                            object obj3 = null;
                            if (ErrorHandler.Succeeded(this.currentDocumentFrame.GetProperty(-4004, out obj3)) && (pvar != obj3))
                            {
                                this.DocumentChanged(this, new MonitorSelectionEventArgs(oldValue, newValue));
                            }
                        }
                    }
                    break;
            }
            return 0;
        }

        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            this.currentSelectionContainer = pSCNew;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, new MonitorSelectionEventArgs(pSCOld, pSCNew));
            }
            return 0;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Properties
        public object CurrentDocument
        {
            get
            {
                object pvar = null;
                if (this.currentDocumentFrame != null)
                {
                    ErrorHandler.ThrowOnFailure(this.currentDocumentFrame.GetProperty(-4004, out pvar));
                }
                return pvar;
            }
        }

        public object CurrentDocumentView
        {
            get
            {
                object pvar = null;
                if (this.currentDocumentFrame != null)
                {
                    ErrorHandler.ThrowOnFailure(this.currentDocumentFrame.GetProperty(-3001, out pvar));
                }
                return pvar;
            }
        }

        public object CurrentSelectionContainer
        {
            get
            {
                return this.currentSelectionContainer;
            }
        }

        public object CurrentUndoManager
        {
            get
            {
                return this.currentUndoManager;
            }
        }

        public object CurrentWindow
        {
            get
            {
                object pvar = null;
                if (this.currentWindowFrame != null)
                {
                    ErrorHandler.ThrowOnFailure(this.currentWindowFrame.GetProperty(-3001, out pvar));
                }
                return pvar;
            }
        }

        public object CurrentWindowFrame
        {
            get
            {
                return this.currentWindowFrame;
            }
        }
    }
}
