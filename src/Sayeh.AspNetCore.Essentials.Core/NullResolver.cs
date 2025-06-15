using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace System;
public static class NullResolver
{

    #region Null Checker

    public static bool None([NotNullWhen(false)] this object? value)
    {
        if (value is null)
            return true;
        return false;
    }

    public static bool None([NotNullWhen(false)] this string? value)
    {
        if (value is null)
            return true;
        if (string.IsNullOrEmpty(value.Trim()))
            return true;
        return false;
    }

    public static bool HasValue([NotNullWhen(true)] this string? value) => !value.None();

    public static bool None([NotNullWhen(false)] this Guid value) => value.Equals(Guid.Empty);

    public static bool HasValue([NotNullWhen(true)] this Guid value) => !value.Equals(Guid.Empty);

    public static bool None([NotNullWhen(false)] this Nullable<Guid> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullWhen(true)] this Nullable<Guid> value) => !value.None();

    public static bool None([NotNullWhen(false)] this decimal value) => value == 0;

    public static bool HasValue([NotNullWhen(true)] this decimal value) => value != 0;

    public static bool None([NotNullWhen(false)] this Nullable<decimal> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullWhen(true)] this Nullable<decimal> value) => !value.None();

    public static bool None([NotNullWhen(false)] this TimeSpan value) => value == TimeSpan.Zero;

    public static bool HasValue([NotNullWhen(true)] this TimeSpan value) => value != TimeSpan.Zero;

    public static bool None([NotNullWhen(false)] this Nullable<TimeSpan> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullWhen(true)] this Nullable<TimeSpan> value) => !value.None();
    
    public static bool None([NotNullWhen(false)] this DateTime value) => value == DateTime.MinValue;

    public static bool HasValue([NotNullWhen(true)] this DateTime value) => value != DateTime.MinValue;

    public static bool None([NotNullWhen(false)] this Nullable<DateTime> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullWhen(true)] this Nullable<DateTime> value) => !value.None();

    public static bool None([NotNullWhen(false)] this double value) => double.IsNaN(value) || value == 0;

    public static bool HasValue([NotNullWhen(true)] this double value) => !value.None();

    public static bool None([NotNullWhen(false)] this Nullable<double> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullWhen(false)] this int value)=> value == 0;

    public static bool HasValue([NotNullWhen(true)] this int value) => value != 0;

    public static bool None([NotNullWhen(false)] this Nullable<int> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue([NotNullWhen(true)] this Nullable<int> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullWhen(false)] this short value) => value == (short)0;

    public static bool HasValue([NotNullWhen(true)] this short value) => value != (short)0;

    public static bool None([NotNullWhen(false)] this Nullable<short> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullWhen(false)] this float value) =>  value == (float)0;

    public static bool HasValue([NotNullWhen(true)] this float value) => value != (float)0;

    public static bool None([NotNullWhen(false)] this Nullable<float> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None([NotNullWhen(false)] this ICollection value) => value == null || value.Count == 0;

    public static bool HasValue([NotNullWhen(true)] this ICollection value) => !value.None();

    public static bool None([NotNullWhen(false)] this IEnumerable value) => value == null || !value.Cast<object>().Any();

    public static bool HasValue([NotNullWhen(true)] this IEnumerable value) => !value.None();

    public static bool None([NotNullWhen(false)] this byte[] value) => value == null || value.Count() == 0;

    public static bool HasValue([NotNullWhen(true)] this byte[] value) => !value.None();

#if NETCOREAPP

    public static bool None([NotNullWhen(false)] this NetTopologySuite.Geometries.Point value) => value == null || value.IsEmpty;

    public static bool HasValue([NotNullWhen(true)] this NetTopologySuite.Geometries.Point value) => !value.None();

    public static NetTopologySuite.Geometries.Point Or([NotNullIfNotNull("ReturnValue")] this NetTopologySuite.Geometries.Point value, NetTopologySuite.Geometries.Point ReturnValue) {
        if (value!.None())
            return ReturnValue;
        return value;
    }

#endif

    #endregion

    #region Null Replace

    public static T Or<T>([NotNullIfNotNull("ReturnValue")] this T value, T ReturnValue)
    {
        if (value!.None())
            return ReturnValue;
        return value;
    }

    public static string Or([NotNullIfNotNull("ReturnValue")] this string value, string ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid Or([NotNullIfNotNull("ReturnValue")] this Guid value, Guid ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid Or([NotNullIfNotNull("ReturnValue")] this Nullable<Guid> value, Guid ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static decimal Or(this decimal value, decimal ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static decimal Or([NotNullIfNotNull("ReturnValue")] this Nullable<decimal> value, decimal ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static TimeSpan Or(this TimeSpan value, TimeSpan ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static TimeSpan Or([NotNullIfNotNull("ReturnValue")] this Nullable<TimeSpan> value, TimeSpan ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static DateTime Or( this DateTime value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static DateTime Or([NotNullIfNotNull("ReturnValue")] this Nullable<DateTime> value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static double Or( this double value, double ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static double Or([NotNullIfNotNull("ReturnValue")] this Nullable<double> value, double ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static short Or(this short value, short ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static short Or([NotNullIfNotNull("ReturnValue")] this Nullable<short> value, short ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static float Or(this float value, float ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static float Or([NotNullIfNotNull("ReturnValue")] this Nullable<float> value, float ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    #endregion

}


