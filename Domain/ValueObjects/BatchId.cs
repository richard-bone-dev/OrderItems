using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<BatchId>))]
public sealed class BatchId : StronglyTypedId<BatchId>
{
    public BatchId(Guid value) : base(value) { }
    public static BatchId New() => new(Guid.NewGuid());
}
