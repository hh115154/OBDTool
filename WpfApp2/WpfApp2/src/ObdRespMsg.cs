using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.src
{
    enum FrmType
    {
        SF,
        FF,
        CF,
        FC
    }
    class ObdRespMsg:CAN_Msg
    {
        private UInt16 _dataLen;
        private byte _pid;
        private byte _funcCode;
        private FrmType _frmType;
        private byte _frmNr;
        private byte _sid;
        private byte[] _actvData;
        private bool _bMsgFinish;

        public bool Get_bMsgFinish()
        {
            return _bMsgFinish;
        }

        public void Set_bMsgFinish(bool value)
        {
            _bMsgFinish = value;
        }

        public byte[] Get_actvData()
        {
            return _actvData;
        }

        public void Set_actvData(byte[] value)
        {
            _actvData = value;
        }

        public byte Get_frmNr()
        {
            return _frmNr;
        }

        public void Set_frmNr(byte value)
        {
            _frmNr = value;
        }

        public FrmType Get_frmType()
        {
            return _frmType;
        }

        public void Set_frmType(FrmType value)
        {
            _frmType = value;
        }

        public byte Get_funcCode()
        {
            return _funcCode;
        }

        public void Set_funcCode(byte value)
        {
            _funcCode = value;
        }

        public byte Get_pid()
        {
            return _pid;
        }

        public void Set_pid(byte value)
        {
            _pid = value;
        }



        public byte Get_sid()
        {
            return _sid;
        }

        public void Set_sid(byte value)
        {
            _sid = value;
        }

        public UInt16 Get_dataLen()
        {
            return _dataLen;
        }

        public void Set_dataLen(UInt16 value)
        {
            _dataLen = value;
        }


        public static  void fillObdRespMsgByCF(CAN_Msg newRxMsg,[In,Out]ObdRespMsg myObdRespMsg)
        {
            byte currFrmNr = (byte)(newRxMsg.GetData()[0] & 0x0F);
            byte maxFrmNr = (byte)((myObdRespMsg.Get_dataLen() - 6) / 7 + 1);
            byte actvDataStrtIdx = (byte)((currFrmNr - 1) * 7 + 5 - myObdRespMsg.Get_sid());


            if (currFrmNr == maxFrmNr)
            {
                byte lastFrmDataLen = (byte)((myObdRespMsg.Get_dataLen() - 6) % 7);
                for (int i = 0; i < lastFrmDataLen; i++)
                {
                    myObdRespMsg.Get_actvData()[actvDataStrtIdx +i] = newRxMsg.GetData()[i + 1];
                }
                myObdRespMsg.Set_bMsgFinish(true);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    myObdRespMsg.Get_actvData()[actvDataStrtIdx + i] = newRxMsg.GetData()[i + 1];
                }
                myObdRespMsg.Set_bMsgFinish(false);
            }
        }


  
        public ObdRespMsg(CAN_Msg msg)
        {
            this.SetData(msg.GetData());

            
            switch (msg.GetData()[0] & 0xF0)
            {
                case 0x00:
                    this.Set_frmType(FrmType.SF);
                    this.Set_dataLen(msg.GetData()[0]);
                    this.Set_sid((byte)(msg.GetData()[1] & 0x0F));
                    this.Set_funcCode((byte)(msg.GetData()[1] & 0xF0));
                    if (0x70 != this.Get_funcCode())
                    {
                        byte[] actvDataSF = new byte[this.Get_dataLen() - this.Get_sid() - 1];
                        this.Set_pid(msg.GetData()[2]);
                        this.Set_bMsgFinish(true);



                        for (int i = 0; i < this.Get_dataLen() - this.Get_sid() - 1; i++)
                        {
                            actvDataSF[i] = this.GetData()[this.Get_sid() + 2 + i];
                        }
                        this.Set_actvData(actvDataSF);
                    }


                    break;
                case 0x10:




                    this.Set_frmType(FrmType.FF);
                    UInt16 low = msg.GetData()[1];
                    UInt16 hi = (UInt16)((msg.GetData()[0] & 0x0F) << 8);

                    UInt16 len = (UInt16)(low + hi);

                    this.Set_dataLen(len);
                    this.Set_sid((byte)(msg.GetData()[2] & 0x0F));
                    this.Set_funcCode((byte)(msg.GetData()[2] & 0xF0));
                    byte[] actvData = new byte[this.Get_dataLen() - this.Get_sid() - 1];
                    this.Set_pid(msg.GetData()[3]);
                    this.Set_bMsgFinish(false);
                    if (1 == this.Get_sid())
                    {
                        for (int i = 0;  i <= 3; i++)
                        {
                            actvData[i] = msg.GetData()[4 + i];
                        }
                    }
                    if (2 == this.Get_sid())
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            actvData[i] = msg.GetData()[5+ i];
                        }
                    }
                    this.Set_actvData(actvData);


                    break;
                case 0x20:
                    this.Set_frmType(FrmType.CF);
                    break;
                case 0x30:
                    this.Set_frmType(FrmType.FC);
                    break;
                default:
                    break;
            }



            //byte[] tmpdata = new byte[this.Get_dataLen()];

            //this.Set_actvData(tmpdata);

        }

        public ObdRespMsg()
        {
            this.SetId(HwCANalystII.OBD_REQ_CAN_ID);
            this.SetChnl(HwCANalystII.OBD_DIAG_CHNL);
            byte[] data = new byte[10];
            byte[] msgData = new byte[8];
            this.SetData(msgData);
            this.Set_actvData(data);
            this.Set_bMsgFinish(true);
            this.Set_dataLen(0);
            this.Set_frmNr(0);
            this.Set_frmType(0);
            this.Set_funcCode(0);
            this.Set_pid(0);
            this.Set_sid(0);
       

        }
    }
}
