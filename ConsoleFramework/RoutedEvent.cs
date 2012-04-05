using System;

namespace ConsoleFramework
{
    public enum RoutingStrategy {
        Tunnel,
        Bubble,
        Direct
    }

    /// <summary>
    /// Key for internal usage in routed event management maps.
    /// </summary>
    public sealed class RoutedEventKey : IEquatable<RoutedEventKey> {
        private readonly string name;
        private readonly Type ownerType;

        public string Name {
            get {
                return name;
            }
        }

        public Type OwnerType {
            get {
                return ownerType;
            }
        }

        public RoutedEventKey(string name, Type ownerType) {
            this.name = name;
            this.ownerType = ownerType;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(RoutedEventKey other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.name, name) && Equals(other.ownerType, ownerType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (RoutedEventKey)) return false;
            return Equals((RoutedEventKey) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() {
            unchecked {
                return ((name != null ? name.GetHashCode() : 0)*397) ^ (ownerType != null ? ownerType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(RoutedEventKey left, RoutedEventKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(RoutedEventKey left, RoutedEventKey right) {
            return !Equals(left, right);
        }
    }

    public sealed class RoutedEvent {
        private readonly Type handlerType;
        private readonly string name;
        private readonly Type ownerType;
        private readonly RoutingStrategy routingStrategy;

        public RoutedEvent(Type handlerType, string name, Type ownerType, RoutingStrategy routingStrategy) {
            this.handlerType = handlerType;
            this.name = name;
            this.ownerType = ownerType;
            this.routingStrategy = routingStrategy;
        }

        public Type HandlerType {
            get {
                return handlerType;
            }
        }

        public string Name {
            get {
                return name;
            }
        }

        public Type OwnerType {
            get {
                return ownerType;
            }
        }

        public RoutingStrategy RoutingStrategy {
            get {
                return routingStrategy;
            }
        }

        public RoutedEventKey Key {
            get {
                // note : mb cache this
                return new RoutedEventKey(name, ownerType);
            }
        }
    }
}
