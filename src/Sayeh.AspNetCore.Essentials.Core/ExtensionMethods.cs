﻿namespace Sayeh.Essentials.Core;

public static class ExtensionMethods
{
    public static bool IsNullableType(this Type type)
    {
        return (type == typeof(string)) || (((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>))) || (!type.IsPrimitive && type.IsClass);
    }

    public static Nullable<DateTime> ToDateTime<TValue>(this TValue value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is DateTime)
        {
            return (DateTime)(object)value;
        }

        if (value is DateTime?)
        {
            return ((DateTime?)(object)value).Value;
        }

        if (value is IConvertible)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch (FormatException)
            {
                throw new InvalidCastException("The TValue provided cannot be converted to DateTime.");
            }
        }
        throw new InvalidCastException("The TValue provided does not implement IConvertible.");
    }
}
