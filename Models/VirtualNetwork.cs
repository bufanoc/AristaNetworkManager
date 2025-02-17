using System.Collections.Generic;

namespace AristaNetworkManager.Models
{
    public class VirtualNetwork
    {
        public string NetworkId { get; }
        public string Name { get; set; }
        public int VlanId { get; set; }
        public List<string> SwitchIds { get; set; } = new();
        public Dictionary<string, string> Properties { get; set; } = new();

        public VirtualNetwork(string networkId, string name, int vlanId)
        {
            NetworkId = networkId;
            Name = name;
            VlanId = vlanId;
        }
    }
}
