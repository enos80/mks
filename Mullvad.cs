using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKS
{
    public class Mullvad
    {
        public string? IP { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool MullvadExitIP { get; set; }
        public bool mullvad_exit_ip { set => MullvadExitIP = value; }
        public string? MullvadHostname { get; set; }
        public string? mullvad_exit_ip_hostname { set => MullvadHostname = value; }


        public Mullvad()
        {
            
        }
    }
}
