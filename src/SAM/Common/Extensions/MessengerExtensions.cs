using DevExpress.Mvvm;
using SAM.Core.Messages;

namespace SAM.Extensions;

public static class MessengerExtensions
{
    public static void SendAction(this IMessenger messenger, ActionMessage action)
    {
        messenger.Send(action);
    }

    public static void SendAction(this IMessenger messenger, EntityType entityType, ActionType action)
    {
        messenger.Send(new ActionMessage(entityType, action));
    }

    public static void SendAction<T>(this IMessenger messenger, T context, EntityType entityType, ActionType action)
    {
        messenger.Send(new ActionMessage<T>(context, entityType, action));
    }

    public static void SendRequest(this IMessenger messenger, RequestMessage request)
    {
        messenger.Send(request);
    }

    public static void SendRequest(this IMessenger messenger, EntityType entityType, RequestType request)
    {
        messenger.Send(new RequestMessage(entityType, request));
    }

    public static void SendRequest<T>(this IMessenger messenger, T context, EntityType entityType, RequestType request)
    {
        messenger.Send(new RequestMessage<T>(context, entityType, request));
    }
}
