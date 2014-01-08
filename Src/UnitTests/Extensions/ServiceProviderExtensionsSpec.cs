namespace UnitTests.Extensions
{
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Xunit;

    public class ServiceProviderExtensionsSpec
    {
        [Fact]
        public void when_service_provider_has_no_guid_then_get_guid_throws()
        {
            var service = Mock.Of<IServiceProvider>();

            Assert.Throws<ArgumentException>(() => service.GetPackageGuidOrThrow());
        }

        [Fact]
        public void when_service_provider_has_no_guid_then_get_loaded_package_throws()
        {
            var service = Mock.Of<IServiceProvider>();

            Assert.Throws<ArgumentException>(() => service.GetLoadedPackage<IServiceProvider>());
        }

        [Fact]
        public void when_shell_fails_to_load_package_then_throws()
        {
            var guid = new Guid("D9F1F0C7-576E-47A7-8D63-BFB2D506C4E4");
            IVsPackage package;
            var service = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(SVsShell)) == Mock.Of<IVsShell>(
                    shell => shell.LoadPackage(ref guid, out package) == -1));

            Assert.Throws<COMException>(() => service.GetLoadedPackage(guid));
        }

        [Fact]
        public void when_shell_fails_to_load_package_by_guid_then_throws()
        {
            var guid = new Guid("D9F1F0C7-576E-47A7-8D63-BFB2D506C4E4");
            IVsPackage package;
            var service = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(SVsShell)) == Mock.Of<IVsShell>(
                    shell => shell.LoadPackage(ref guid, out package) == -1));

            Assert.Throws<COMException>(() => service.GetLoadedPackage<FakePackage>());
        }

        [Guid("D9F1F0C7-576E-47A7-8D63-BFB2D506C4E4")]
        public class FakePackage : IVsPackage
        {
            public int Close()
            {
                throw new NotImplementedException();
            }

            public int CreateTool(ref Guid rguidPersistenceSlot)
            {
                throw new NotImplementedException();
            }

            public int GetAutomationObject(string pszPropName, out object ppDisp)
            {
                throw new NotImplementedException();
            }

            public int GetPropertyPage(ref Guid rguidPage, VSPROPSHEETPAGE[] ppage)
            {
                throw new NotImplementedException();
            }

            public int QueryClose(out int pfCanClose)
            {
                throw new NotImplementedException();
            }

            public int ResetDefaults(uint grfFlags)
            {
                throw new NotImplementedException();
            }

            public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
            {
                throw new NotImplementedException();
            }
        }
    }
}
