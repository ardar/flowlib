namespace ClientExample.Containers
{
    public class OnlineStatus
    {
        public string HubId
        {
            get;
            set;
        }

        public string UserId
        {
            get;
            set;
        }

        public int Status
        {
            get;
            set;
        }

        public OnlineStatus(string hubId, string userId, int status)
        {
            UserId = userId;
            HubId = hubId;
            Status = status;
        }
    }
}
