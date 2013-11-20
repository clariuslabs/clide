namespace Clide
{
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell.Settings;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

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
        public void WhenRetrievingSettings_ThenSavesDefaultTracingLevelValue()
        {
            var devEnv = DevEnv.Get(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

            var settings = devEnv.ServiceLocator.GetInstance<ClideSettings>();

            Assert.NotNull(settings);

            var defaultValue = Reflect<ClideSettings>.GetProperty(x => x.TracingLevel)
                .GetCustomAttributes(typeof(DefaultValueAttribute), true)
                .OfType<DefaultValueAttribute>()
                .Select(d => (SourceLevels)d.Value)
                .First();

            var collection = SettingsManager.GetSettingsCollectionName(typeof(ClideSettings));
            Assert.True(settingsStore.CollectionExists(collection));
            Assert.Equal(defaultValue, (SourceLevels)Enum.Parse(typeof(SourceLevels), settingsStore.GetString(collection, Reflect<ClideSettings>.GetPropertyName(x => x.TracingLevel), "-1")));
        }

    }
}
