using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CSharp;
using RemoteFork.Properties;

namespace RemoteFork.Plugins {
    internal class PluginManager {
        public static readonly PluginManager Instance = new PluginManager();

        private readonly Dictionary<string, PluginInstance> plugins = new Dictionary<string, PluginInstance>();

        private PluginManager() {
            LoadPlugins();
        }

        public static Assembly CompileAssembly(string sourceFile) {
            var codeProvider = new CSharpCodeProvider();

            var compilerParameters = new CompilerParameters {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };

            compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");

            var result = codeProvider.CompileAssemblyFromFile(compilerParameters, sourceFile);

            var assembly = result.CompiledAssembly;

            return assembly;
        }

        private void LoadPlugins() {
            string pathPlugins = Path.Combine(Environment.CurrentDirectory, "Plugins");
            if (Directory.Exists(pathPlugins)) {
                var files = Directory.GetFiles(pathPlugins, "*.cs");
                foreach (string file in files) {
                    try {
                        var results = CompileAssembly(file);
                        var types = results.GetExportedTypes();
                        if (types.Length > 0) {
                            var type = types[0];
                            if (type.BaseType == typeof (BasePlugin)) {
                                var attributes = type.GetCustomAttributes(true);

                                if (attributes.Length > 0) {
                                    var attribute =
                                        (PluginAttribute)
                                            attributes.FirstOrDefault(i => i.GetType() == typeof (PluginAttribute));
                                    if (attribute != null) {
                                        var plugin = new PluginInstance(GetChecksum(file), results, type, attribute);
                                        if (!plugins.ContainsKey(plugin.Name)) {
                                            plugins.Add(plugin.Id, plugin);
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Debug("LoadPlugins->{0}: {1}", file, ex);
                    }
                }
            }
        }
        private static string GetChecksum(string file) {
            using (var stream = File.OpenRead(file)) {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public void ReimportPlugins() {
            plugins.Clear();
            LoadPlugins();
        }

        public void RemovePlugin(string name) {
            if (plugins.ContainsKey(name)) {
                plugins.Remove(name);
            }
        }

        public Dictionary<string, PluginInstance> GetPlugins(bool filtering = true) {
            if (filtering) {
                var dict = new Dictionary<string, PluginInstance>();

                if (Settings.Default.Plugins && Settings.Default.EnablePlugins != null) {
                    foreach (var plugin in plugins.Where(plugin => Settings.Default.EnablePlugins.Contains(plugin.Value.Key))) {
                        dict.Add(plugin.Key, plugin.Value);
                    }
                }

                return dict;
            }
            return plugins;
        }

        public PluginInstance GetPlugin(string name) {
            if (plugins.ContainsKey(name)) {
                if (Settings.Default.Plugins && Settings.Default.EnablePlugins != null) {
                    if (Settings.Default.EnablePlugins.Contains(plugins[name].Key)) {
                        return plugins[name];
                    }
                }
            }

            return null;
        }
    }
}
