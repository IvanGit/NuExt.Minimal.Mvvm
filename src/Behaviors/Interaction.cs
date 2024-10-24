#if NETFRAMEWORK || WINDOWS
using System.Windows;

namespace Minimal.Mvvm.UI
{
    /// <summary>
    /// Provides attached properties and methods for managing <see cref="BehaviorCollection"/> on WPF elements.
    /// </summary>
    public static class Interaction
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the Behaviors attached dependency property.
        /// </summary>
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            "InteractionBehaviors", typeof(BehaviorCollection), typeof(Interaction), 
            new FrameworkPropertyMetadata(null, OnBehaviorsChanged));

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when the value of the Behaviors attached property changes.
        /// </summary>
        /// <param name="d">The dependency object where the property change occurred.</param>
        /// <param name="e">Event data that contains information about which property changed and its old and new values.</param>
        private static void OnBehaviorsChanged(DependencyObject? d, DependencyPropertyChangedEventArgs e)
        {
            var oldBehaviors = (BehaviorCollection?)e.OldValue;
            var newBehaviors = (BehaviorCollection?)e.NewValue;

            if (oldBehaviors == newBehaviors)
            {
                return;
            }
            if (oldBehaviors?.AssociatedObject != null)
            {
                oldBehaviors.Detach();
            }
            if (newBehaviors != null && d != null)
            {
                newBehaviors.Attach(d);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the <see cref="BehaviorCollection"/> associated with the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object from which to get the behaviors.</param>
        /// <returns>The <see cref="BehaviorCollection"/> associated with the specified object.</returns>
        public static BehaviorCollection GetBehaviors(DependencyObject obj)
        {
            var behaviors = (BehaviorCollection?)obj.GetValue(BehaviorsProperty);
            if (behaviors == null)
            {
                behaviors = new BehaviorCollection();
                obj.SetValue(BehaviorsProperty, behaviors);
            }
            return behaviors;
        }

        #endregion
    }
}
#endif
