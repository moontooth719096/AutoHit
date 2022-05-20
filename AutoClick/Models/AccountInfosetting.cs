using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClick.Models
{
    public class AccountInfosetting
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public List<string> HolidayLIst { get; set; }
        public List<string> MakeupWorkList { get; set; }
    }

}
