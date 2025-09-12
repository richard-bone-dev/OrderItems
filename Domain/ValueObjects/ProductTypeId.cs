using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ProductTypeId>))]
public sealed class ProductTypeId : StronglyTypedId<ProductTypeId>
{
    public ProductTypeId(Guid value) : base(value) { }
    public static ProductTypeId New() => new(Guid.NewGuid());
}
