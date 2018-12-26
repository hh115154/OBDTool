using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.src
{
    class CAN_Msg
    {
        private UInt32 _id;
        private byte[] _data;
        private byte _chnl;

        public void sendCanMsg()
        {

        }

        public byte GetChnl()
        {
            return _chnl;
        }

        public void SetChnl(byte value)
        {
            _chnl = value;
        }

        public byte[] GetData()
        {
            return _data;
        }

        public void SetData(byte[] value)
        {
            _data = value;
        }

        public uint GetId()
        {
            return _id;
        }

        public void SetId(uint value)
        {
            _id = value;
        }
    }


}
