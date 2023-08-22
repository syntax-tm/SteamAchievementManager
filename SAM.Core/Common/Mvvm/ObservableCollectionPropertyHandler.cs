using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SAM.Core
{
    public class ObservableCollectionPropertyHandler<T, TU> : ObservableCollectionHandler<T, TU>
        where T : class, INotifyCollectionChanged, ICollection<TU>
        where TU : class, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Action> _genericHandlers = new ();
        private readonly Dictionary<string, Action<T, TU>> _handlers = new ();
        private readonly WeakReference<T> _source;
        private readonly List<WeakReference<T>> _sourceList = new ();

        public ObservableCollectionPropertyHandler([NotNull] T source) : base(source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = new (source);

            SetAddItem(OnAddItem);
            SetRemoveItem(OnRemoveItem);
            SetReset(OnReset);
        }

        private void OnReset([NotNull] T collection)
        {
        }

        private void OnRemoveItem([NotNull] T collection, [NotNull] TU item)
        {
            foreach (var handler in _handlers)
            {
                PropertyChangedEventManager.RemoveListener(item, this, handler.Key);
            }
        }

        private void OnAddItem([NotNull] T collection, [NotNull] TU item)
        {
            foreach (var handler in _handlers)
            {
                PropertyChangedEventManager.AddListener(item, this, handler.Key);
            }
        }

        public ObservableCollectionPropertyHandler<T, TU> Add([NotNull] Expression<Func<TU, object>> expression,
            [NotNull] Action<T, TU> handler)
        {
            var source = GetSource();
            if (source == null) throw new InvalidOperationException($"The {nameof(source)} has been garbage collected.");

            var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

            if (!_handlers.ContainsKey(propertyName))
            {
                _handlers.Add(propertyName, handler);
            }
            else
            {
                _handlers[propertyName] = handler;
            }

            foreach (var item in source)
            {
                PropertyChangedEventManager.AddListener(item, this, propertyName);
            }

            return this;
        }
        
        public ObservableCollectionPropertyHandler<T, TU> AddAndInvoke([NotNull] Expression<Func<TU, object>> expression,
                                                              [NotNull] Action<T, TU> handler)
        {
            var source = GetSource();
            if (source == null) throw new InvalidOperationException($"The {nameof(source)} has been garbage collected.");

            var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

            if (!_handlers.ContainsKey(propertyName))
            {
                _handlers.Add(propertyName, handler);
            }
            else
            {
                _handlers[propertyName] = handler;
            }

            foreach (var item in source)
            {
                PropertyChangedEventManager.AddListener(item, this, propertyName);
            }

            return this;
        }

        private T GetSource()
        {
            var hasSource = _source.TryGetTarget(out var sourceObj);
            if (!hasSource) throw new InvalidOperationException($"Failed to get the instance of the {nameof(_source)} object from the stored {nameof(WeakReference)}.");
            return sourceObj;
        }

        public override bool OnCollectionReceiveWeakEvent([NotNull] Type managerType, [CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (base.OnCollectionReceiveWeakEvent(managerType, sender, e)) return true;
            if (managerType != typeof(PropertyChangedEventManager)) return false;

            var source = GetSource();
            if (source == null)
            {
                var sourceNullMessage = @"Confused, received a PropertyChanged event from a source that has been garbage collected.";
                throw new InvalidOperationException(sourceNullMessage);
            }

            var item = sender as TU;
            var propertyName = ((PropertyChangedEventArgs) e)?.PropertyName;

            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var propHandler in _handlers.Values)
                {
                    propHandler?.Invoke(source, item);
                }
                return true;
            }

            if (_handlers.TryGetValue(propertyName, out var handler))
            {
                handler?.Invoke(source, item);
            }

            return true;
        }
    }
}
