using Microsoft.VisualStudio.ComponentModelHost;
using Xunit;

namespace Clide
{
    public class DevEnvInfoProviderSpec
    {
        [VsFact]
        public void when_getting_device_info_then_can_get_app_id()
        {
            var info = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<DevEnvInfo>();

            Assert.NotNull(info);
            Assert.NotEmpty(info.ChannelId);

            if (info.Version.Major >= 16)
            {
                // These are only available in dev16+
                Assert.NotEmpty(info.ChannelSuffix);
                Assert.NotEmpty(info.ChannelTitle);
            }
        }
    }
}
