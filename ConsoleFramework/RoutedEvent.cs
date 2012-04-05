using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFramework
{
    public enum RoutingStrategy {
        Tunnel,
        Bubble,
        Direct
    }

    public sealed class RoutedEvent {
        private readonly Type handledType;
        private readonly string name;
        private readonly Type ownerType;
        private readonly RoutingStrategy routingStrategy;

        public RoutedEvent(Type handledType, string name, Type ownerType, RoutingStrategy routingStrategy) {
            this.handledType = handledType;
            this.name = name;
            this.ownerType = ownerType;
            this.routingStrategy = routingStrategy;
        }

        public Type HandledType {
            get {
                return handledType;
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
    }
}
