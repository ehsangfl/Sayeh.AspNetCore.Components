
namespace Sayeh.Essentials.Core;

public static class Reflection
{
    public static bool IsNullableType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(nameof(type));
        return (type == typeof(string)) || (((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>))) || (!type!.IsPrimitive && type.IsClass);
    }

    public static bool IsA<TType>(this object obj)
    {
        var requestedType = typeof(TType);
        if (obj is Type)
        {
            if (requestedType.IsInterface)
                return requestedType.IsAssignableFrom(obj as Type);
            else
                return ((Type)obj) == typeof(TType);
        }
        else
        {
            if (requestedType.IsInterface)
                return requestedType.IsAssignableFrom(obj.GetType());
            else
                return obj is TType;
        }
    }

    public static TType As<TType>(this object obj) => (TType)obj;

}
