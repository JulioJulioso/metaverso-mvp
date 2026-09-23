namespace Metaverso
{
    public interface INetworkSession
    {
        string RoomName { get; }
        bool IsOnline { get; }
    }

    public static class NetworkServices
    {
        public static INetworkSession Session { get; set; }

        public static bool IsOnline => Session != null && Session.IsOnline;
    }

    public sealed class OfflineSession : INetworkSession
    {
        public OfflineSession(string roomName)
        {
            RoomName = roomName;
        }

        public string RoomName { get; }
        public bool IsOnline => false;
    }
}
