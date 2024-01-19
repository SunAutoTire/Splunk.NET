using System.Reflection;

namespace SunAuto.Extensions;

public static partial class Methods
{
    /// <summary>
    /// Get embedded resource
    /// </summary>
    /// <param name="type">Any type in the assembly containing the embedded resource.</param>
    /// <param name="path">Dot delimited path to resource in the assembly project.</param>
    /// <returns>String content of the embedded resource.</returns>
    public static async Task<string> GetEmbeddedResourceAsync(this Type type, string path)
    {
        var result = default(string);

        using (var reader = type.GetEmbeddedResourceReader(path))
            result = await reader.ReadToEndAsync();

        return result;
    }

    /// <summary>
    /// Get embedded resource
    /// </summary>
    /// <param name="type">Any type in the assembly containing the embedded resource.</param>
    /// <param name="path">Dot delimited path to resource in the assembly project. <code>{DefaultNamespace}.[FolderName1]...[FolderNameN].{Filename}</code> (Case sensitive including extension, and don't use assembly name)</param>
    /// <returns>String content of the embedded resource.</returns>
    public static string GetEmbeddedResource(this Type type, string path)
    {
        var result = default(string);

        using (var reader = type.GetEmbeddedResourceReader(path))
            result = reader.ReadToEnd();

        return result;
    }

    /// <summary>
    /// Get embedded resource
    /// </summary>
    /// <param name="type">Any type in the assembly containing the embedded resource.</param>
    /// <param name="path">Dot delimited path to resource in the assembly project. <code>{DefaultNamespace}.[FolderName1]...[FolderNameN].{Filename}</code> (Case sensitive including extension, and don't use assembly name)</param>
    /// <returns>Stream reader of a stream of the embedded resource.</returns>
    public static StreamReader GetEmbeddedResourceReader(this Type type, string path)
    {
        var assembly = type.GetTypeInfo().Assembly;
        var resourcestream = assembly.GetManifestResourceStream(path);

        if (resourcestream == null)
            throw new ArgumentException("The argument does not represent a valid embedded resource which was specified during compilation, or the resource is not visible to the caller", nameof(path));
        else
            return new StreamReader(resourcestream);
    }
}
