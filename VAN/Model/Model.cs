using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAN.Model
{
    public class RootState
    {
        public List<State> States { get; set; }
    }

    public class RootDistrict
    {
        public List<District> Districts { get; set; }
    }

    public class State
    {
        public int state_id { get; set; }
        public string state_name { get; set; }

    }

    public class District
    {
        public int district_id { get; set; }
        public string district_name { get; set; }

    }

}
