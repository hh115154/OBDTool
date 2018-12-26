using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.Windows;

namespace WpfApp2.src
{
    class XmlUtil
    {
        public static bool CreateXML<T>(T obj, string filePath)
        {
            XmlWriter writer = null;    //声明一个xml编写器
            XmlWriterSettings writerSetting = new XmlWriterSettings //声明编写器设置
            {
                Indent = true,//定义xml格式，自动创建新的行
                Encoding = UTF8Encoding.UTF8,//编码格式
            };

            try
            {
                //创建一个保存数据到xml文档的流
                writer = XmlWriter.Create(filePath, writerSetting);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建xml文档失败1");
                return false;
            }

            XmlSerializer xser = new XmlSerializer(typeof(T));  //实例化序列化对象

            try
            {
                xser.Serialize(writer, obj);  //序列化对象到xml文档
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建xml文档失败2");
                return false;
            }
            finally
            {
                writer.Close();
            }
            return true;
        }




        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        #endregion
    }
}
