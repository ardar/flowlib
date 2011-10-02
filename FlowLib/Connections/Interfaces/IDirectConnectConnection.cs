using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Interfaces;

namespace FlowLib.Connections.Interfaces
{
    public interface IDirectConnectConnection : IConnection
    {
        IShare Share { get; set; }
    }
}
