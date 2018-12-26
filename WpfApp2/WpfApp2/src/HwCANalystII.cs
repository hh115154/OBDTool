using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}




public struct VCI_INIT_CONFIG
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
    public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
    public byte Timing1;
    public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
}



public struct VCI_BOARD_INFO1
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    public byte Reserved;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_Usb_Serial;
}

public struct CHGDESIPANDPORT
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}


namespace WpfApp2.src
{ 
    class HwCANalystII
    {
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, [In, Out] VCI_CAN_OBJ[] pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
        /*------------函数描述结束---------------------------------*/

        static UInt32 m_devtype = 4;//USBCAN2

        UInt32 m_bOpen = 0;
        static UInt32 m_devind = 0;
        static UInt32 m_canind = 0;

        static VCI_INIT_CONFIG vic;
        static UInt32 dwRel;



        [StructLayout(LayoutKind.Sequential)]
        public struct VCI_CAN_OBJ
        {
            public uint ID;
            public uint TimeStamp;        //时间标识
            public byte TimeFlag;         //是否使用时间标识
            public byte SendType;         //发送标志。保留，未用
            public byte RemoteFlag;       //是否是远程帧
            public byte ExternFlag;       //是否是扩展帧
            public byte DataLen;          //数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;    //数据
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;//保留位
        }

        public const UInt16 canRxBufLen = 100;
        public static VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[canRxBufLen];


        public const UInt32 OBD_REQ_CAN_ExtdID = 0x18CCCC01;
        public const UInt32 OBD_RESP_CAN_ExtdID = 0x18AAAA00;
        public const UInt32 OBD_REQ_CAN_ID = 0x7E0;
        public const UInt32 OBD_RESP_CAN_ID = 0x7E8;
        public const byte OBD_DIAG_CHNL = 0;


        public static void connectHwBox( )
        {
            if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
            {
                MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误");
                return;
            }

            vic.AccCode = 0x80000008;
            vic.AccMask = 0xFFFFFFFF;
            vic.Filter = 1;
            vic.Timing0 = 0x00;
            vic.Timing1 = 0x1C;
            vic.Mode = 0;
            dwRel = VCI_InitCAN(m_devtype, m_devind, m_canind, ref vic);

            if (dwRel != 1)
            {
                VCI_CloseDevice(m_devtype, m_devind);
                MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误");
                return;
            }

            UInt32 retV1 = 0;
            retV1 = VCI_StartCAN(m_devtype, m_devind, m_canind);
            if (retV1 != 1)
            {
                MessageBox.Show("启动设备失败");
                return;
            }


            for (int i = 0; i < canRxBufLen; i++)
            {
                m_recobj[i].Data = new byte[8];
                m_recobj[i].Reserved = new byte[3];
            }

        }

        private static void initCanData(ref VCI_CAN_OBJ pSend , CAN_Msg msg)
        {
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 0;
            pSend.ID = msg.GetId();
            pSend.DataLen = 8;
            pSend.Data = new byte[8];
            pSend.Data =(byte[]) msg.GetData().Clone();

            pSend.Reserved = new byte[3];
            pSend.Reserved[0] = 0;
            pSend.Reserved[1] = 0;
            pSend.Reserved[2] = 0;
            pSend.SendType = 0;

        }

        public static void sendCanMse(CAN_Msg msg)
        {
            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            UInt32 retv = 0;

            initCanData(ref sendobj,msg);

            retv = VCI_Transmit(m_devtype, m_devind, msg.GetChnl(), ref sendobj, 1);

            if (retv != 1)
            {
                MessageBox.Show("发送失败");
            }
        }

        public static CAN_Msg[] getCanMsgsFromBuf()
        {
            UInt32 res = 0;

            res = VCI_Receive(m_devtype, m_devind, m_canind, m_recobj, canRxBufLen, 0);
            CAN_Msg[] msgList = new CAN_Msg[res];
            if (res > 0)
            {
                for (int i = 0; i < res; i++)
                {
                    msgList[i] = new CAN_Msg();
                    msgList[i].SetId(m_recobj[i].ID);
                    msgList[i].SetData(m_recobj[i].Data);
                }
            }

            return msgList;
        }


        public static List<CAN_Msg> getObdRespMsgFromBuf()
        {
            List<CAN_Msg> msgList = new List<CAN_Msg>();
            UInt32 res = VCI_Receive(m_devtype, m_devind, m_canind, m_recobj, canRxBufLen, 0);

            if (res > 0)
            {
                for (int i = 0; i < res; i++)
                {
                    if (OBD_RESP_CAN_ID == m_recobj[i].ID)
                    {
                        if (0x10 ==(byte)( m_recobj[i].Data[0] & 0xF0))
                        {
                            CAN_Msg msgFC = new CAN_Msg();
                            HwCANalystII.newFlowControlFrm(msgFC);
                            HwCANalystII.sendCanMse(msgFC);
                        }
                       

                        CAN_Msg msg = new CAN_Msg();
                        msg = new CAN_Msg();
                        msg.SetId(m_recobj[i].ID);
                        msg.SetData(m_recobj[i].Data);
                        msgList.Add(msg);
                    }
                }
            }

            return msgList;

        }

        public static void newObdReqMsg(byte sid, byte pid,[In,Out]CAN_Msg msg)
        {

            msg.SetChnl(0);
            msg.SetId(OBD_REQ_CAN_ID);
            byte[] data = new byte[8];

            data[0] = (byte)(sid + 1);
            data[1] = sid;
            data[2] = pid;
            data[3] = 0x00;
            msg.SetData(data);

        }

        public static void newFlowControlFrm( [In, Out]CAN_Msg msg)
        {

            msg.SetChnl(0);
            msg.SetId(OBD_REQ_CAN_ID);
            byte[] data = new byte[8];

            data[0] = 0x30;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0x00;
            msg.SetData(data);

        }

        public static void ClearBuf()
        {
            VCI_ClearBuffer(m_devtype, m_devind, m_canind);
        }


    }
}
