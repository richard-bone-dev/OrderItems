using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIntIdTypeConverter<BatchNumber>))]
public sealed class BatchNumber : StronglyTypedIntId<BatchNumber>
{
    public BatchNumber(int value) : base(value) { }

    public static BatchNumber New(int value) => new(value);
}