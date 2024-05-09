using System;

namespace SAM.Core.Messages;

public class RequestMessage(EntityType entityType, RequestType requestType)
{
    public static RequestMessage LibraryRefresh { get; } = new (EntityType.Library, RequestType.Refresh);
    public static RequestMessage FilterChanged { get; } = new (EntityType.Filter, RequestType.Apply);

    public EntityType EntityType { get; } = entityType;
    public RequestType RequestType { get; } = requestType;
    
    protected virtual bool Equals(RequestMessage other)
    {
        return EntityType == other.EntityType && RequestType == other.RequestType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((RequestMessage) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EntityType, RequestType);
    }

    public static bool operator ==(RequestMessage left, RequestMessage right)
    {
        if (ReferenceEquals(null, left)) return ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return false;
        return left.GetHashCode() == right.GetHashCode();
    }

    public static bool operator !=(RequestMessage left, RequestMessage right)
    {
        if (ReferenceEquals(null, left)) return !ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return true;
        return left.GetHashCode() != right.GetHashCode();
    }
}

public class RequestMessage<T>(T context, EntityType entityType, RequestType requestType) : RequestMessage(entityType, requestType)
{
    public T Context { get; } = context;
    
    protected virtual bool Equals(RequestMessage<T> other)
    {
        return EntityType == other.EntityType && RequestType == other.RequestType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((RequestMessage<T>) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Context, EntityType, RequestType);
    }

    public static bool operator ==(RequestMessage<T> left, RequestMessage<T> right)
    {
        if (ReferenceEquals(null, left)) return ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return false;
        return left.GetHashCode() == right.GetHashCode();
    }

    public static bool operator !=(RequestMessage<T> left, RequestMessage<T> right)
    {
        if (ReferenceEquals(null, left)) return !ReferenceEquals(right, null);
        if (ReferenceEquals(null, right)) return true;
        return left.GetHashCode() != right.GetHashCode();
    }
}
