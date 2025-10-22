using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<BatchId>))]
public sealed class BatchId : StronglyTypedId<BatchId>
{
    public BatchId() : base() { }
    public BatchId(Guid value) : base(value) { }
    public static BatchId New() => new(Guid.NewGuid());
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<BatchStockId>))]
public sealed class BatchStockId : StronglyTypedId<BatchStockId>
{
    public BatchStockId() : base() { }
    public BatchStockId(Guid value) : base(value) { }
    public static BatchStockId New() => new(Guid.NewGuid());
}