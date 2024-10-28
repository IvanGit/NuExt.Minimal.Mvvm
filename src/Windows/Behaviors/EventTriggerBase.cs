#if NETFRAMEWORK || WINDOWS
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Minimal.Mvvm.UI
{
    /// <summary>
    /// Represents an abstract base class for triggers that are invoked in response to events.
    /// </summary>
    /// <typeparam name="T">The type of object to which the trigger is attached.</typeparam>
    public abstract class EventTriggerBase<T> : Behavior<T> where T : DependencyObject
    {
        private Delegate? _subscribedEventHandler;

        #region Dependency Properties

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
        /// Called when the event name changes.
        /// </summary>
        /// <param name="oldEventName">The old event name.</param>
        /// <param name="newEventName">The new event name.</param>
        protected virtual void OnEventNameChanged(string? oldEventName, string? newEventName)
        {
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
        /// Gets the method info for the OnEvent method.
        /// </summary>
        /// <returns>The MethodInfo for the OnEvent method.</returns>
        private MethodInfo GetOnEventMethodInfo()
        {
            return GetType().GetMethod(nameof(OnEvent), BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        /// <summary>
        /// Determines whether the specified type is a valid event handler.
        /// </summary>
        /// <param name="eventHandlerType">The type of the event handler.</param>
        /// <returns>true if the type is a valid event handler; otherwise, false.</returns>
        private static bool IsValidEvent(Type eventHandlerType)
        {
            if (!typeof(Delegate).IsAssignableFrom(eventHandlerType))
            {
                return false;
            }
            var parameters = eventHandlerType.GetMethod("Invoke")?.GetParameters();
            return parameters?.Length == 2 && typeof(object).IsAssignableFrom(parameters[0].ParameterType) && typeof(object).IsAssignableFrom(parameters[1].ParameterType);
        }

        /// <summary>
        /// Called when the behavior is attached to an element.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            UnregisterEvent(AssociatedObject, EventName);
            RegisterEvent(AssociatedObject, EventName);
        }

        /// <summary>
        /// Called when the behavior is detached from an element.
        /// </summary>
        protected override void OnDetaching()
        {
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
        /// Registers the event on the specified object.
        /// </summary>
        /// <param name="obj">The object to register the event on.</param>
        /// <param name="eventName">The name of the event.</param>
        private void RegisterEvent(DependencyObject? obj, string? eventName)
        {
            Debug.Assert(_subscribedEventHandler == null);
            if (obj == null || string.IsNullOrEmpty(eventName))
            {
                return;
            }
            var eventInfo = obj.GetType().GetEvent(eventName);
            Type? eventHandlerType;
            if (eventInfo == null || (eventHandlerType = eventInfo.EventHandlerType) == null || !IsValidEvent(eventHandlerType))
            {
                return;
            }
            if (_subscribedEventHandler != null)
            {
                throw new InvalidOperationException();
            }
            _subscribedEventHandler = Delegate.CreateDelegate(eventHandlerType, this, GetOnEventMethodInfo());
            eventInfo.AddEventHandler(obj, _subscribedEventHandler);
        }

        /// <summary>
        /// Unregisters the event from the specified object.
        /// </summary>
        /// <param name="obj">The object to unregister the event from.</param>
        /// <param name="eventName">The name of the event.</param>
        private void UnregisterEvent(DependencyObject? obj, string? eventName)
        {
            if (_subscribedEventHandler == null)
            {
                return;
            }
            if (obj == null || string.IsNullOrEmpty(eventName))
            {
                return;
            }
            var eventInfo = obj.GetType().GetEvent(eventName);
            Type? eventHandlerType;
            if (eventInfo == null || (eventHandlerType = eventInfo.EventHandlerType) == null || !IsValidEvent(eventHandlerType))
            {
                return;
            }
            eventInfo.RemoveEventHandler(obj, _subscribedEventHandler);
            _subscribedEventHandler = null;
        }

        #endregion
    }
}
#endif