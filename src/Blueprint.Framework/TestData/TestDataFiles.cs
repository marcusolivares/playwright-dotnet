using System.Text.Json;
using Blueprint.Framework.Utilities;

namespace Blueprint.Framework.TestData;

/// <summary>
/// Static test data under TestData/Files (JSON datasets, upload fixtures), copied next to the test assembly at build
/// time. Paths resolve from the output folder, never from the current working directory.
/// </summary>
public static class TestDataFiles
{
    private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "TestData", "Files");

    /// <summary>Absolute path of a test data file, e.g. for <c>SetInputFilesAsync</c>.</summary>
    public static string PathOf(string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(Root, relativePath));

        return File.Exists(path)
            ? path
            : throw new FileNotFoundException($"Test data file '{relativePath}' was not found in {Root}.", path);
    }

    /// <summary>Deserializes a camelCase JSON file, e.g. the cases of a data-driven test.</summary>
    public static T LoadJson<T>(string relativePath)
    {
        using var stream = File.OpenRead(PathOf(relativePath));

        return JsonSerializer.Deserialize<T>(stream, JsonDefaults.Files)
            ?? throw new InvalidDataException($"Test data file '{relativePath}' is empty.");
    }
}
