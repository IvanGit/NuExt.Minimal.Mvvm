#if NETFRAMEWORK || WINDOWS
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a collection of <see cref="Behavior"/> objects that can be attached to a WPF element.
    /// </summary>
    public sealed class BehaviorCollection : FreezableCollection<Behavior>
    {
        private readonly List<Behavior> _snapshot = [];
        private DependencyObject? _associatedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorCollection"/> class.
        /// </summary>
        internal BehaviorCollection()
        {
            if (!(bool)GetValue(DesignerProperties.IsInDesignModeProperty))
            {
                ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
            }
        }

        #region Properties

        /// <summary>
        /// Gets the object to which this behavior collection is attached.
        /// </summary>
        public DependencyObject? AssociatedObject
        {
            get
            {
                ReadPreamble();
                return _associatedObject;
            }
            private set
            {
                WritePreamble();
                _associatedObject = value;
                WritePostscript();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles changes in the collection to attach or detach behaviors as necessary.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ClearItems();
                    foreach (Behavior item in this)
                    {
                        AddItem(item);
                    }
                    return;
                case NotifyCollectionChangedAction.Move:
                    return;
            }

            foreach (Behavior item in e.OldItems ?? Array.Empty<Behavior>())
            {
                RemoveItem(item);
            }
            foreach (Behavior item in e.NewItems ?? Array.Empty<Behavior>())
            {
                AddItem(item);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attaches the behavior collection to the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to attach to.</param>
        public void Attach(DependencyObject? obj)
        {
            if (obj == AssociatedObject)
            {
                return;
            }
            ThrowInvalidOperationExceptionIfAttached();
            if ((bool)GetValue(DesignerProperties.IsInDesignModeProperty))
            {
                return;
            }
            AssociatedObject = obj;
            foreach (Behavior item in this)
            {
                item.Attach(AssociatedObject);
            }
        }

        /// <summary>
        /// Adds a behavior to the snapshot and attaches it if the collection is already attached.
        /// </summary>
        /// <param name="item">The behavior to add.</param>
        private void AddItem(Behavior item)
        {
            if (_snapshot.Contains(item))
            {
                return;
            }
            ItemAdded(item);
            _snapshot.Insert(IndexOf(item), item);
        }

        /// <summary>
        /// Clears all behaviors from the snapshot and detaches them.
        /// </summary>
        private void ClearItems()
        {
            foreach (Behavior item in _snapshot)
            {
                ItemRemoved(item);
            }
            _snapshot.Clear();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BehaviorCollection"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="BehaviorCollection"/>.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BehaviorCollection();
        }

        /// <summary>
        /// Detaches the behavior collection from the associated <see cref="DependencyObject"/>.
        /// </summary>
        public void Detach()
        {
            foreach (Behavior item in this)
            {
                item.Detach();
            }
            AssociatedObject = null;
        }

        /// <summary>
        /// Attaches the behavior to the associated object if one exists.
        /// </summary>
        /// <param name="item">The behavior to attach.</param>
        private void ItemAdded(Behavior item)
        {
            if (AssociatedObject != null)
            {
                item.Attach(AssociatedObject);
            }
        }

        /// <summary>
        /// Detaches the behavior from its associated object.
        /// </summary>
        /// <param name="item">The behavior to detach.</param>
        private static void ItemRemoved(Behavior item)
        {
            if (item.AssociatedObject != null)
            {
                item.Detach();
            }
        }

        /// <summary>
        /// Removes the behavior from the snapshot and detaches it.
        /// </summary>
        /// <param name="item">The behavior to remove.</param>
        private void RemoveItem(Behavior item)
        {
            ItemRemoved(item);
            _snapshot.Remove(item);
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the collection is already attached to an object.
        /// </summary>
        private void ThrowInvalidOperationExceptionIfAttached()
        {
            if (AssociatedObject != null)
            {
                throw new InvalidOperationException("Cannot set the same BehaviorCollection on multiple objects.");
            }
        }

        #endregion
    }
}
#endif