using System;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace SAM.Core.Storage;

public class IsolatedStorageManager : IStorageManager
{
	private static readonly ILog log = LogManager.GetLogger(nameof(IsolatedStorageManager));
	private static readonly object syncLock = new();
	private static IsolatedStorageManager _instance;

	public string Path
	{
		get;
	}

	protected IsolatedStorageManager ()
	{
		using var store = IsolatedStorageFile.GetMachineStoreForAssembly();

		var fi = store.GetType().GetField(@"_rootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
		var path = (string) fi!.GetValue(store);

		log.Debug($"IsolatedStorageFile Path: '{path}'");

		Path = path;

		if (!store.DirectoryExists(@"apps"))
			{
			store.CreateDirectory(@"apps");
		}
	}

	public static IsolatedStorageManager Default
	{
		get
		{
			if (_instance != null)
				{
				return _instance;
			}

			lock (syncLock)
			{
				_instance = new();
			}

			return _instance;
		}
	}

	public void SaveBytes (string fileName, byte [] bytes, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();

		if (isoStorage.FileExists(fileName))
		{
			isoStorage.DeleteFile(fileName);
		}

		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

		file.Write(bytes, 0, bytes.Length);
	}

	public async Task SaveBytesAsync (string fileName, byte [] bytes, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();

		if (isoStorage.FileExists(fileName))
		{
			isoStorage.DeleteFile(fileName);
		}

		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

		await file.WriteAsync(bytes);
	}

	public void SaveImage (string fileName, Image img, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

		img.Save(file, img.RawFormat);
	}

	public async Task SaveImageAsync (string fileName, Image img, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

		img.Save(file, img.RawFormat);
	}

	public void SaveText (string fileName, string text, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);
		using var writer = new StreamWriter(file);

		writer.WriteLine(text);
	}

	public async Task SaveTextAsync (string fileName, string text, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);
		await using var writer = new StreamWriter(file);

		await writer.WriteLineAsync(text);
	}

	public byte [] GetBytes (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		if (!FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		using var isoStorage = GetStore();

		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

		var buffer = new byte [file.Length];
		_ = file.Read(buffer, 0, buffer.Length);

		return buffer;
	}

	public async Task<byte []> GetBytesAsync (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		if (!FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		using var isoStorage = GetStore();

		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

		var buffer = new byte [file.Length];
		_ = await file.ReadAsync(buffer);

		return buffer;
	}

	public Image GetImageFile (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();

		if (!isoStorage.FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

		var img = Image.FromStream(file);

		return img;
	}

	public async Task<Image> GetImageFileAsync (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();

		if (!isoStorage.FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

		var img = Image.FromStream(file);

		return img;
	}

	public string GetTextFile (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		if (!isoStorage.FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);
		using var reader = new StreamReader(file);
		var fileText = reader.ReadToEnd();

		return fileText;
	}

	public async Task<string> GetTextFileAsync (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();
		if (!isoStorage.FileExists(fileName))
			{
			throw new FileNotFoundException(nameof(fileName));
		}

		await using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);
		using var reader = new StreamReader(file);
		var fileText = await reader.ReadToEndAsync();

		return fileText;
	}

	public void CreateDirectory (string path)
	{
		ArgumentNullException.ThrowIfNull(path);

		using var isoStorage = GetStore();
		if (isoStorage.DirectoryExists(path))
			{
			return;
		}

		isoStorage.CreateDirectory(path);
	}

	public bool FileExists (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			{
			throw new ArgumentNullException(fileName);
		}

		using var isoStorage = GetStore();

		return isoStorage.FileExists(fileName);
	}

	public static IsolatedStorageFile GetStore ()
	{
		return IsolatedStorageFile.GetMachineStoreForAssembly();
	}
}
