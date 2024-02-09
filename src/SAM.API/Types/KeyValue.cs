using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAM.API.Types;

public class KeyValue
{
	private static readonly KeyValue invalid = new();

	public List<KeyValue> Children;
	public string Name = @"<root>";
	public KeyValueType Type = KeyValueType.None;
	public bool Valid;
	public object Value;

	public KeyValue this [string key]
	{
		get
		{
			var child = Children?.SingleOrDefault(c => string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase));

			return child ?? invalid;
		}
	}

	public string AsString (string defaultValue = "")
	{
		return !Valid ? defaultValue : Value == null ? defaultValue : Value.ToString();
	}

	public int AsInteger (int defaultValue = default)
	{
		return !Valid
			? defaultValue
			: Type switch
			{
				KeyValueType.String => int.TryParse((string) Value, out var value) == false ? defaultValue : value,
				KeyValueType.WideString => int.TryParse((string) Value, out var value) == false ? defaultValue : value,
				KeyValueType.Int32 => (int) Value,
				KeyValueType.Float32 => (int) (float) Value,
				KeyValueType.UInt64 => (int) ((ulong) Value & 0xFFFFFFFF),
				_ => defaultValue
			};
	}

	public float AsFloat (float defaultValue = default)
	{
		return !Valid
			? defaultValue
			: Type switch
			{
				KeyValueType.String => float.TryParse((string) Value, out var value) == false ? defaultValue : value,
				KeyValueType.WideString => float.TryParse((string) Value, out var value) == false ? defaultValue : value,
				KeyValueType.Int32 => (int) Value,
				KeyValueType.Float32 => (float) Value,
				KeyValueType.UInt64 => (ulong) Value & 0xFFFFFFFF,
				_ => defaultValue
			};
	}

	public bool AsBoolean (bool defaultValue = default)
	{
		return !Valid
			? defaultValue
			: Type switch
			{
				KeyValueType.String => int.TryParse((string) Value, out var value) == false ? defaultValue : value != 0,
				KeyValueType.WideString => int.TryParse((string) Value, out var value) == false ? defaultValue : value != 0,
				KeyValueType.Int32 => (int) Value != 0,
				KeyValueType.Float32 => (float) Value != 0,
				KeyValueType.UInt64 => (ulong) Value != 0,
				_ => defaultValue
			};
	}

	public string GetLocalizedString (string language, string defaultValue)
	{
		var name = this [language].AsString();
		if (!string.IsNullOrEmpty(name))
			return name;

		if (language != @"english")
		{
			name = this [@"english"].AsString();

			if (!string.IsNullOrEmpty(name))
				return name;
		}

		name = AsString();
		return string.IsNullOrEmpty(name) ? defaultValue : name;
	}

	public override string ToString ()
	{
		return !Valid
			? @"<invalid>"
			: Type == KeyValueType.None
			? Name
			: $"{Name} = {Value}";
	}

	public static KeyValue LoadAsBinary (string path)
	{
		if (!File.Exists(path))
			return null;

		try
		{
			using var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var kv = new KeyValue();
			return kv.ReadAsBinary(input) == false ? null : kv;
		}
		catch
		{
			return null;
		}
	}

	public bool ReadAsBinary (Stream input)
	{
		Children = [];

		try
		{
			var type = input.ReadKeyValueType();

			while (type != KeyValueType.End)
			{
				var current = new KeyValue
				{
					Type = type,
					Name = input.ReadStringUnicode()
				};

				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				switch (type)
				{
					case KeyValueType.None:
					current.ReadAsBinary(input);
					break;
					case KeyValueType.String:
					current.Valid = true;
					current.Value = input.ReadStringUnicode();
					break;
					case KeyValueType.WideString:
					throw new FormatException($"{nameof(KeyValueType.WideString)} is unsupported");
					case KeyValueType.Int32:
					current.Valid = true;
					current.Value = input.ReadValueS32();
					break;
					case KeyValueType.UInt64:
					current.Valid = true;
					current.Value = input.ReadValueU64();
					break;
					case KeyValueType.Float32:
					current.Valid = true;
					current.Value = input.ReadValueF32();
					break;
					case KeyValueType.Color:
					current.Valid = true;
					current.Value = input.ReadValueU32();
					break;
					case KeyValueType.Pointer:
					current.Valid = true;
					current.Value = input.ReadValueU32();
					break;
					default:
					throw new FormatException();
				}

				if (input.Position >= input.Length)
					throw new FormatException();

				Children.Add(current);

				type = input.ReadKeyValueType();
			}

			Valid = true;
			return input.Position == input.Length;
		}
		catch
		{
			return false;
		}
	}
}
