using System.ComponentModel;
using System.Globalization;

namespace Api.Domain.Core;

public class StronglyTypedIdTypeConverter<T> : TypeConverter where T : StronglyTypedId<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string s && Guid.TryParse(s, out var guid))
        {
            return Activator.CreateInstance(typeof(T), guid);
        }

        throw new NotSupportedException($"Cannot convert '{value}' to {typeof(T).Name}");
    }
}