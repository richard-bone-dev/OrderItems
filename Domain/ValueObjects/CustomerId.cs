using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<CustomerId>))]
public sealed class CustomerId : StronglyTypedId<CustomerId>
{
    //public CustomerId() : base() { }
    public CustomerId(Guid value) : base(value) { }
    public static CustomerId New() => new(Guid.NewGuid());
}
