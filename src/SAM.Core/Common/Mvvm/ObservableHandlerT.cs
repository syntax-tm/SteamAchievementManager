using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using JetBrains.Annotations;

namespace SAM.Core;

// ReSharper disable InconsistentNaming
public class ObservableHandler<T> : IWeakEventListener
	where T : class, INotifyPropertyChanged
{
	private readonly WeakReference<T> _source;
	private readonly Dictionary<string, Action> _handlers = [];
	private readonly Dictionary<string, Action<T>> _handlersT = [];

	public ObservableHandler ([NotNull] T source)
	{
		ArgumentNullException.ThrowIfNull(source);

		_source = new(source);
	}

	[NotNull]
	public ObservableHandler<T> Add ([NotNull] Expression<Func<T, object>> expression, [NotNull] Action handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		var source = GetSource() ?? throw new InvalidOperationException("Source has been garbage collected.");

		var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

		_handlers [propertyName] = handler;
		PropertyChangedEventManager.AddListener(source, this, propertyName);

		return this;
	}

	[NotNull]
	public ObservableHandler<T> Add ([NotNull] Expression<Func<T, object>> expression, [NotNull] Action<T> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		var source = GetSource() ?? throw new InvalidOperationException("Source has been garbage collected.");

		var propertyName = ReflectionHelper.GetPropertyNameFromLambda(expression);

		_handlersT [propertyName] = handler;
		PropertyChangedEventManager.AddListener(source, this, propertyName);

		return this;
	}

	[NotNull]
	public ObservableHandler<T> AddAndInvoke ([NotNull] Expression<Func<T, object>> expression, [NotNull] Action handler)
	{
		Add(expression, handler);
		handler();
		return this;
	}

	[NotNull]
	public ObservableHandler<T> AddAndInvoke ([NotNull] Expression<Func<T, object>> expression, [NotNull] Action<T> handler)
	{
		Add(expression, handler);
		handler(GetSource());
		return this;
	}

	private T GetSource ()
	{
		return _source.TryGetTarget(out var source)
			? source
			: throw new InvalidOperationException($"{nameof(source)} has been garbage collected.");
	}

	bool IWeakEventListener.ReceiveWeakEvent (Type managerType, object sender, EventArgs e)
	{
		return OnReceiveWeakEvent(managerType, sender, e);
	}

	public virtual bool OnReceiveWeakEvent (Type managerType, object sender, EventArgs e)
	{
		if (managerType != typeof(PropertyChangedEventManager))
		{
			return false;
		}

		var propertyName = ((PropertyChangedEventArgs) e).PropertyName;
		Notify(propertyName);

		return true;
	}

	protected void Notify (string propertyName)
	{
		var source = GetSource() ?? throw new InvalidOperationException("Source has been garbage collected.");

		if (string.IsNullOrEmpty(propertyName))
		{
			foreach (var handler in _handlers.Values)
			{
				handler();
			}

			foreach (var handler in _handlersT.Values)
			{
				handler(source);
			}
		}
		else
		{
			if (_handlers.TryGetValue(propertyName, out var handler))
			{
				handler();
			}

			if (_handlersT.TryGetValue(propertyName, out var handlerT))
			{
				handlerT(source);
			}
		}
	}
}
