using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

namespace ConsoleDemo
{
    class Testing
    {
        public Testing()
        {
            HubSetting setting = new HubSetting();
            setting.Address = "flow84.no-ip.org";
            setting.Port = 2876;
            setting.DisplayName = "FlowLibNick";
            setting.Password = "1";

            Hub hubConnection = new Hub(setting);
            hubConnection.Update += new FmdcEventHandler(hubConnection_Update);
            hubConnection.Protocol = new HubNmdcProtocol(hubConnection);
            hubConnection.Connect();

            System.Threading.Thread.Sleep(10 * 1000);

            foreach (System.Collections.Generic.KeyValuePair<string, User> var in hubConnection.Userlist)
            {
                System.Console.WriteLine(string.Format("LIST User: {0} is Operator: {1}", var.Value.UserInfo.ID, var.Value.UserInfo.IsOperator));
            }
        }

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            UserInfo usrInfo = null;
            switch (e.Action)
            {
                case Actions.UserOnline:
                    usrInfo = e.Data as UserInfo;
                    if (usrInfo != null)
                        System.Console.WriteLine(string.Format("ONLINE User: {0} is Operator: {1}", usrInfo.ID, usrInfo.IsOperator));
                    break;
                case Actions.UserInfoChange:
                    usrInfo = e.Data as UserInfo;
                    if (usrInfo != null)
                        System.Console.WriteLine(string.Format("CHANGE User: {0} is Operator: {1}", usrInfo.ID, usrInfo.IsOperator));
                    break;
                default:
                    break;
            }
        }
    }
}
