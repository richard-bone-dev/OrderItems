using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<PaymentId>))]
public sealed class PaymentId : StronglyTypedId<PaymentId>
{
    public PaymentId(Guid value) : base(value) { }
    public static PaymentId New() => new(Guid.NewGuid());
}
