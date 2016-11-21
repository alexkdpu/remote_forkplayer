using RemoteFork.Plugins;

namespace PluginApi.Plugins
{
    public class Response
    {
        public static readonly Response EmptyResponse = new Response
        {
            Items = new Item[0]
        };
        
        public string NextPageUrl { get; set; }
        public Item[] Items { get; set; }
    }
}
