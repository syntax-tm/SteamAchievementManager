using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SAM.Core;

public class ObservableCollectionPropertyHandler<T, TU> : ObservableCollectionHandler<T, TU>
	where T : class, INotifyCollectionChanged, ICollection<TU>
	where TU : class, INotifyPropertyChanged
{
	private readonly Dictionary<string, Action> _genericHandlers = [];
	private readonly Dictionary<string, Action<T, TU>> _handlers = [];
	private readonly WeakReference<T> _source;
	private readonly List<WeakReference<T>> _sourceList = [];

	public ObservableCollectionPropertyHandler ([NotNull] T source) : base(source)
	{
		ArgumentNullException.ThrowIfNull(source);

		_source = new(source);

		SetAddItem(OnAddItem);
		SetRemoveItem(OnRemoveItem);
		SetReset(OnReset);
	}

	public ObservableCollectionPropertyHandler<T, TU> Add ([NotNull] Expression<Func<TU, object>> expression,
		[NotNull] Action handler)
	{
		var source = GetSource();
		if (source == null)
			throw new InvalidOperationException($"The {nameof(source)} has been garbage collected.");

		var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

		_handlers [propertyName] = (_, _) => handler();

		foreach (var item in source)
		{
			PropertyChangedEventManager.AddListener(item, this, propertyName);
		}

		return this;
	}

	public ObservableCollectionPropertyHandler<T, TU> Add ([NotNull] Expression<Func<TU, object>> expression,
		[NotNull] Action<T, TU> handler)
	{
		var source = GetSource();
		if (source == null)
			throw new InvalidOperationException($"The {nameof(source)} has been garbage collected.");

		var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

		_handlers [propertyName] = handler;

		foreach (var item in source)
		{
			PropertyChangedEventManager.AddListener(item, this, propertyName);
		}

		return this;
	}

	public ObservableCollectionPropertyHandler<T, TU> AddAndInvoke ([NotNull] Expression<Func<TU, object>> expression,
																   [NotNull] Action handler)
	{
		var source = GetSource();
		if (source == null)
			throw new InvalidOperationException($"The {nameof(source)} has been garbage collected.");

		var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

		_handlers [propertyName] = (_, _) => handler();

		foreach (var item in source)
		{
			PropertyChangedEventManager.AddListener(item, this, propertyName);
		}

		handler();

		return this;
	}

	protected override bool OnCollectionReceiveWeakEvent ([NotNull] Type managerType, [CanBeNull] object sender, [CanBeNull] EventArgs e)
	{
		if (base.OnCollectionReceiveWeakEvent(managerType, sender, e))
			return true;
		if (managerType != typeof(PropertyChangedEventManager))
			return false;

		var source = GetSource();
		if (source == null)
		{
			var sourceNullMessage = @"Confused, received a PropertyChanged event from a source that has been garbage collected.";
			throw new InvalidOperationException(sourceNullMessage);
		}

		var item = sender as TU;
		var propertyName = ((PropertyChangedEventArgs) e)?.PropertyName;

		Debug.Assert(propertyName != null, nameof(propertyName) + " != null");

		if (_handlers.TryGetValue(propertyName, out var handler))
		{
			handler?.Invoke(source, item);
		}

		return true;
	}

	private void OnReset ([NotNull] T collection)
	{

	}

	private void OnRemoveItem ([NotNull] T collection, [NotNull] TU item)
	{
		foreach (var handler in _handlers)
		{
			PropertyChangedEventManager.RemoveListener(item, this, handler.Key);
		}
	}

	private void OnAddItem ([NotNull] T collection, [NotNull] TU item)
	{
		foreach (var handler in _handlers)
		{
			PropertyChangedEventManager.AddListener(item, this, handler.Key);
		}
	}

}
