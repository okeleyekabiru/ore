using System.Linq;

namespace Ore.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
        {
            return false;
        }

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public bool Equals(ValueObject? other) => Equals(other as object);

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Where(x => x is not null)
            .Aggregate(0, HashCode.Combine);
    }
}
