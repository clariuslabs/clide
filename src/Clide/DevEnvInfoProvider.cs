using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DevEnvInfoProvider
    {
        [Export(typeof(JoinableLazy<DevEnvInfo>))]
        JoinableLazy<DevEnvInfo> devEnvInfo;

        [ImportingConstructor]
        public DevEnvInfoProvider(Lazy<IServiceLocator> serviceLocator, JoinableTaskContext context)
        {
            devEnvInfo = JoinableLazy.Create(() =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return GetDevEnvInfo(serviceLocator.Value);
            }, context.Factory, executeOnMainThread: true);
        }

        [Export(typeof(DevEnvInfo))]
        DevEnvInfo DevEnvInfo => devEnvInfo.GetValue();

        DevEnvInfo GetDevEnvInfo(IServiceLocator serviceLocator)
        {
            var vsAppId = serviceLocator.TryGetService<SVsAppId, IVsAppId>();

            if (vsAppId == null)
                return null;

            var workloads = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_IsolationInstallationWorkloads);
            var workloadsList = workloads?.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
            var packages = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_IsolationInstallationPackages);
            var packagesList = packages?.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

            return new DevEnvInfo
            {
                ChannelId = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_ChannelId),
                ChannelTitle = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_ChannelTitle),
                Edition = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_SKUName),
                InstallationID = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_IsolationInstallationId),
                InstallationName = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_IsolationInstallationName),
                Version = Version.TryParse(vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_IsolationInstallationVersion), out var version) ? version : default(Version),
                DisplayVersion = vsAppId.GetProperty<string>(VSAPropID.VSAPROPID_ProductDisplayVersion),
                Workloads = workloadsList,
                Packages = packagesList
            };
        }
    }
}
