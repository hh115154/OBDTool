using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfApp2.src
{
    class ObdData
    {
        private string _sigName;
        private byte _pid;
        private Int16 _offs;
        private float _scaling;
        private string _uint;
        private byte _strtByte;
        private byte _strtBit;
        private byte _sigLen;
        private string _sigValue;
        private UInt16 _lineNrInGrid;

        public ushort Get_LineNrInGrid()
        {
            return _lineNrInGrid;
        }

        public void Set_LineNrInGrid(ushort value)
        {
            _lineNrInGrid = value;
        }

        public string Get_sigValue()
        {
            return _sigValue;
        }

        public void Set_sigValue(string value)
        {
            _sigValue = value;
        }

        public byte Get_sigLen()
        {
            return _sigLen;
        }

        public void Set_sigLen(byte value)
        {
            _sigLen = value;
        }

        public byte Get_strtBit()
        {
            return _strtBit;
        }

        public void Set_strtBit(byte value)
        {
            _strtBit = value;
        }

        public byte Get_pid()
        {
            return _pid;
        }

        public void Set_pid(byte value)
        {
            _pid = value;
        }

        public short Get_offs()
        {
            return _offs;
        }

        public void Set_offs(short value)
        {
            _offs = value;
        }

        public float GetScaling()
        {
            return _scaling;
        }

        public void Set_Scaling(float value)
        {
            _scaling = value;
        }

        public string Get_Uint()
        {
            return _uint;
        }

        public void Set_Uint(string value)
        {
            _uint = value;
        }

        public string Get_SigName()
        {
            return _sigName;
        }

        public void Set_SigName(string value)
        {
            _sigName = value;
        }

        public byte Get_strtByte()
        {
            return _strtByte;
        }

        public void Set_strtByte(byte value)
        {
            _strtByte = value;
        }

        private string getFormatUint16(UInt16 data)
        {
            string str = "";
            float res = (data + this.Get_offs()) * this.GetScaling();
            //float res = data * this.GetScaling() - this.Get_offs();
            str = res.ToString();
            return str;
        }
        private string getFormatByte(byte data)
        {
            string str = "";
            float res = (data + this.Get_offs()) * this.GetScaling();
            str = res.ToString();
            return str;
        }
        private string getFormatUint32(UInt32 data)
        {
            string str = "";
            float res = (data + this.Get_offs()) * this.GetScaling();
            str = res.ToString();
            return str;
        }

        public string getSigValFromOrgData(byte[] data)
        {
            string str = "";
            int nrByte = 0;
            if (0 == this.Get_sigLen() % 8)
            {
                nrByte = this.Get_sigLen() / 8;
            }
            else
            {
                nrByte = this.Get_sigLen() / 8 + 1;
            }
            
            byte[] tarData = new byte[nrByte];
            byte msk = 0;
            if (this.Get_sigLen() <= 8)
            {
                
                tarData[0] = data[this.Get_strtByte()];
                tarData[0] >>= this.Get_strtBit();
                for (int i = 0; i < this.Get_sigLen(); i++)
                {
                    msk |= (byte)(0x01 << i);
                }
                tarData[0] &= msk;
                str = getFormatByte(tarData[0]);
            }

            if (nrByte > 1)
            {//long signal donot put into mid of byte
                switch (nrByte)
                {
                    case 2:
                        UInt16 tarWord = 0;
                            tarWord = data[this.Get_strtByte()];
                            tarWord <<= 8;
                            tarWord  |= data[this.Get_strtByte()+1];
                        str = getFormatUint16(tarWord);
                        break;
                    case 4:
                        UInt32 tarDWord = 0;
                        tarDWord = data[this.Get_strtByte()];
                        tarDWord <<= 8;
                        tarDWord |= data[this.Get_strtByte() + 1];
                        tarDWord <<= 8;
                        tarDWord |= data[this.Get_strtByte() + 2];
                        tarDWord <<= 8;
                        tarDWord |= data[this.Get_strtByte() + 3];
                        str = getFormatUint32(tarDWord);
                        break;
                    default:
                        break;
                }

            }
            return str;

        }
    }

}
