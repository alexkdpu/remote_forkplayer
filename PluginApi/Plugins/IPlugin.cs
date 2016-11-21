using PluginApi.Plugins;

namespace RemoteFork.Plugins {
    public interface IPlugin {
        Response GetList(IPluginContext context);
    }
}
