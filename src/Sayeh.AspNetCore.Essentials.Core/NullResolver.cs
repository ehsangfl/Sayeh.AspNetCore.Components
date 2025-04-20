using System.Collections;

namespace System;
public static class NullResolver
{

    #region Null Checker

    public static bool None(this object value)
    {
        if (value is null)
            return true;
        return false;
    }

    public static bool None(this string value)
    {
        if (value == null)
            return true;
        if (string.IsNullOrEmpty(((string)value).Trim()))
            return true;
        return false;
    }

    public static bool HasValue(this string value) => !value.None();

    public static bool None(this Guid value) => value.Equals(Guid.Empty);

    public static bool HasValue(this Guid value) => !value.Equals(Guid.Empty);

    public static bool None(this Nullable<Guid> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue(this Nullable<Guid> value) => !value.None();

    public static bool None(this decimal value) => value == 0;

    public static bool HasValue(this decimal value) => value != 0;

    public static bool None(this Nullable<decimal> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue(this Nullable<decimal> value) => !value.None();

    public static bool None(this TimeSpan value) => value == TimeSpan.Zero;

    public static bool HasValue(this TimeSpan value) => value != TimeSpan.Zero;

    public static bool None(this Nullable<TimeSpan> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue(this Nullable<TimeSpan> value) => !value.None();
    
    public static bool None(this DateTime value) => value == DateTime.MinValue;

    public static bool HasValue(this DateTime value) => value != DateTime.MinValue;

    public static bool None(this Nullable<DateTime> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool HasValue(this Nullable<DateTime> value) => !value.None();

    public static bool None(this double value) => double.IsNaN(value) || value == 0;

    public static bool HasValue(this double value) => !value.None();

    public static bool None(this Nullable<double> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None(this int value)=> value == 0;

    public static bool HasValue(this int value) => value != 0;

    public static bool None(this Nullable<int> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None(this short value) => value == (short)0;

    public static bool HasValue(this short value) => value != (short)0;

    public static bool None(this Nullable<short> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None(this float value) =>  value == (float)0;

    public static bool HasValue(this float value) => value != (float)0;

    public static bool None(this Nullable<float> value)
    {
        if (value.HasValue)
            return value.Value.None();
        return true;
    }

    public static bool None(this ICollection value) => value == null || value.Count == 0;

    public static bool HasValue(this ICollection value) => !value.None();

    public static bool None(this IEnumerable value) => value == null || !value.Cast<object>().Any();

    public static bool HasValue(this IEnumerable value) => !value.None();

    public static bool None(this byte[] value) => value == null || value.Count() == 0;

    public static bool HasValue(this byte[] value) => !value.None();

#if NETCOREAPP

    public static bool None(this NetTopologySuite.Geometries.Point value) => value == null || value.IsEmpty;

    public static bool HasValue(this NetTopologySuite.Geometries.Point value) => !value.None();

    public static NetTopologySuite.Geometries.Point Or(this NetTopologySuite.Geometries.Point value, NetTopologySuite.Geometries.Point ReturnValue) {
        if (value!.None())
            return ReturnValue;
        return value;
    }

#endif

    #endregion

    #region Null Replace

    public static T Or<T>(this T value, T ReturnValue)
    {
        if (value!.None())
            return ReturnValue;
        return value;
    }

    public static string Or(this string value, string ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid Or(this Guid value, Guid ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static Guid  Or(this Nullable<Guid> value, Guid ReturnValue)
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

    public static decimal Or(this Nullable<decimal> value, decimal ReturnValue)
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

    public static TimeSpan Or(this Nullable<TimeSpan> value, TimeSpan ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static DateTime Or(this DateTime value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static DateTime Or(this Nullable<DateTime> value, DateTime ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    public static double Or(this double value, double ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value;
    }

    public static double Or(this Nullable<double> value, double ReturnValue)
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

    public static short Or(this Nullable<short> value, short ReturnValue)
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

    public static float Or(this Nullable<float> value, float ReturnValue)
    {
        if (value.None())
            return ReturnValue;
        return value!.Value;
    }

    #endregion

}


