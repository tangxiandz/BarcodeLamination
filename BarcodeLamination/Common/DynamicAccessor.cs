using System.Collections.Concurrent;

internal sealed class DynamicAccessor(Type targetType) : IDynamicAccessor
{
    private readonly Type _targetType = targetType;
    private readonly ConcurrentDictionary<string, GetterDelegate> _getters = new();
    private readonly ConcurrentDictionary<string, SetterDelegate> _setters = new();

    public object? GetProperty(object obj, string name)
    {
        var getter = _getters.GetOrAdd(name, key => DynamicMethodCompiler.CreateGetterDelegate(_targetType, key));
        return getter.Invoke(obj);
    }

    public void SetProperty(object obj, string name, object? value)
    {
        var setter = _setters.GetOrAdd(name, key => DynamicMethodCompiler.CreateSetterDelegate(_targetType, key));
        setter.Invoke(obj, value);
    }
}