using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ConsoleFramework.Events;

namespace TestApp
{
    /// <summary>
    /// Тип маршрутизации события.
    /// </summary>
    public enum RoutingStrategy
    {
        /// <summary>
        /// Событие передаётся всем подписчикам, от корневого элемента управления к источнику.
        /// </summary>
        Tunnel,
        /// <summary>
        /// Событие передаётся всем подписчикам, от источника до корневого элемента управления.
        /// </summary>
        Bubble,
        /// <summary>
        /// Событие будет передано только тем подписчикам, которые подписаны на
        /// источник события.
        /// </summary>
        Direct
    }

    /// <summary>
    /// Key for internal usage in routed event management maps.
    /// </summary>
    public sealed class RoutedEventKey : IEquatable<RoutedEventKey>
    {
        private readonly string name;
        private readonly Type ownerType;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Type OwnerType
        {
            get
            {
                return ownerType;
            }
        }

        public RoutedEventKey(string name, Type ownerType)
        {
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
        public bool Equals(RoutedEventKey other)
        {
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
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(RoutedEventKey)) return false;
            return Equals((RoutedEventKey)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0) * 397) ^ (ownerType != null ? ownerType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(RoutedEventKey left, RoutedEventKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RoutedEventKey left, RoutedEventKey right)
        {
            return !Equals(left, right);
        }
    }

    

    public class Program2
    {
        private struct MinMax
        {
            /// <summary>
            /// Определяет реальные констрейнты для текущих значений MinHeight/MaxHeight, MinWidth/MaxWidth
            /// и Width/Height. Min-значения не могут быть null, по дефолту равны нулю, также не могут быть int.MaxValue.
            /// Max-значения тоже не могут быть null, по дефолту равны int.MaxValue.
            /// Width и Height могут быть не заданы - в этом случае они контрол будет занимать как можно большее
            /// доступное пространство.
            /// В случае конфликта приоритет имеет Min-property, затем явно заданное значение (Width или Height),
            /// и в последнюю очередь играет роль Max-property.
            /// </summary>
            internal MinMax(int minHeight, int maxHeight, int minWidth, int maxWidth, int? width, int? height)
            {
                this.maxHeight = maxHeight;
                this.minHeight = minHeight;
                int? l = height;

                int tmp_height = l ?? int.MaxValue;
                this.maxHeight = Math.Max(Math.Min(tmp_height, this.maxHeight), this.minHeight);

                tmp_height = l ?? 0;
                this.minHeight = Math.Max(Math.Min(this.maxHeight, tmp_height), this.minHeight);

                this.maxWidth = maxWidth;
                this.minWidth = minWidth;
                l = width;

                int tmp_width = l ?? int.MaxValue;
                this.maxWidth = Math.Max(Math.Min(tmp_width, this.maxWidth), this.minWidth);

                tmp_width = l ?? 0;
                this.minWidth = Math.Max(Math.Min(this.maxWidth, tmp_width), this.minWidth);
            }

            internal readonly int minWidth;
            internal readonly int maxWidth;
            internal readonly int minHeight;
            internal readonly int maxHeight;
        }

        private static int? x;

        public static void Main( string[ ] args ) {
//            Console.WriteLine("Start");
//            object target = new object( );
//            EventManager2.AddHandler( target,
//                new RoutedEvent( typeof(string), "", typeof(string), RoutingStrategy.Bubble ),
//                new Action(( ) => {
//                    
//                }), false);
//            EventManager2.AddHandler(target,
//                new RoutedEvent(typeof(string), "", typeof(string), RoutingStrategy.Bubble),
//                new Action(() =>
//                {
//
//                }), false);

//                int? height = null;
//                int? l = height;
//                int tmp_height = l ?? int.MaxValue;
//                Console.WriteLine(tmp_height);
            MinMax mm = new MinMax(0, int.MaxValue, 0, int.MaxValue, x, x);
            Console.WriteLine("{0} {1} {2} {3}", mm.minWidth, mm.maxWidth, mm.minHeight, mm.maxHeight);
        }
    }

    /// <summary>
    /// Represents event that supports routing through visual tree.
    /// </summary>
    public sealed class RoutedEvent
    {
        private readonly Type handlerType;
        private readonly string name;
        private readonly Type ownerType;
        private readonly RoutingStrategy routingStrategy;

        public RoutedEvent(Type handlerType, string name, Type ownerType, RoutingStrategy routingStrategy)
        {
            this.handlerType = handlerType;
            this.name = name;
            this.ownerType = ownerType;
            this.routingStrategy = routingStrategy;
        }

        /// <summary>
        /// Тип делегата - обработчика события.
        /// </summary>
        public Type HandlerType
        {
            get
            {
                return handlerType;
            }
        }

        /// <summary>
        /// Имя события - должно быть уникальным в рамках указанного <see cref="OwnerType"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Тип владельца события.
        /// </summary>
        public Type OwnerType
        {
            get
            {
                return ownerType;
            }
        }

        /// <summary>
        /// Стратегия маршрутизации события.
        /// </summary>
        public RoutingStrategy RoutingStrategy
        {
            get
            {
                return routingStrategy;
            }
        }

        public RoutedEventKey Key
        {
            get
            {
                // note : mb cache this
                return new RoutedEventKey(name, ownerType);
            }
        }
    }

    public sealed class EventManager2
    {
        private class DelegateInfo
        {
            public readonly Delegate @delegate;
            public readonly bool handledEventsToo;

            public DelegateInfo(Delegate @delegate, bool handledEventsToo)
            {
                this.@delegate = @delegate;
                this.handledEventsToo = handledEventsToo;
            }
        }

        private class RoutedEventTargetInfo
        {
            public readonly object target;
            public List<DelegateInfo> handlersList;

            public RoutedEventTargetInfo(object target)
            {
                if (null == target)
                    throw new ArgumentNullException("target");
                this.target = target;
            }
        }

        private class RoutedEventInfo
        {
            public List<RoutedEventTargetInfo> targetsList;

            public RoutedEventInfo(RoutedEvent routedEvent)
            {
                if (null == routedEvent)
                    throw new ArgumentNullException("routedEvent");
            }
        }

        private static readonly Dictionary<RoutedEventKey, RoutedEventInfo> routedEvents = new Dictionary<RoutedEventKey, RoutedEventInfo>();
        
        public static void AddHandler(object target, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
        {
            if (null == target)
                throw new ArgumentNullException("target");
            if (null == routedEvent)
                throw new ArgumentNullException("routedEvent");
            if (null == handler)
                throw new ArgumentNullException("handler");
            //
            
            RoutedEventKey key = routedEvent.Key;
//            if (!routedEvents.ContainsKey(key))
//                throw new ArgumentException("Specified routed event is not registered.", "routedEvent");
//            RoutedEventInfo routedEventInfo = routedEvents[key];
            if (!routedEvents.ContainsKey( key ))
                routedEvents.Add( key, new RoutedEventInfo( routedEvent ) );
            RoutedEventInfo routedEventInfo = routedEvents[key];

            bool needAddTarget = true;
            if (routedEventInfo.targetsList != null)
            {
                RoutedEventTargetInfo targetInfo = routedEventInfo.targetsList.FirstOrDefault(info => info.target == target);
                if (null != targetInfo)
                {
                    Console.WriteLine("null != targetInfo");
                    if (targetInfo.handlersList == null)
                        targetInfo.handlersList = new List<DelegateInfo>();
                    targetInfo.handlersList.Add(new DelegateInfo(handler, handledEventsToo));
                    needAddTarget = false;
                }
            }
            if (needAddTarget)
            {
                RoutedEventTargetInfo targetInfo = new RoutedEventTargetInfo(target);
                targetInfo.handlersList = new List<DelegateInfo>();
                targetInfo.handlersList.Add(new DelegateInfo(handler, handledEventsToo));
                if (routedEventInfo.targetsList == null)
                    routedEventInfo.targetsList = new List<RoutedEventTargetInfo>();
                routedEventInfo.targetsList.Add(targetInfo);
            }
        }
    }
}
