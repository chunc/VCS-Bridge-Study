using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCS_WOZ
{
    class restaurant_class
    {
    }

    public class restaurant
    {
        public string Task { get; set; }
        public string Mode { get; set; }
        
        public string Name { get; set; }
        public string Cuisine { get; set; }
        public double Rating { get; set; }
        public double Reviews { get; set; }
        public double Distance { get; set; }
        public string Price { get; set; }
        public int Trial { get; set; }

        public double Score { get; set; }
        public string N_Audio { get; set; }

        public string R_Name { get; set; }
        public string R_Audio_Prompt { get; set; }
        public string R_Audio_Confirm { get; set; }
        public string R_Name_Incorrect { get; set; }
        public string R_Audio_Incorrect { get; set; }
    }
}
