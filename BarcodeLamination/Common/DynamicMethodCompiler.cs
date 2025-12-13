using System.Reflection;
using System.Reflection.Emit;

internal delegate object? GetterDelegate(object obj);
internal delegate void SetterDelegate(object obj, object? value);

internal static class DynamicMethodCompiler
{
    internal static GetterDelegate CreateGetterDelegate(Type type, string name)
    {
        var property = type.GetProperty(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException($"Could not find property '{name}' on type '{type}'.");
        var getMethod = property.GetGetMethod(true)
            ?? throw new MissingMethodException($"Could not find a getter for property '{name}' on type '{type}'.");

        var dynamicGet = new DynamicMethod($"Get{name}", typeof(object), [typeof(object)], type, true);
        var generator = dynamicGet.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Call, getMethod);
        BoxIfNeeded(getMethod.ReturnType, generator);
        generator.Emit(OpCodes.Ret);

        return dynamicGet.CreateDelegate<GetterDelegate>();
    }

    internal static SetterDelegate CreateSetterDelegate(Type type, string name)
    {
        var property = type.GetProperty(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException($"Could not find property '{name}' on type '{type}'.");
        var setMethod = property.GetSetMethod(true)
            ?? throw new MissingMethodException($"Could not find a setter for property '{name}' on type '{type}'.");

        var dynamicSet = new DynamicMethod($"Set{name}", typeof(void), [typeof(object), typeof(object)], type, true);
        var generator = dynamicSet.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldarg_1);
        UnboxIfNeeded(setMethod.GetParameters()[0].ParameterType, generator);
        generator.Emit(OpCodes.Call, setMethod);
        generator.Emit(OpCodes.Ret);

        return dynamicSet.CreateDelegate<SetterDelegate>();
    }


    private static void BoxIfNeeded(Type type, ILGenerator generator)
    {
        if (type.IsValueType)
        {
            generator.Emit(OpCodes.Box, type);
        }
    }

    private static void UnboxIfNeeded(Type type, ILGenerator generator)
    {
        if (type.IsValueType)
        {
            generator.Emit(OpCodes.Unbox_Any, type);
        }
    }
}