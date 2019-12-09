using System;
using System.ComponentModel.Composition;

namespace Clide
{
    [Export(typeof(IDevEnv))]
    class DevEnv : IDevEnv
    {
        public bool IsElevated => throw new NotImplementedException();

        public DevEnvInfo Info => throw new NotImplementedException();

        public IDialogWindowFactory DialogWindowFactory => throw new NotImplementedException();

        public IErrorsManager Errors => throw new NotImplementedException();

        public IMessageBoxService MessageBoxService => throw new NotImplementedException();

        public IOutputWindowManager OutputWindow => throw new NotImplementedException();

        public IServiceLocator ServiceLocator => throw new NotImplementedException();

        public IStatusBar StatusBar => throw new NotImplementedException();

        public void Exit(bool saveAll = true) => throw new NotImplementedException();

        public bool Restart(bool saveAll = true) => throw new NotImplementedException();
    }
}
