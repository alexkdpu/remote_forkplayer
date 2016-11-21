using System;
using System.Reflection;

namespace RemoteFork.Plugins {
    internal class PluginInstance {
        public readonly string Id;
        public readonly string Key;
        public readonly string Name;
        public readonly string Description;
        public readonly string ImageLink;
        public readonly string Version;
        public readonly string Author;

        private readonly Assembly assembly;
        private readonly Type type;

        private IPlugin instance;

        public IPlugin Instance { get {
            return instance ?? (instance = assembly.CreateInstance(type.FullName) as IPlugin);
        } }

        public PluginInstance(string key, Assembly assembly, Type type, PluginAttribute attribute) {
            Key = key;
            Id = attribute.Id;
            Name = attribute.Name;
            Description = attribute.Description;
            ImageLink = attribute.ImageLink;
            Version = attribute.Version;
            Author = attribute.Author;
            this.assembly = assembly;
            this.type = type;
        }

        public override string ToString() {
            return string.Format("{0} {1} ({2})", Name, Version, Author);
        }
    }
}
