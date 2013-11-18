namespace Clide
{
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell.Settings;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class ClideSettingsSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        private WritableSettingsStore settingsStore;

        [TestInitialize]
        public void Initialize()
        {
            var shellManager = new ShellSettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
            this.settingsStore = shellManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            var collection = SettingsManager.GetSettingsCollectionName(typeof(ClideSettings));
            if (this.settingsStore.CollectionExists(collection))
                this.settingsStore.DeleteCollection(collection);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingSettings_ThenSavesLogCompositionDefaultValue()
        {
            var devEnv = DevEnv.Get(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

            var settings = devEnv.ServiceLocator.GetInstance<ClideSettings>();

            Assert.NotNull(settings);

            var collection = SettingsManager.GetSettingsCollectionName(typeof(ClideSettings));
            Assert.True(settingsStore.CollectionExists(collection));
            Assert.False(bool.Parse(
                settingsStore.GetString(collection, Reflect<ClideSettings>.GetPropertyName(x => x.TracingLevel), "True")));
        }

    }
}
