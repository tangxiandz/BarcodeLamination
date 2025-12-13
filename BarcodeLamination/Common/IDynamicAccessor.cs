public interface IDynamicAccessor
{
    object? GetProperty(object obj, string name);
    void SetProperty(object obj, string name, object? value);
}

public interface ICustomAttributeCollection
{
    bool Contains(string name);

    ICustomAttribute? this[string name] { get; }
}

public interface ICustomAttribute
{
    string Name { get; }
    string? Value { get; set; }
}