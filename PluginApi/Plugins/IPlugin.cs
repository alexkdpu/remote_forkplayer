using PluginApi.Plugins;

namespace RemoteFork.Plugins {
    public interface IPlugin {
        Item[] GetList(IPluginContext context);
    }
}
