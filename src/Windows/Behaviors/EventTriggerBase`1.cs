#if NETFRAMEWORK || WINDOWS
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents an abstract base class for triggers that are invoked in response to events.
    /// </summary>
    /// <typeparam name="T">The type of object to which the trigger is attached.</typeparam>
    public abstract class EventTriggerBase<T> : Behavior<T> where T : DependencyObject
    {
        private readonly Action<object?, object?> _onEvent;
        private Delegate? _subscribedEventHandler;

        protected EventTriggerBase()
        {
            _onEvent = OnEvent;
        }

        #region Dependency Properties

        /// <summary>
        /// Identifies the Event dependency property.
        /// </summary>
        public static readonly DependencyProperty EventProperty = DependencyProperty.Register(
            nameof(Event), typeof(RoutedEvent), typeof(EventTriggerBase<T>),
            new PropertyMetadata(null, (d, e) => ((EventTriggerBase<T>)d).OnEventChanged((RoutedEvent?)e.OldValue, (RoutedEvent?)e.NewValue)));

        /// <summary>
        /// Identifies the EventName dependency property.
        /// </summary>
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register(
            nameof(EventName), typeof(string), typeof(EventTriggerBase<T>), 
            new PropertyMetadata(nameof(FrameworkElement.Loaded), 
                (d, e) => ((EventTriggerBase<T>)d).OnEventNameChanged((string?)e.OldValue, (string?)e.NewValue)));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the routed event that activates this trigger.
        /// </summary>
        public RoutedEvent? Event
        {
            get => (RoutedEvent?)GetValue(EventProperty);
            set => SetValue(EventProperty, value);
        }

        /// <summary>
        /// Gets or sets the name of the event that activates this trigger.
        /// </summary>
        public string? EventName
        {
            get => (string?)GetValue(EventNameProperty);
            set => SetValue(EventNameProperty, value);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the event changes.
        /// </summary>
        /// <param name="oldEvent">The old routed event.</param>
        /// <param name="newEvent">The new routed event.</param>
        protected virtual void OnEventChanged(RoutedEvent? oldEvent, RoutedEvent? newEvent)
        {
            if (newEvent != null)
            {
                EventName = null;
            }
            if (AssociatedObject == null)
            {
                return;
            }
            UnregisterEvent(AssociatedObject, oldEvent);
            RegisterEvent(AssociatedObject, newEvent);
        }

        /// <summary>
        /// Called when the event name changes.
        /// </summary>
        /// <param name="oldEventName">The old event name.</param>
        /// <param name="newEventName">The new event name.</param>
        protected virtual void OnEventNameChanged(string? oldEventName, string? newEventName)
        {
            if (newEventName != null)
            {
                Event = null;
            }
            if (AssociatedObject == null)
            {
                return;
            }
            UnregisterEvent(AssociatedObject, oldEventName);
            RegisterEvent(AssociatedObject, newEventName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an event handler delegate.
        /// </summary>
        /// <param name="eventHandlerType">The type of the event handler.</param>
        /// <param name="parameters">The parameters of the event handler method.</param>
        /// <returns>A delegate representing the event handler.</returns>
        private Delegate CreateEventHandler(Type eventHandlerType, ParameterInfo[] parameters)
        {
            Type handlerType = typeof(EventTriggerHandler<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType);
            var handlerWrapper = Activator.CreateInstance(handlerType, _onEvent);
            Debug.Assert(handlerWrapper != null && handlerWrapper.GetType() == handlerType);
            return Delegate.CreateDelegate(eventHandlerType, handlerWrapper, handlerType.GetMethod(nameof(EventTriggerHandler<object, object>.Handler))!);
        }

        /// <summary>
        /// Determines whether the specified type is a valid event handler.
        /// A valid event handler is a delegate with an 'Invoke' method that returns void 
        /// and has exactly two parameters.
        /// </summary>
        /// <param name="eventHandlerType">The type of the event handler.</param>
        /// <param name="parameters">When this method returns, contains the parameters of the 'Invoke' method if the type is valid.</param>
        /// <returns>true if the type is a valid event handler; otherwise, false.</returns>
        private static bool IsValidEvent(Type eventHandlerType, [NotNullWhen(true)] out ParameterInfo[]? parameters)
        {
            MethodInfo? methodInfo;
            if (!typeof(Delegate).IsAssignableFrom(eventHandlerType) 
                || (methodInfo = eventHandlerType.GetMethod("Invoke")) == null 
                || methodInfo.ReturnType != typeof(void))
            {
                parameters = null;
                return false;
            }
            parameters = methodInfo.GetParameters();
            return parameters.Length == 2;
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            UnregisterEvent(AssociatedObject, Event);
            UnregisterEvent(AssociatedObject, EventName);
            RegisterEvent(AssociatedObject, Event);
            RegisterEvent(AssociatedObject, EventName);
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            UnregisterEvent(AssociatedObject, Event);
            UnregisterEvent(AssociatedObject, EventName);
            base.OnDetaching();
        }

        /// <summary>
        /// Called when the associated event is raised.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected abstract void OnEvent(object? sender, object? eventArgs);

        /// <summary>
        /// Registers the routed event on the specified object.
        /// </summary>
        /// <param name="obj">The object to register the routed event on.</param>
        /// <param name="event">The routed event to register.</param>
        private void RegisterEvent(T? obj, RoutedEvent? @event)
        {
            if (obj is not UIElement element || @event == null)
            {
                return;
            }
            Debug.Assert(_subscribedEventHandler == null);
            if (_subscribedEventHandler != null)
            {
                throw new InvalidOperationException($"Subscribed Event Handler is already registered on {@event}");
            }

            var eventHandlerType = @event.HandlerType;
            if (!IsValidEvent(eventHandlerType, out var parameters))
            {
                return;
            }

            _subscribedEventHandler = CreateEventHandler(@event.HandlerType, parameters);
            element.AddHandler(@event, _subscribedEventHandler);
        }

        /// <summary>
        /// Registers the event on the specified object.
        /// </summary>
        /// <param name="obj">The object to register the event on.</param>
        /// <param name="eventName">The name of the event.</param>
        private void RegisterEvent(T? obj, string? eventName)
        {
            if (obj == null || string.IsNullOrEmpty(eventName))
            {
                return;
            }
            Debug.Assert(_subscribedEventHandler == null);
            if (_subscribedEventHandler != null)
            {
                throw new InvalidOperationException($"Subscribed Event Handler is already registered on {eventName}");
            }

            var eventInfo = obj.GetType().GetEvent(eventName);
            Type? eventHandlerType;
            if (eventInfo == null || (eventHandlerType = eventInfo.EventHandlerType) == null || !IsValidEvent(eventHandlerType, out var parameters))
            {
                return;
            }

            _subscribedEventHandler = CreateEventHandler(eventHandlerType, parameters);
            eventInfo.AddEventHandler(obj, _subscribedEventHandler);
        }

        /// <summary>
        /// Unregisters the routed event from the specified object.
        /// </summary>
        /// <param name="obj">The object to unregister the routed event from.</param>
        /// <param name="event">The routed event to unregister.</param>
        private void UnregisterEvent(T? obj, RoutedEvent? @event)
        {
            if (obj is not UIElement element || @event == null || _subscribedEventHandler == null)
            {
                return;
            }

            element.RemoveHandler(@event, _subscribedEventHandler);
            _subscribedEventHandler = null;
        }

        /// <summary>
        /// Unregisters the event from the specified object.
        /// </summary>
        /// <param name="obj">The object to unregister the event from.</param>
        /// <param name="eventName">The name of the event.</param>
        private void UnregisterEvent(T? obj, string? eventName)
        {
            if (obj == null || string.IsNullOrEmpty(eventName) || _subscribedEventHandler == null)
            {
                return;
            }

            var eventInfo = obj.GetType().GetEvent(eventName);
            if (eventInfo == null)
            {
                return;
            }
            eventInfo.RemoveEventHandler(obj, _subscribedEventHandler);
            _subscribedEventHandler = null;
        }

        #endregion
    }

    /// <summary>
    /// A generic class used to handle events for EventTriggerBase.
    /// </summary>
    /// <typeparam name="TSender">The type of the first parameter of the event handler.</typeparam>
    /// <typeparam name="TEventArgs">The type of the second parameter of the event handler.</typeparam>
    internal class EventTriggerHandler<TSender, TEventArgs>
    {
        private readonly Action<object?, object?> _action;

        public EventTriggerHandler(Action<object?, object?> action)
        {
            _action = action;
        }

        public void Handler(TSender sender, TEventArgs e)
        {
            _action(sender, e);
        }
    }
}
#endif