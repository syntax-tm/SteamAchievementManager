using System;
using System.Collections.Generic;

namespace SAM.Core.Messages;

public class ActionMessage(EntityType entityType, ActionType actionType)
{
    public static ActionMessage AppHidden { get; } = new (EntityType.Statistic, ActionType.Changed);
    public static ActionMessage AppFavorited { get; } = new (EntityType.SteamApp, ActionType.Favorited);
    public static ActionMessage StatChanged { get; } = new (EntityType.Statistic, ActionType.Changed);

    public EntityType EntityType { get; } = entityType;
    public ActionType ActionType { get; } = actionType;

    protected virtual bool Equals(ActionMessage other)
    {
        return EntityType == other.EntityType && ActionType == other.ActionType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((ActionMessage)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EntityType, ActionType);
    }

    public static bool operator ==(ActionMessage left, ActionMessage right)
    {
        if (ReferenceEquals(null, left)) return ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return false;
        return left.GetHashCode() == right.GetHashCode();
    }

    public static bool operator !=(ActionMessage left, ActionMessage right)
    {
        if (ReferenceEquals(null, left)) return !ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return true;
        return left.GetHashCode() != right.GetHashCode();
    }
}

public class ActionMessage<T>(T context, EntityType entityType, ActionType actionType) : ActionMessage(entityType, actionType)
{
    public T Context { get; } = context;

    public override int GetHashCode()
    {
        return HashCode.Combine(Context, EntityType, ActionType);
    }
    
    protected bool Equals(ActionMessage<T> other)
    {
        var contextEqual = EqualityComparer<T>.Default.Equals(Context, other.Context);

        return contextEqual && EntityType == other.EntityType && ActionType == other.ActionType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((ActionMessage<T>) obj);
    }

    public static bool operator ==(ActionMessage<T> left, ActionMessage<T> right)
    {
        if (ReferenceEquals(null, left)) return ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return false;
        return left.GetHashCode() == right.GetHashCode();
    }

    public static bool operator !=(ActionMessage<T> left, ActionMessage<T> right)
    {
        if (ReferenceEquals(null, left)) return !ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return true;
        return left.GetHashCode() != right.GetHashCode();
    }
}
