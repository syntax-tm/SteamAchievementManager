using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core.Storage;

// TODO: Add configurable cache expiration
[DebuggerDisplay("{GetFullPath()}")]
public class CacheKey : ICacheKey
{
	private const string DEFAULT_EXTENSION = ".json";

	protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.ReflectedType ?? typeof(CacheKey));

	private string _fullPath;

	public string Key
	{
		get; protected set;
	}
	public string FilePath
	{
		get; protected set;
	}

	protected CacheKey ()
	{

	}

	public CacheKey (object key, CacheKeyType type)
	{
		ArgumentNullException.ThrowIfNull(key);

		SetKey(key);

		if (type == CacheKeyType.App)
		{
			throw new NotSupportedException(@$"{CacheKeyType.App} cache keys require an additional id parameter.");
		}

		FilePath = type.GetDescription();
	}

	public CacheKey (string fileName, object id, CacheKeyType type = CacheKeyType.Default, CacheKeySubType subType = CacheKeySubType.None)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException(nameof(fileName));
		}

		SetKey(fileName);

		if (type != CacheKeyType.App)
		{
			throw new NotSupportedException(@$"Only {CacheKeyType.App} cache keys support {nameof(id)}.");
		}

		var path = new List<object>
		{
			type.GetDescription(),
			id
		};

		// add subtype to path if set (currently only for app images)
		if (subType != CacheKeySubType.None)
		{
			path.Add(subType.GetDescription());
		}

		FilePath = string.Join('\\', path);
	}

	protected void SetKey (object key)
	{
		var fileName = key.ToString();

		var hasExtension = Path.HasExtension(fileName);
		if (!hasExtension)
		{
			fileName = Path.ChangeExtension(fileName, DEFAULT_EXTENSION);
		}

		Key = fileName;
	}

	public virtual string GetFullPath ()
	{
		return _fullPath ??= Path.Combine(FilePath, Key);
	}
}
