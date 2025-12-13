using System.Collections.Concurrent;

public static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, IDynamicAccessor> cache = new();

    private static IDynamicAccessor GetDynamicAccessor(Type type)
        => cache.GetOrAdd(type, t => new DynamicAccessor(t));

    public static object? GetProperty(this object? obj, string path)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        ArgumentNullException.ThrowIfNull(path, nameof(path));

        var dynamicAccessor = GetDynamicAccessor(obj.GetType());
        int index = path.IndexOf('.');
        if (index == -1)
        {
            return dynamicAccessor.GetProperty(obj, path);
        }

        obj = dynamicAccessor.GetProperty(obj, path[..index]);
        return GetProperty(obj, path[(index + 1)..]);
    }

    public static void SetProperty(this object? obj, string path, object? value)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        ArgumentNullException.ThrowIfNull(path, nameof(path));

        var dynamicAccessor = GetDynamicAccessor(obj.GetType());
        int index = path.IndexOf(".");
        if (index == -1)
        {
            dynamicAccessor.SetProperty(obj, path, value);
            return;
        }

        obj = dynamicAccessor.GetProperty(obj, path[..index]);
        SetProperty(obj, path[(index + 1)..], value);
    }
}