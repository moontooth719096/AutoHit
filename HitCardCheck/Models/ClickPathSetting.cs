using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitCardCheck.Models
{
    public class ClickPathSetting
    {
        public string Url { get; set; }
        public Login Login { get; set; }
        public string HitCardLogButton { get; set; }
        public string HitCardSearchStartDate { get; set; }
        public string HitCardSearchEndDate { get; set; }
        public string HitCardSearchButton { get; set; }

        public string ChickHit { get; set; }
        
    }

    public class Login
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string LoginButton { get; set; }
        
    }

}
