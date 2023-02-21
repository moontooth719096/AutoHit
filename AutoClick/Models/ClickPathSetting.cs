using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClick.Models
{
    public class ClickPathSetting
    {
        public string Url { get; set; }
        public Login Login { get; set; }
        public string WorkButton { get; set; }
        public string UnWorkButton { get; set; }
        public string ChickHit { get; set; }
    }

    public class Login
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string LoginButton { get; set; }
    }

}
