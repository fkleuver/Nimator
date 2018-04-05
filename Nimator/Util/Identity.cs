using System;

namespace Nimator.Util
{
    /// <summary>
    /// An identity Value Object with equality comparison.
    /// </summary>
    public sealed class Identity : IEquatable<Identity>
    {
        /// <summary>
        /// A uniquely identifiable name.
        /// </summary>
        public string Name { get; }

        public Identity(string name)
        {
            Guard.AgainstNullAndEmpty(nameof(name), name);
            Name = name;
        }

        public Identity(Type type)
        {
            Guard.AgainstNull(nameof(type), type);
            Name = type.GetClosedGenericTypeName();
        }

        #region IEquatable implementation
        public bool Equals(Identity other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Identity)obj);
        }

        public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;

        public static bool operator ==(Identity left, Identity right) => Equals(left, right);

        public static bool operator !=(Identity left, Identity right) => !Equals(left, right);
        #endregion
    }
}
