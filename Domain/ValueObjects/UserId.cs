using Api.Domain.Core;
using System.ComponentModel;

namespace Api.Domain.ValueObjects;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<UserId>))]
public sealed class UserId : StronglyTypedId<UserId>
{
    //public UserId() : base() { }
    public UserId(Guid value) : base(value) { }
    public static UserId New() => new(Guid.NewGuid());
}
