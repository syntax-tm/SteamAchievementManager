using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using JetBrains.Annotations;

namespace SAM.WPF.Core
{
    public class ObservableHandler<T> : IWeakEventListener
        where T : class, INotifyPropertyChanged
    {
        private readonly WeakReference<T> m_source;
        private readonly Dictionary<string, Action> m_handlers = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action<T>> m_handlersT = new Dictionary<string, Action<T>>();

        public ObservableHandler([NotNull] T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            m_source = new WeakReference<T>(source);
        }
        
        [NotNull]
        public ObservableHandler<T> Add([NotNull] Expression<Func<T, object>> expression, [NotNull] Action handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var source = GetSource();
            if (source == null)
            {
                throw new InvalidOperationException("Source has been garbage collected.");
            }

            var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

            m_handlers[propertyName] = handler;
            PropertyChangedEventManager.AddListener(source, this, propertyName);

            return this;
        }

        [NotNull]
        public ObservableHandler<T> Add([NotNull] Expression<Func<T, object>> expression, [NotNull] Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var source = GetSource();
            if (source == null)
            {
                throw new InvalidOperationException("Source has been garbage collected.");
            }

            var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

            m_handlersT[propertyName] = handler;
            PropertyChangedEventManager.AddListener(source, this, propertyName);

            return this;
        }

        [NotNull]
        public ObservableHandler<T> AddAndInvoke([NotNull] Expression<Func<T, object>> expression, [NotNull] Action<T> handler)
        {
            Add(expression, handler);
            handler(GetSource());
            return this;
        }

        private T GetSource()
        {
            if (m_source.TryGetTarget(out var source)) return source;

            throw new InvalidOperationException($"{nameof(source)} has been garbage collected.");
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return OnReceiveWeakEvent(managerType, sender, e);
        }

        public virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
            {
                return false;
            }

            var propertyName = ((PropertyChangedEventArgs)e).PropertyName;
            Notify(propertyName);

            return true;
        }

        protected void Notify(string propertyName)
        {
            var source = GetSource();
            if (source == null)
            {
                throw new InvalidOperationException("Confused, received a PropertyChanged event from a source that has been garbage collected.");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var handler in m_handlers.Values)
                {
                    handler();
                }
                foreach (var handler in m_handlersT.Values)
                {
                    handler(source);
                }
            }
            else
            {
                if (m_handlers.TryGetValue(propertyName, out var handler))
                    handler();

                if (m_handlersT.TryGetValue(propertyName, out var handlerT))
                    handlerT(source);
            }
        }
    }
}
