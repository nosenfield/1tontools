using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OneTon.Logging;

namespace OneTon.Events
{
    /// <summary>
    /// A static event hub that supports strongly typed, executionOrder-based event subscription and publishing.
    /// Only accessible from the main Unity thread.
    /// </summary>
    public static class EventHub
    {
        private static readonly LogService logger = LogService.GetStatic(typeof(EventHub));
        private static readonly Dictionary<Type, SortedDictionary<int, List<Subscriber>>> subscribers = new();
        private static readonly int mainThreadId;

        private class Subscriber
        {
            public Delegate OriginalHandler;
            public Action<object> WrappedHandler;
        }

        // Initialize main thread ID at class load time
        static EventHub()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            logger.Debug($"EventHub initialized on thread {mainThreadId}");
        }

        // ---- Subscribe Overloads ----

        /// <summary>
        /// Subscribes a handler to an event of type <typeparamref name="TEvent"/> with optional executionOrder.
        /// The handler receives the event instance.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to subscribe to.</typeparam>
        /// <param name="handler">The handler that will be invoked when the event is published.</param>
        /// <param name="executionOrder">The executionOrder of the handler. Lower priorities are called first.</param>
        public static void Subscribe<TEvent>(Action<TEvent> handler, int executionOrder = 0)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Action<object> wrapped = obj => handler((TEvent)obj);
            AddSubscriber(typeof(TEvent), handler, wrapped, executionOrder);
        }

        /// <summary>
        /// Subscribes a parameterless handler to an event of type <typeparamref name="TEvent"/>.
        /// Use this for signal-style events that carry no data.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to subscribe to.</typeparam>
        /// <param name="handler">The parameterless handler to invoke when the event is published.</param>
        /// <param name="executionOrder">The executionOrder of the handler. Lower priorities are called first.</param>
        public static void Subscribe<TEvent>(Action handler, int executionOrder = 0)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Action<object> wrapped = _ => handler();
            AddSubscriber(typeof(TEvent), handler, wrapped, executionOrder);
        }

        // ---- Unsubscribe Overloads ----

        /// <summary>
        /// Unsubscribes a typed handler from the event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to unsubscribe from.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        /// <param name="executionOrder">The executionOrder the handler was registered with.</param>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler, int executionOrder = 0)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            RemoveSubscriber(typeof(TEvent), handler, executionOrder);
        }

        /// <summary>
        /// Unsubscribes a parameterless handler from the event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to unsubscribe from.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        /// <param name="executionOrder">The executionOrder the handler was registered with.</param>
        public static void Unsubscribe<TEvent>(Action handler, int executionOrder = 0)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            RemoveSubscriber(typeof(TEvent), handler, executionOrder);
        }

        // ---- Publish Methods ----

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/> with the given instance.
        /// All subscribers will be invoked in order of executionOrder and subscription.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        /// <param name="ev">The event instance to pass to handlers.</param>
        public static void Publish<TEvent>(TEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            Type eventType = typeof(TEvent);
            if (!subscribers.TryGetValue(eventType, out SortedDictionary<int, List<Subscriber>> executionOrderMap)) return;

            if (Thread.CurrentThread.ManagedThreadId != mainThreadId)
            {
                logger.Warn($"EventHub: Event {eventType.Name} published from non-main thread (ID {Thread.CurrentThread.ManagedThreadId}). This may cause thread safety issues.");
            }

            foreach (KeyValuePair<int, List<Subscriber>> kvp in executionOrderMap)
            {
                List<Subscriber> list = kvp.Value;
                foreach (Subscriber sub in list)
                {
                    try
                    {
                        sub.WrappedHandler(ev);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Exception in event handler for {eventType}: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Publishes a signal-style event of type <typeparamref name="TEvent"/> that carries no data.
        /// The type must have a parameterless constructor.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        public static void Publish<TEvent>() where TEvent : new()
        {
            Publish(new TEvent());
        }

        // ---- Internal Helpers ----

        private static void AddSubscriber(Type eventType, Delegate original, Action<object> wrapped, int executionOrder)
        {
            if (!subscribers.TryGetValue(eventType, out SortedDictionary<int, List<Subscriber>> executionOrderMap))
            {
                executionOrderMap = new SortedDictionary<int, List<Subscriber>>();
                subscribers[eventType] = executionOrderMap;
            }

            if (!executionOrderMap.TryGetValue(executionOrder, out List<Subscriber> list))
            {
                list = new List<Subscriber>();
                executionOrderMap[executionOrder] = list;
            }

            foreach (Subscriber existing in list)
            {
                if (existing.OriginalHandler.Equals(original))
                {
                    logger.Warn($"Attempted to subscribe duplicate handler to {eventType.Name} @ executionOrder {executionOrder}");
                    return;
                }
            }


            list.Add(new Subscriber
            {
                OriginalHandler = original,
                WrappedHandler = wrapped
            });

            logger.Debug($"Subscribed to {eventType.Name} @ executionOrder {executionOrder}");
        }

        private static void RemoveSubscriber(Type eventType, Delegate original, int executionOrder)
        {
            if (!subscribers.TryGetValue(eventType, out SortedDictionary<int, List<Subscriber>> executionOrderMap))
            {
                logger.Warn($"Tried to unsubscribe from {eventType.Name}, but no such event is registered");
                return;
            }

            if (!executionOrderMap.TryGetValue(executionOrder, out List<Subscriber> list))
            {
                logger.Warn($"Tried to unsubscribe from {eventType.Name} with executionOrder {executionOrder}, but no such tier exists");
                return;
            }

            int removed = list.RemoveAll(sub => sub.OriginalHandler.Equals(original));
            if (removed == 0)
            {
                logger.Warn($"Tried to unsubscribe from {eventType.Name}, but no matching handler was found at executionOrder {executionOrder}");
            }
            else
            {
                logger.Debug($"Unsubscribed from {eventType.Name} @ executionOrder {executionOrder}");
            }

            // Clean up empty list
            if (list.Count == 0)
                executionOrderMap.Remove(executionOrder);

            // Clean up entire event type if no priorities remain
            if (executionOrderMap.Count == 0)
                subscribers.Remove(eventType);
        }
    }
}