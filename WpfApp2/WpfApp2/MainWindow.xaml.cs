using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Data;
using System.Runtime.Serialization;
using WpfApp2.src;
using System.IO;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using System.Data.Odbc;
using System.Data.OleDb;



namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ObdData> myObdDataList = new List<ObdData>();

        System.Data.DataTable dt = new System.Data.DataTable("myta");

        private byte getPidValueFromString(string src)
        {
            byte res = 0;
            if (src.Contains("0x"))
            {
                src.Remove(0, 2);
            }

            res = Convert.ToByte(src, 16);

            return res;
        }

        private static DispatcherTimer readDataTimer = new DispatcherTimer();

        private static UInt16 currentPidIdx = 0;

        public MainWindow()
        {
            InitializeComponent();

            string defaultFilePath = @"诊断数据.xlsx";

            string fileSuffix = System.IO.Path.GetExtension(defaultFilePath);



            string connString = "";

                connString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + defaultFilePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";

            //读取文件

            string sql_select = " SELECT * FROM [Sheet1$]";

            using (OleDbConnection conn = new OleDbConnection(connString))

            using (OleDbDataAdapter cmd = new OleDbDataAdapter(sql_select, conn))
            {
                conn.Open();
                cmd.Fill(dt);

            }
            dt.Columns.Add("Value");
            dtgShow.ItemsSource = dt.DefaultView;

            dtgShow.LayoutUpdated += new EventHandler(dataGrid1_LayoutUpdated);



            string[] str = new string[36];

            for (int j = 0; j < dt.Rows.Count; j++)
            {
                ObdData tmpObdData = new ObdData();
                tmpObdData.Set_LineNrInGrid((UInt16)j);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    
                    switch (dt.Columns[i].ToString())
                    {
                        case "PID":
                            tmpObdData.Set_pid(getPidValueFromString( dt.Rows[j][i].ToString()));
                            break;
                        case "信号":
                            tmpObdData.Set_SigName(dt.Rows[j][i].ToString());
                            break;
                        case "精度":
                            UInt16 d1 = 1;
                            UInt16 d2 = 1;
                            string scaling = dt.Rows[j][i].ToString();
                            int idx = scaling.IndexOf("/");
                            float fScaling = 1.0f;
                            if (idx >= 0)
                            {
                                d1 = Convert.ToUInt16( scaling.Substring(0,idx));
                                d2 = Convert.ToUInt16(scaling.Substring(idx + 1));
                                fScaling = (float)d1 / d2;
                            }
                            else
                            {
                                fScaling = float.Parse(scaling);
                            }
                            tmpObdData.Set_Scaling(fScaling);
                            
                            break;
                        case "偏移":
                            tmpObdData.Set_offs(Convert.ToInt16(dt.Rows[j][i].ToString()));
                            break;
                        case "起始字节":
                            string strtByteTmp = dt.Rows[j][i].ToString();
                            byte[] nrStrtByte = System.Text.Encoding.ASCII.GetBytes(strtByteTmp);
                            nrStrtByte[0] -= (byte)'A';
                            tmpObdData.Set_strtByte(nrStrtByte[0]);
                            break;
                        case "起始位":
                            tmpObdData.Set_strtBit(Convert.ToByte(dt.Rows[j][i].ToString()));
                            break;
                        case "信号长度":
                            tmpObdData.Set_sigLen(Convert.ToByte(dt.Rows[j][i].ToString()));
                            break;
                        case "单位":
                            tmpObdData.Set_Uint(dt.Rows[j][i].ToString());
                            break;
                        default:
                            break;
                    }
                }
                myObdDataList.Add(tmpObdData);
            }


        }

        private void dataGrid1_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgShow.Columns.Count; i++)
            {
                switch (dtgShow.Columns[i].Header)
                {
                    case "PID":
                        break;
                    case "信号":
                        break;
                    case "精度":
                        dtgShow.Columns[i].Visibility = Visibility.Hidden;
                        break;
                    case "偏移":
                        dtgShow.Columns[i].Visibility = Visibility.Hidden;
                        break;
                    case "起始字节":
                        dtgShow.Columns[i].Visibility = Visibility.Hidden;
                        break;
                    case "起始位":
                        dtgShow.Columns[i].Visibility = Visibility.Hidden;
                        break;
                    case "信号长度":
                        dtgShow.Columns[i].Visibility = Visibility.Hidden;
                        break;
                    case "单位":
                        break;
                    default:
                        break;
                }
            }
        }

        byte testPid = 0xA2;
        private void updateObdGridView(ObdRespMsg msg,[In, Out]System.Data.DataTable myDt)
        {
            foreach (ObdData obdDt in myObdDataList)
            {
                if (msg.Get_pid() == obdDt.Get_pid())
                {

                    if (testPid == obdDt.Get_pid())
                    {
                        int a = 0;
                    }
                    obdDt.Set_sigValue( obdDt.getSigValFromOrgData(msg.Get_actvData()));
                    for (int i = 0; i < myDt.Columns.Count; i++)
                    {
                        if ("Value" == dt.Columns[i].ToString())
                        {
                            myDt.Rows[obdDt.Get_LineNrInGrid()][i] = obdDt.Get_sigValue();
                        }
                    }
                        
                }
            }

        }
        static byte[] resData = new byte[10];
        static ObdRespMsg lastMsgBuf = new ObdRespMsg();
        static UInt16 tiCntr = 0;

        private void sendNextPidReq(byte lastPid)
        {
            if (currentPidIdx == myObdDataList.Count - 1)
            {
                currentPidIdx = 0;
            }
            else
            {
                while (lastPid == myObdDataList[currentPidIdx].Get_pid())
                {
                    currentPidIdx++;
                }
                if (currentPidIdx == myObdDataList.Count )
                {
                    currentPidIdx = 0;
                }
            }
            CAN_Msg msgSF = new CAN_Msg();
            HwCANalystII.newObdReqMsg(0x01, myObdDataList[currentPidIdx].Get_pid(), msgSF);
            HwCANalystII.sendCanMse(msgSF);
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //CAN_Msg[] myMsgList = new CAN_Msg[100];
            //myMsgList = HwCANalystII.getCanMsgsFromBuf();
            List<CAN_Msg> ObdRespCANMsgList = HwCANalystII.getObdRespMsgFromBuf();
            
            if (0 != ObdRespCANMsgList.Count)
            {
                tiCntr = 0;
                foreach (CAN_Msg msg in ObdRespCANMsgList)
                {
                    FrmType frmType = (FrmType)(msg.GetData()[0] >> 4);

                    switch (frmType)
                    {
                        case FrmType.SF:
                            ObdRespMsg obdMsgSF = new ObdRespMsg(msg);
                            if (0x70 != obdMsgSF.Get_funcCode())
                            {
                                updateObdGridView(obdMsgSF, dt);
                                sendNextPidReq(obdMsgSF.Get_pid());
                            }
                            else
                            {
                                lastMsgBuf = obdMsgSF;
                                MessageBox.Show("OBD 应答异常");
                            }

                            break;
                        case FrmType.FF:
                            //sendout a FC


                            ObdRespMsg obdMsgFF = new ObdRespMsg(msg);
                            lastMsgBuf = obdMsgFF;
                            break;
                        case FrmType.CF:
                            ObdRespMsg.fillObdRespMsgByCF(msg, lastMsgBuf);
                            if (lastMsgBuf.Get_bMsgFinish())
                            {
                                updateObdGridView(lastMsgBuf, dt);
                                for (int i = 0; i < lastMsgBuf.Get_dataLen()- lastMsgBuf.Get_sid()-1; i++)
                                {
                                    lastMsgBuf.Get_actvData()[i] = 0;
                                }
                                sendNextPidReq(lastMsgBuf.Get_pid());
                            }


                            break;
                        case FrmType.FC:
                            break;
                        default:
                            break;
                    }
                }

            }
            else
            {
                if ( 0x70 != lastMsgBuf.Get_funcCode())
                {
                    tiCntr++;
                    if (tiCntr > 20 )
                    {
                        tiCntr = 0;
                        CAN_Msg msg = new CAN_Msg();
                        HwCANalystII.newObdReqMsg(0x01, lastMsgBuf.Get_pid(), msg);
                        HwCANalystII.sendCanMse(msg);
                    }
                }

            }
        }


        /// <summary>
        /// export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {


            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            string localFilePath = "", fileNameExt = "", newFileName = "", FilePath = "";
            saveFileDialog.Filter = "Excel(*.xlsx)|*.xlsx|Excel(*.xls)|*.xls";
            saveFileDialog.RestoreDirectory = true;
            bool? result = saveFileDialog.ShowDialog();

            //点了保存按钮进入
            if (result == true)
            {
                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                Workbook excelWB = excelApp.Workbooks.Add(System.Type.Missing);    //创建工作簿（WorkBook：即Excel文件主体本身）  
                Worksheet excelWS = (Worksheet)excelWB.Worksheets[1];   //创建工作表（即Excel里的子表sheet） 1表示在子表sheet1里进行数据导出  

                //excelWS.Cells.NumberFormat = "@";     //  如果数据中存在数字类型 可以让它变文本格式显示  
                //将数据导入到工作表的单元格  
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    excelWS.Cells[1, i + 1] = dt.Columns[i].ToString();
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        excelWS.Cells[i + 2, j + 1] = dt.Rows[i][j].ToString();   //Excel单元格第一个从索引1开始  
                    }
                }
                //获得文件路径
                localFilePath = saveFileDialog.FileName.ToString();

                //获取文件名，不带路径
                fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);

                //获取文件路径，不带文件名
                FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\"));

                //给文件名前加上时间
               // newFileName = fileNameExt + "_" + DateTime.Now.ToString("yyyyMMdd");
                newFileName = FilePath + "\\" + fileNameExt;

                excelWB.SaveAs(newFileName);  //将其进行保存到指定的路径  
                excelWB.Close();
            }


        }

        // 释放资源  
        private void ReleaseCOM(object pObj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pObj);
            }
            catch
            {
                throw new Exception("释放资源时发生错误！");
            }
            finally
            {
                pObj = null;
            }
        }

        private string GetExcelSheetName(string pPath)
        {
            //打开一个Excel应用  
            Microsoft.Office.Interop.Excel.Application excelApp;
            Workbook excelWB;//创建工作簿（WorkBook：即Excel文件主体本身）  
            Workbooks excelWBs;
            Worksheet excelWS;//创建工作表（即Excel里的子表sheet）  

            Sheets excelSts;

            excelApp = new Microsoft.Office.Interop.Excel.Application();
            if (excelApp == null)
            {
                throw new Exception("打开Excel应用时发生错误！");
            }
            excelWBs = excelApp.Workbooks;
            //打开一个现有的工作薄  
            excelWB = excelWBs.Add(pPath);
            excelSts = excelWB.Sheets;
            //选择第一个Sheet页  
            excelWS = excelSts.get_Item(1);
            string sheetName = excelWS.Name;

            ReleaseCOM(excelWS);
            ReleaseCOM(excelSts);
            ReleaseCOM(excelWB);
            ReleaseCOM(excelWBs);
            excelApp.Quit();
            ReleaseCOM(excelApp);
            return sheetName;
        }

        /// <summary>
        /// import excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInport_Click(object sender, RoutedEventArgs e)
        {
            string pPath;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "Excel(*.xlsx)|*.xlsx|Excel(*.xls)|*.xls";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.FilterIndex = 1;
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;//允许同时选择多个文件 
            bool? result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                string[] files = openFileDialog.FileNames;
                pPath = files[0];
                FileInfo existingFile = new FileInfo(pPath);

            }
            var path = openFileDialog.FileName;
            string fileSuffix = System.IO.Path.GetExtension(path);

           // using (DataSet ds = new DataSet())
            //{

                //判断Excel文件是2003版本还是2007版本

                string connString = "";

                if (fileSuffix == ".xls")
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + pPath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
                else
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + pPath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";

                //读取文件

                string sql_select = " SELECT * FROM [Sheet1$]";

                using (OleDbConnection conn = new OleDbConnection(connString))

                using (OleDbDataAdapter cmd = new OleDbDataAdapter(sql_select, conn))
                {
                    conn.Open();
                    cmd.Fill(dt);

                }

            dt.Columns.Add("valude");


                dtgShow.ItemsSource = dt.DefaultView;

            //}

        }

        private void ButtonLink_Click(object sender, RoutedEventArgs e)
        {
            HwCANalystII.connectHwBox();
            //send out req of first element in the table 
            readDataTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            readDataTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            CAN_Msg msg = new CAN_Msg();
            msg.SetChnl(0);
            msg.SetId(0x100);
            byte[] data = {1,2,3,4,5,6,7,8 };
            msg.SetData(data);
            HwCANalystII.sendCanMse(msg);
        }

        private void ButtonMornitor_Click(object sender, RoutedEventArgs e)
        {

            readDataTimer.Start();
            CAN_Msg msg = new CAN_Msg();
            HwCANalystII.newObdReqMsg(0x01, myObdDataList[0].Get_pid(), msg);
            HwCANalystII.sendCanMse(msg);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            readDataTimer.Stop();
            for (int i = 0; i < resData.Length; i++)
            {
                resData[i] = 0;
            }
            lastMsgBuf.Set_bMsgFinish(true);
            tiCntr = 0;
            HwCANalystII.ClearBuf();

          
        }
    }
}
