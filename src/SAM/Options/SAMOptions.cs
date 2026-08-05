using CommandLine;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SAM;

public abstract class OptionsBase : IOptionUri
{
    protected const string URI_SCHEME = @"sam";

    public abstract ApplicationMode Mode { get; }

    public abstract Uri ToUri();
    public abstract string[] GetArgs();

    protected Uri BuildUri(IEnumerable<string> items)
    {
        var segments = new List<string>(items);

        if (segments.Count < 1)
        {
            throw new ArgumentException($"At least one segment is required.", nameof(items));
        }

        var ub = new UriBuilder();
        ub.Scheme = URI_SCHEME;
        ub.Host = string.Join(@"/", segments);

        return ub.Uri;
    }
}

[Verb(VERB, true, ["open", "library", "apps"], HelpText = "Opens the library view for selecting apps")]
public class SelectOptions : OptionsBase
{
    internal const string VERB = "select";

    internal const char RESET_SHORT = 'r';
    internal const char VIEW_GRID_SHORT = 'g';
    internal const char VIEW_TILE_SHORT = 't';

    internal const string RESET = "reset";
    internal const string VIEW_GRID = "grid";
    internal const string VIEW_TILE = "tile";

    [Option(RESET_SHORT, RESET, Default = false)]
    public bool Reset { get; set; }

    [Option(VIEW_GRID_SHORT, VIEW_GRID, Default = false, SetName = "view")]
    public bool GridView { get; set; }

    [Option(VIEW_TILE_SHORT, VIEW_TILE, Default = false, SetName = "view")]
    public bool TileView { get; set; }

    public override ApplicationMode Mode => ApplicationMode.Default;

    public override string[] GetArgs()
    {
        List<string> args = [ VERB ];

        if (GridView)
            args.Add(VIEW_GRID);
        if (TileView)
            args.Add(VIEW_TILE);
        if (Reset)
            args.Add(RESET);

        return args.ToArray();
    }

    public override Uri ToUri()
    {
        List<string> segments = [ VERB ];

        if (GridView)
            segments.Add(VIEW_GRID);
        else if (TileView)
            segments.Add(VIEW_TILE);

        return BuildUri(segments);
    }
}

[Verb(VERB, false, HelpText = "Opens the manager for the specified app")]
public class ManageOptions : OptionsBase
{
    internal const string VERB = "manage";

    internal const char UNLOCK_ALL_SHORT = 'u';
    internal const char LOCK_ALL_SHORT = 'l';

    internal const string UNLOCK_ALL = "unlock-all";
    internal const string LOCK_ALL = "lock-all";

    internal const string ACTION_SET = "lock-unlock";

    [Value(0, HelpText = "A Steam app id", MetaName = "AppId")]
    public uint AppId { get; set; }
    
    [Option(UNLOCK_ALL_SHORT, UNLOCK_ALL, Default = false, SetName = ACTION_SET)]
    public bool UnlockAll { get; set; }

    [Option(LOCK_ALL_SHORT, LOCK_ALL, Default = false, SetName = ACTION_SET)]
    public bool LockAll { get; set; }

    public override ApplicationMode Mode => ApplicationMode.Manager;

    public override string[] GetArgs()
    {
        List<string> args = [ VERB, $"{AppId}" ];

        if (UnlockAll)
            args.Add(UNLOCK_ALL);
        if (LockAll)
            args.Add(LOCK_ALL);

        return args.ToArray();
    }

    public override Uri ToUri()
    {
        List<string> segments = [ VERB, $"{AppId}" ];

        if (UnlockAll)
            segments.Add(UNLOCK_ALL);
        else if (LockAll)
            segments.Add(LOCK_ALL);

        return BuildUri(segments);
    }
}
