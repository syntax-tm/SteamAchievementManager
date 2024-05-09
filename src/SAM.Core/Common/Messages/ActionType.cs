namespace SAM.Core.Messages;

/// <summary>
/// Indicates the action being requested or action that was performed.
/// </summary>
/// <seealso cref="ActionMessage" />
/// <seealso cref="RequestMessage" />
public enum ActionType
{
    Adding,
    Added,
    Updating,
    Updated,
    Removing,
    Removed,
    Starting,
    Started,
    Uploading,
    Uploaded,
    Changing,
    Changed,
    Saving,
    Saved,
    Cancelling,
    Cancelled,
    Caching,
    Cached,
    Completed,
    Refreshing,
    Refreshed,
    Grouping,
    Filtering,
    Sorting,
    Favorited,
    Unfavorited,
    Hidden,
    Visible
}
