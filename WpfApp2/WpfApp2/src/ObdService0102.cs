using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.src
{
    class ObdService0102
    {
        private List<ObdData> _ObdDataList;

        public List<ObdData> Get_ObdDataList()
        {
            return _ObdDataList;
        }

        public void Set_ObdDataList(List<ObdData> value)
        {
            _ObdDataList = value;
        }

        public static void requestPid(UInt16 pid)
        {

        }

        static private UInt16 tmpPid = 0;
        public void monitorAllPid()
        {       
            foreach (var obdVehData in _ObdDataList)
            {
                if (tmpPid != obdVehData.Get_pid())
                {
                    tmpPid = obdVehData.Get_pid();
                }
            }
        }

        public static void obdRespCbk()
        {

        }
    }
}
