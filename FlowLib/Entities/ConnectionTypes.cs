namespace FlowLib.Entities
{
    /// <summary>
    /// Enum representing Connection Types
    /// Not all are used for public use in Direct Connect
    /// </summary>
    public enum ConnectionTypes
    {
        /// <summary>
        /// Unknown connection type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Direct connection (Active mode in Direct Connect Protocol)
        /// </summary>
        Direct,
        /// <summary>
        /// Universal Plug and Play, (Active mode in Direct Connect Protocol)
        /// http://en.wikipedia.org/wiki/Universal_Plug_and_Play
        /// </summary>
        UPnP,
        /// <summary>
        /// Port Forwarding (Firewall, Active mode in Direct Connect Protocol)
        /// </summary>
        Forward,
        /// <summary>
        /// Behind Firewall, (Passive mode in Direct Connect Protocol)
        /// </summary>
        Passive,
        /// <summary>
        /// Socket5 (Passive mode in Direct Connect Protocol)
        /// </summary>
        Socket5
    }
}