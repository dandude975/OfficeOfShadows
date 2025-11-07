using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace OOS.Shared
{
    public enum ItemKind { File, Directory, Shortcut }

    public record ManifestItem
    {
        public string Path { get; set; } = "";
        public ItemKind Kind { get; set; }
        public string? Source { get; set; } // optional: for files copied from Assets
    }

    public record Discrepancy(ManifestItem Item, string Kind, string FullPath, string Details);

    public class SandboxManifest
    {
        public List<ManifestItem> Items { get; set; } = new();

        public static SandboxManifest Load(string path)
        {
            var json = File.ReadAllText(path);

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // accept "shortcut", "Shortcut", etc.
            opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

            return JsonSerializer.Deserialize<SandboxManifest>(json, opts)
                   ?? new SandboxManifest();
        }

        public IEnumerable<Discrepancy> Validate(string sandboxRoot)
        {
            foreach (var it in Items)
            {
                var full = System.IO.Path.Combine(sandboxRoot, it.Path);
                bool exists = it.Kind switch
                {
                    ItemKind.Directory => System.IO.Directory.Exists(full),
                    _ => System.IO.File.Exists(full),
                };
                if (!exists)
                    yield return new Discrepancy(it, "Missing", full, "");
            }
        }
    }
}
