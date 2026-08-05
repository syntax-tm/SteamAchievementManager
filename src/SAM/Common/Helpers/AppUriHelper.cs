using System;
using System.Collections.Generic;
using System.Linq;
using Wpf.Ui.Controls;

namespace SAM;

public static class AppUriHelper
{
    private const string URI_SCHEME = @"sam";

    private static readonly Uri baseUri = new ($"{URI_SCHEME}:");

    public static string[] ParseUri(Uri uri)
    {
        // checks to make sure this is the correct scheme (e.g. sam:/...)
        if (!baseUri.IsBaseOf(uri))
        {
            var message = $"Invalid uri scheme '{uri.Scheme}' (expected '{URI_SCHEME}').";
            throw new ArgumentException(message);
        }

        // TODO: clean this up, needs to remove the scheme from the uri
        var path = uri.ToString().Split(':')[1];
        var args = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return args;
    }

    /// <summary>
    /// Converts an <see cref="Uri"/> formatted string to command line arguments.
    /// </summary>
    /// <param name="value">The <see langword="string"/> value to parse.</param>
    /// <returns>The <see cref="Uri.Segments"/> as a <see langword="string[]"/>.</returns>
    public static string[] ParseUri(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
        {
            return null;
        }

        var uri = new Uri(value, UriKind.Absolute);

        // checks to make sure this is the correct scheme (e.g. sam:/...)
        if (!baseUri.IsBaseOf(uri))
        {
            var message = $"Invalid uri scheme '{uri.Scheme}' (expected '{URI_SCHEME}').";
            throw new ArgumentException(message);
        }

        var path = uri.PathAndQuery;
        var args = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return args;
    }

    /// <summary>
    /// Attempts to convert an <see cref="Uri"/> formatted string to command line arguments.
    /// </summary>
    /// <param name="text">The <see langword="string"/> value to parse.</param>
    /// <param name="args">The result, if successful.</param>
    /// <returns><see langword="true"/> if <param name="text"></param> was a valid <see cref="Uri"/>.</returns>
    public static bool TryParseUri(string text, out string[] args)
    {
        args = [];

        if (string.IsNullOrEmpty(text)) return false;
        if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
        {
            return false;
        }

        var uri = new Uri(text, UriKind.Absolute);

        // checks to make sure this is the correct scheme (e.g. sam:/...)
        if (!baseUri.IsBaseOf(uri))
        {
            var message = $"Invalid uri scheme '{uri.Scheme}' (expected '{URI_SCHEME}').";
            throw new ArgumentException(message);
        }

        var path = uri.PathAndQuery;
        args = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return true;
    }

    /// <summary>
    /// Converts any valid <see cref="Uri"/> <see langword="string"/>s to command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments used to start the application.</param>
    /// <returns>The <see cref="Uri.Segments"/> as a <see langword="string[]"/>.</returns>
    public static string[] ParseArguments(string[] args)
    {
        if (args == null) return null;
        if (args.Length == 0) return args;

        foreach (var arg in args)
        {
            if (!TryParseUri(arg, out var parts))
            {
                continue;
            }

            return parts;
        }

        return args;
    }
}
