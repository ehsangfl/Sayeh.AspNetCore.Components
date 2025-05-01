using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace System;
public static class NullResolver
{

    #region Null Checker

    public static bool None([NotNullIfNotNull("value")] this object value)
    {
        if (value is null)
            return true;
        return false;
    }

    public static bool None([NotNullIfNotNull("value")]this string value)
    {
        if (value == null)
            return true;
        if (string.IsNullOrEmpty(((string)value).Trim()))
            return true;
        return false;
    }

    public static bool HasValue([NotNullIfNotNull("value")] this string value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this Guid value) => value.Equals(Guid.Empty);

    public static bool HasValue([NotNullIfNotNull("value")] this Guid value) => !value.Equals(Guid.Empty);

    public static bool None([NotNullIfNotNull("value")] this Nullable<Guid> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullIfNotNull("value")] this Nullable<Guid> value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this decimal value) => value == 0;

    public static bool HasValue([NotNullIfNotNull("value")] this decimal value) => value != 0;

    public static bool None([NotNullIfNotNull("value")] this Nullable<decimal> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullIfNotNull("value")] this Nullable<decimal> value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this TimeSpan value) => value == TimeSpan.Zero;

    public static bool HasValue([NotNullIfNotNull("value")] this TimeSpan value) => value != TimeSpan.Zero;

    public static bool None([NotNullIfNotNull("value")] this Nullable<TimeSpan> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullIfNotNull("value")] this Nullable<TimeSpan> value) => !value.None();
    
    public static bool None([NotNullIfNotNull("value")] this DateTime value) => value == DateTime.MinValue;

    public static bool HasValue([NotNullIfNotNull("value")] this DateTime value) => value != DateTime.MinValue;

    public static bool None([NotNullIfNotNull("value")] this Nullable<DateTime> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullIfNotNull("value")] this Nullable<DateTime> value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this double value) => double.IsNaN(value) || value == 0;

    public static bool HasValue([NotNullIfNotNull("value")] this double value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this Nullable<double> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullIfNotNull("value")] this int value)=> value == 0;

    public static bool HasValue([NotNullIfNotNull("value")] this int value) => value != 0;

    public static bool None([NotNullIfNotNull("value")] this Nullable<int> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullIfNotNull("value")] this short value) => value == (short)0;

    public static bool HasValue([NotNullIfNotNull("value")] this short value) => value != (short)0;

    public static bool None([NotNullIfNotNull("value")] this Nullable<short> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullIfNotNull("value")] this float value) =>  value == (float)0;

    public static bool HasValue([NotNullIfNotNull("value")] this float value) => value != (float)0;

    public static bool None([NotNullIfNotNull("value")] this Nullable<float> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullIfNotNull("value")] this ICollection value) => value == null || value.Count == 0;

    public static bool HasValue([NotNullIfNotNull("value")] this ICollection value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this IEnumerable value) => value == null || !value.Cast<object>().Any();

    public static bool HasValue([NotNullIfNotNull("value")] this IEnumerable value) => !value.None();

    public static bool None([NotNullIfNotNull("value")] this byte[] value) => value == null || value.Count() == 0;

    public static bool HasValue([NotNullIfNotNull("value")] this byte[] value) => !value.None();

#if NETCOREAPP

    public static bool None([NotNullIfNotNull("value")] this NetTopologySuite.Geometries.Point value) => value == null || value.IsEmpty;

    public static bool HasValue([NotNullIfNotNull("value")] this NetTopologySuite.Geometries.Point value) => !value.None();

    public static NetTopologySuite.Geometries.Point Or(this NetTopologySuite.Geometries.Point value, NetTopologySuite.Geometries.Point ReturnValue) {
        if (value!.None())
            return ReturnValue;
        return value;
    }

#endif

    #endregion

    #region Null Replace

    public static T Or<T>([NotNullIfNotNull("value")] this T value, T ReturnValue)
    {
        if (value!.None())
            return ReturnValue;
        return value;
    }

    public static string Or([NotNullIfNotNull("value")] this string value, string ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid Or([NotNullIfNotNull("value")] this Guid value, Guid ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid  Or([NotNullIfNotNull("value")] this Nullable<Guid> value, Guid ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static decimal Or([NotNullIfNotNull("value")] this decimal value, decimal ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static decimal Or([NotNullIfNotNull("value")] this Nullable<decimal> value, decimal ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static TimeSpan Or([NotNullIfNotNull("value")] this TimeSpan value, TimeSpan ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static TimeSpan Or([NotNullIfNotNull("value")] this Nullable<TimeSpan> value, TimeSpan ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static DateTime Or([NotNullIfNotNull("value")] this DateTime value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static DateTime Or([NotNullIfNotNull("value")] this Nullable<DateTime> value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static double Or([NotNullIfNotNull("value")] this double value, double ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static double Or([NotNullIfNotNull("value")] this Nullable<double> value, double ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static short Or([NotNullIfNotNull("value")] this short value, short ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static short Or([NotNullIfNotNull("value")] this Nullable<short> value, short ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static float Or([NotNullIfNotNull("value")] this float value, float ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static float Or([NotNullIfNotNull("value")] this Nullable<float> value, float ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    #endregion

}


