using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class TextMeshProTests
{
    private static readonly Dictionary<string, string> s_FileGuids = new Dictionary<string, string>()
    {
        { "Hidden-TMP_Bitmap-Mobile-UIEffect.shader.meta", "guid: 63a1dba10bff94661a909d63808a9a3b" },
        { "Hidden-TMP_Bitmap-UIEffect.shader.meta", "guid: d1cb940e3804b4adcaf9b22083fadfad" },
        { "Hidden-TMP_SDF Overlay-UIEffect.shader.meta", "guid: 03ea49e29b5ad4fbe8e340af208fe8e7" },
        { "Hidden-TMP_SDF SSD-UIEffect.shader.meta", "guid: 26b66f9b09b8d4ac6a2790c3fa8b21c1" },
        { "Hidden-TMP_SDF-Mobile Overlay-UIEffect.shader.meta", "guid: 16f26337621974eaf998b9857bac39c6" },
        { "Hidden-TMP_SDF-Mobile SSD-UIEffect.shader.meta", "guid: 2c69b4dabb5c24ae99889f581c8b556a" },
        { "Hidden-TMP_SDF-Mobile-UIEffect.shader.meta", "guid: e65241fa80a374114b3f55ed746c04d9" },
        { "Hidden-TMP_SDF-UIEffect.shader.meta", "guid: 935b7be1c88464d2eb87204fdfab5a38" }
    };

    private const string k_Version = "userData: v5.6.0";
    private const string k_VersionUnity6 = k_Version + " (Unity 6)";

    [Test]
    public void GuidMatch()
    {
        ForEachMeta(x =>
        {
            var (path, guid, version) = x;
            var fileName = Path.GetFileName(path);
            s_FileGuids.TryGetValue(fileName.Replace("-Unity6", ""), out var expectedGuid);
            var expectedVersion = path.Contains("-Unity6") ? k_VersionUnity6 : k_Version;

            Assert.AreEqual(expectedGuid, guid, $"GUID mismatch: {fileName}");
            Assert.AreEqual(expectedVersion, version, $"Version mismatch: {fileName}");
        });
    }

    [MenuItem("Development/Update GUID, Version for TMP Samples")]
    public static void UpdateGuidVersion()
    {
        ForEachMeta(x =>
        {
            var (path, guid, version) = x;
            var fileName = Path.GetFileName(path);
            s_FileGuids.TryGetValue(fileName.Replace("-Unity6", ""), out var expectedGuid);
            var expectedVersion = path.Contains("-Unity6") ? k_VersionUnity6 : k_Version;

            if (expectedGuid != guid || expectedVersion != version)
            {
                Debug.Log($"Update: {fileName}");
                var meta = File.ReadAllText(path);
                File.WriteAllText(path, meta.Replace(guid, expectedGuid).Replace(version, expectedVersion));
            }
        });
    }


    [MenuItem("Development/List GUID for TMP Samples")]
    public static void ListGuid()
    {
        var sb = new StringBuilder();
        ForEachMeta(x =>
        {
            var (path, guid, _) = x;
            sb.AppendLine($"{Path.GetFileName(path)}: {guid}");
        });
        Debug.Log(sb);
    }

    private static void ForEachMeta(Action<(string path, string guid, string version)> action)
    {
        foreach (var p in Directory.GetFiles("Packages/src/Samples~", "*.shader.meta", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(p);
            if (!fileName.Contains("TMP_")) continue;

            var meta = File.ReadAllText(p);
            var guid = Regex.Match(meta, @"(guid: \w+)$", RegexOptions.Multiline).Groups[1].Value;
            var version = Regex.Match(meta, @"(userData: .*)$", RegexOptions.Multiline).Groups[1].Value;

            action((p, guid, version));
        }
    }
}
