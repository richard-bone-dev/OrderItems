using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<PaymentStatusId>))]
public sealed class PaymentStatusId : StronglyTypedId<PaymentStatusId>
{
    public PaymentStatusId(Guid value) : base(value) { }
    public static PaymentStatusId New() => new(Guid.NewGuid());
}
