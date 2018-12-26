using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data;
using System.Xml.Serialization;

namespace WpfApp2.src
{
    class Dt2Xml
    {


        private static string ConvertDataTableToXML(DataTable xmlDS)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
            serializer.Serialize(writer, xmlDS);
            writer.Close();
            return sb.ToString();



            /*

            MemoryStream stream = null;
            XmlTextWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, Encoding.Default);
                xmlDS.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);
                ASCIIEncoding utf = new ASCIIEncoding();
                return utf.GetString(arr).Trim();
            }
            catch
            {
                return String.Empty;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
            */
        }

        private static DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (Exception ex)
            {
                string strTest = ex.Message;
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public static DataSet ReadXmlFileToDataSet(string filePath, DataTable dt)
        {
            string str = File.ReadAllText(@filePath);

            return ConvertXMLToDataSet(str);
        }

        public static void WriteDtToXmlFile(string filePath, DataTable dt)
        {
            string str = ConvertDataTableToXML(dt);
            File.WriteAllText(@filePath, str, Encoding.UTF8);

        }

    }
}
