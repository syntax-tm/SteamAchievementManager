using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;

namespace SAM.WPF.Core
{
    public class ObservableCollectionHandler<T, TU> : IWeakEventListener
        where T : class, INotifyCollectionChanged, ICollection<TU>
    {
        private readonly WeakReference m_source;
        private Action<T, TU> m_addItemHandler;
        private bool m_isRegistered;
        private Action<T, TU> m_removeItemHandler;
        private Action<T> m_resetHandler;

        public ObservableCollectionHandler([NotNull] T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            m_source = new WeakReference(source);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return OnCollectionReceiveWeakEvent(managerType, sender, e);
        }

        [NotNull]
        public ObservableCollectionHandler<T, TU> SetAddItem([NotNull] Action<T, TU> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            RegisterEventHandler();
            m_addItemHandler = handler;

            return this;
        }

        [NotNull]
        public ObservableCollectionHandler<T, TU> SetAddItemAndInvoke([NotNull] Action<T, TU> handler)
        {
            SetAddItem(handler);

            var source = GetSource();

            if (source == null)
                throw new InvalidOperationException("Source has been garbage collected.");

            foreach (var item in source)
            {
                handler(GetSource(), item);
            }

            return this;
        }

        [NotNull]
        public ObservableCollectionHandler<T, TU> SetRemoveItem([NotNull] Action<T, TU> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            RegisterEventHandler();
            m_removeItemHandler = handler;

            return this;
        }

        [NotNull]
        public ObservableCollectionHandler<T, TU> SetReset([NotNull] Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            RegisterEventHandler();
            m_resetHandler = handler;

            return this;
        }

        [CanBeNull]
        private T GetSource()
        {
            var source = m_source.Target as T;

            return source;
        }

        private void RegisterEventHandler()
        {
            if (m_isRegistered)
                return;

            var source = GetSource();

            if (source == null)
                throw new InvalidOperationException("Source has been garbage collected.");

            CollectionChangedEventManager.AddListener(source, this);
            m_isRegistered = true;
        }

        public virtual bool OnCollectionReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;

            if (m_addItemHandler == null)
                return false;

            var source = GetSource();

            if (source == null)
                throw new InvalidOperationException("Confused, received a CollectionChanged event from a source that has been garbage collected.");

            var actualEventArgs = (NotifyCollectionChangedEventArgs) e;

            RaiseAddItem(actualEventArgs, source);
            RaiseRemoveItem(actualEventArgs, source);
            RaiseReset(actualEventArgs, source);

            return true;
        }

        private void RaiseReset(NotifyCollectionChangedEventArgs e, T source)
        {
            if (m_resetHandler == null)
                return;

            if (e.Action != NotifyCollectionChangedAction.Reset)
                return;

            m_resetHandler(source);
        }

        private void RaiseRemoveItem(NotifyCollectionChangedEventArgs e, T source)
        {
            if (m_removeItemHandler == null)
                return;

            if (e.OldItems == null)
                return;

            foreach (var item in e.OldItems.Cast<TU>())
            {
                m_removeItemHandler(source, item);
            }
        }

        private void RaiseAddItem(NotifyCollectionChangedEventArgs e, T source)
        {
            if (m_addItemHandler == null)
                return;

            if (e.NewItems == null)
                return;

            foreach (var item in e.NewItems.Cast<TU>())
            {
                m_addItemHandler(source, item);
            }
        }
    }
}
