using System.ComponentModel;
using System.Globalization;

namespace Api.Domain.Core;

public class StronglyTypedIntIdTypeConverter<T> : TypeConverter where T : StronglyTypedIntId<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || sourceType == typeof(int);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is int i) return Activator.CreateInstance(typeof(T), i);
        if (value is string s && int.TryParse(s, out var result))
            return Activator.CreateInstance(typeof(T), result);

        throw new NotSupportedException($"Cannot convert '{value}' to {typeof(T).Name}");
    }
}