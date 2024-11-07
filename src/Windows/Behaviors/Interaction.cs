#if NETFRAMEWORK || WINDOWS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
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
        private static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            "InteractionBehaviors", typeof(BehaviorCollection), typeof(Interaction), 
            new FrameworkPropertyMetadata(null, OnBehaviorsChanged));

        /// <summary>
        /// Identifies the BehaviorsTemplate attached dependency property.
        /// </summary>
        public static readonly DependencyProperty BehaviorsTemplateProperty = DependencyProperty.RegisterAttached(
            "BehaviorsTemplate", typeof(DataTemplate), typeof(Interaction), 
            new FrameworkPropertyMetadata(null, OnBehaviorsTemplateChanged));

        /// <summary>
        /// Identifies the BehaviorsTemplateSnapshot attached dependency property.
        /// </summary>
        private static readonly DependencyProperty BehaviorsTemplateSnapshotProperty = DependencyProperty.RegisterAttached(
            "BehaviorsTemplateSnapshot", typeof(List<Behavior>), typeof(Interaction));

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when the value of the Behaviors attached property changes.
        /// </summary>
        /// <param name="obj">The dependency object where the property change occurred.</param>
        /// <param name="e">Event data that contains information about which property changed and its old and new values.</param>
        private static void OnBehaviorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(obj != null);
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
            newBehaviors?.Attach(obj);
        }

        private static void OnBehaviorsTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = GetBehaviors(obj);
            if (obj.GetValue(BehaviorsTemplateSnapshotProperty) is List<Behavior> oldItems)
            {
                foreach (Behavior behavior in oldItems)
                {
                    behaviors.Remove(behavior);
                }
            }

            if (e.NewValue is not DataTemplate newDataTemplate)
            {
                obj.SetValue(BehaviorsTemplateSnapshotProperty, null);
                return;
            }

            if (!newDataTemplate.IsSealed)
            {
                newDataTemplate.Seal();
            }

            List<Behavior> newItems;
            var content = newDataTemplate.LoadContent();

            switch (content)
            {
                case ContentControl contentControl:
                {
                    newItems = new List<Behavior>();
                    if (contentControl.Content is Behavior behavior)
                    {
                        newItems.Add(behavior);
                    }
                    contentControl.Content = null;
                    break;
                }
                case ItemsControl itemsControl:
                    newItems = new List<Behavior>();
                    foreach (object item in itemsControl.Items)
                    {
                        if (item is not Behavior behavior)
                        {
                            continue;
                        }
                        newItems.Add(behavior);
                    }
                    itemsControl.Items.Clear();
                    itemsControl.ItemsSource = null;
                    break;
                default:
                    throw new InvalidOperationException("Use ContentControl or ItemsControl in the template to specify Behaviors.");
            }

            if (newItems.Count > 0)
            {
                obj.SetValue(BehaviorsTemplateSnapshotProperty, newItems);
                foreach (Behavior behavior in newItems)
                {
                    behaviors.Add(behavior);
                }
            }
            else
            {
                obj.SetValue(BehaviorsTemplateSnapshotProperty, null);
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

        public static DataTemplate? GetBehaviorsTemplate(DependencyObject obj)
        {
            return (DataTemplate?)obj.GetValue(BehaviorsTemplateProperty);
        }

        public static void SetBehaviorsTemplate(DependencyObject obj, DataTemplate? template)
        {
            obj.SetValue(BehaviorsTemplateProperty, template);
        }

        #endregion
    }
}
#endif
