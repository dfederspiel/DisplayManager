using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DisplayManager
{
    public class MonitorSwitcher
    {
        public MonitorSwitcher()
        {
        }

        public static bool GetDisplaySettings(ref CCDWrapper.DisplayConfigPathInfo[] pathInfoArray, ref CCDWrapper.DisplayConfigModeInfo[] modeInfoArray, bool ActiveOnly)
        {
            unsafe
            {
                uint num;
                uint num1;
                CCDWrapper.QueryDisplayFlags queryDisplayFlag = CCDWrapper.QueryDisplayFlags.AllPaths;
                if (ActiveOnly)
                {
                    queryDisplayFlag = CCDWrapper.QueryDisplayFlags.OnlyActivePaths;
                }
                if (CCDWrapper.GetDisplayConfigBufferSizes(queryDisplayFlag, out num, out num1) == 0)
                {
                    pathInfoArray = new CCDWrapper.DisplayConfigPathInfo[num];
                    modeInfoArray = new CCDWrapper.DisplayConfigModeInfo[num1];
                    if (CCDWrapper.QueryDisplayConfig(queryDisplayFlag, ref num, pathInfoArray, ref num1, modeInfoArray, IntPtr.Zero) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool LoadDisplaySettings(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CCDWrapper.DisplayConfigPathInfo));
            XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigTargetMode));
            XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigSourceMode));
            XmlSerializer xmlSerializer3 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigModeInfoType));
            XmlSerializer xmlSerializer4 = new XmlSerializer(typeof(CCDWrapper.LUID));
            List<CCDWrapper.DisplayConfigPathInfo> displayConfigPathInfos = new List<CCDWrapper.DisplayConfigPathInfo>();
            List<CCDWrapper.DisplayConfigModeInfo> displayConfigModeInfos = new List<CCDWrapper.DisplayConfigModeInfo>();
            XmlReader xmlReader = XmlReader.Create(fileName);
            xmlReader.Read();
            do
            {
            Label0:
                if (xmlReader.Name.CompareTo("DisplayConfigPathInfo") != 0 || !xmlReader.IsStartElement())
                {
                    if (xmlReader.Name.CompareTo("modeInfo") != 0 || !xmlReader.IsStartElement())
                    {
                        continue;
                    }
                    CCDWrapper.DisplayConfigModeInfo num = new CCDWrapper.DisplayConfigModeInfo();
                    xmlReader.Read();
                    xmlReader.Read();
                    num.id = Convert.ToUInt32(xmlReader.Value);
                    xmlReader.Read();
                    xmlReader.Read();
                    num.adapterId = (CCDWrapper.LUID)xmlSerializer4.Deserialize(xmlReader);
                    num.infoType = (CCDWrapper.DisplayConfigModeInfoType)xmlSerializer3.Deserialize(xmlReader);
                    if (num.infoType != CCDWrapper.DisplayConfigModeInfoType.Target)
                    {
                        num.sourceMode = (CCDWrapper.DisplayConfigSourceMode)xmlSerializer2.Deserialize(xmlReader);
                    }
                    else
                    {
                        num.targetMode = (CCDWrapper.DisplayConfigTargetMode)xmlSerializer1.Deserialize(xmlReader);
                    }
                    displayConfigModeInfos.Add(num);
                    goto Label0;
                }
                else
                {
                    displayConfigPathInfos.Add((CCDWrapper.DisplayConfigPathInfo)xmlSerializer.Deserialize(xmlReader));
                    goto Label0;
                }
            }
            while (xmlReader.Read());
            CCDWrapper.DisplayConfigPathInfo[] item = new CCDWrapper.DisplayConfigPathInfo[displayConfigPathInfos.Count];
            for (int i = 0; i < displayConfigPathInfos.Count; i++)
            {
                item[i] = displayConfigPathInfos[i];
            }
            CCDWrapper.DisplayConfigModeInfo[] lowPart = new CCDWrapper.DisplayConfigModeInfo[displayConfigModeInfos.Count];
            for (int j = 0; j < displayConfigModeInfos.Count; j++)
            {
                lowPart[j] = displayConfigModeInfos[j];
            }
            CCDWrapper.DisplayConfigPathInfo[] displayConfigPathInfoArray = new CCDWrapper.DisplayConfigPathInfo[0];
            CCDWrapper.DisplayConfigModeInfo[] displayConfigModeInfoArray = new CCDWrapper.DisplayConfigModeInfo[0];
            if (!MonitorSwitcher.GetDisplaySettings(ref displayConfigPathInfoArray, ref displayConfigModeInfoArray, false))
            {
                return false;
            }
            for (int k = 0; k < (int)item.Length; k++)
            {
                int num1 = 0;
                while (num1 < (int)displayConfigPathInfoArray.Length)
                {
                    if (item[k].sourceInfo.id != displayConfigPathInfoArray[num1].sourceInfo.id || item[k].targetInfo.id != displayConfigPathInfoArray[num1].targetInfo.id)
                    {
                        num1++;
                    }
                    else
                    {
                        item[k].sourceInfo.adapterId.LowPart = displayConfigPathInfoArray[num1].sourceInfo.adapterId.LowPart;
                        item[k].targetInfo.adapterId.LowPart = displayConfigPathInfoArray[num1].targetInfo.adapterId.LowPart;
                        break;
                    }
                }
            }
            for (int l = 0; l < (int)lowPart.Length; l++)
            {
                int num2 = 0;
                while (num2 < (int)displayConfigPathInfoArray.Length)
                {
                    if (lowPart[l].id == displayConfigPathInfoArray[num2].targetInfo.id && lowPart[l].infoType == CCDWrapper.DisplayConfigModeInfoType.Target)
                    {
                        lowPart[l].adapterId.LowPart = displayConfigPathInfoArray[num2].targetInfo.adapterId.LowPart;
                        break;
                    }
                    else if (lowPart[l].id != displayConfigPathInfoArray[num2].sourceInfo.id || lowPart[l].infoType != CCDWrapper.DisplayConfigModeInfoType.Source)
                    {
                        num2++;
                    }
                    else
                    {
                        lowPart[l].adapterId.LowPart = displayConfigPathInfoArray[num2].sourceInfo.adapterId.LowPart;
                        break;
                    }
                }
            }
            uint length = (uint)item.Length;
            uint length1 = (uint)lowPart.Length;
            long num3 = (long)CCDWrapper.SetDisplayConfig(length, item, length1, lowPart, CCDWrapper.SdcFlags.UseSuppliedDisplayConfig | CCDWrapper.SdcFlags.Apply | CCDWrapper.SdcFlags.SaveToDatabase);
            if (num3 == (long)0)
            {
                xmlReader.Close();
                return true;
            }
            Console.WriteLine(string.Concat("Failed to set display settings, ERROR: ", num3.ToString()));
            return false;
        }

        private static void Main(string[] args)
        {
            bool flag = false;
            string[] strArrays = args;
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str = strArrays[i];
                char[] chrArray = new char[] { ':' };
                string[] strArrays1 = str.Split(chrArray, 2);
                string lower = strArrays1[0].ToLower();
                string str1 = lower;
                if (lower != null)
                {
                    if (str1 == "-save")
                    {
                        MonitorSwitcher.SaveDisplaySettings(strArrays1[1]);
                        flag = true;
                    }
                    else if (str1 == "-load")
                    {
                        MonitorSwitcher.LoadDisplaySettings(strArrays1[1]);
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                Console.WriteLine("Monitor Profile Switcher command line utlility:\n");
                Console.WriteLine("Paremeters to MonitorSwitcher.exe:");
                Console.WriteLine("\t -save:{xmlfile} \t save the current monitor configuration to file");
                Console.WriteLine("\t -load:{xmlfile} \t load and apply monitor configuration from file");
                Console.WriteLine("");
                Console.WriteLine("Examples:");
                Console.WriteLine("\tMonitorSwitcher.exe -save:MyProfile.xml");
                Console.WriteLine("\tMonitorSwitcher.exe -load:MyProfile.xml");
                Console.ReadKey();
            }
        }

        public static bool SaveDisplaySettings(string fileName)
        {
            CCDWrapper.DisplayConfigPathInfo[] displayConfigPathInfoArray = new CCDWrapper.DisplayConfigPathInfo[0];
            CCDWrapper.DisplayConfigModeInfo[] displayConfigModeInfoArray = new CCDWrapper.DisplayConfigModeInfo[0];
            bool displaySettings = MonitorSwitcher.GetDisplaySettings(ref displayConfigPathInfoArray, ref displayConfigModeInfoArray, true);
            if (!displaySettings)
            {
                Console.WriteLine(string.Concat("Failed to get display settings, ERROR: ", displaySettings.ToString()));
                return false;
            }
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CCDWrapper.DisplayConfigPathInfo));
            XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigTargetMode));
            XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigSourceMode));
            XmlSerializer xmlSerializer3 = new XmlSerializer(typeof(CCDWrapper.DisplayConfigModeInfoType));
            XmlSerializer xmlSerializer4 = new XmlSerializer(typeof(CCDWrapper.LUID));
            XmlWriter xmlWriter = XmlWriter.Create(fileName);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("displaySettings");
            xmlWriter.WriteStartElement("pathInfoArray");
            CCDWrapper.DisplayConfigPathInfo[] displayConfigPathInfoArray1 = displayConfigPathInfoArray;
            for (int i = 0; i < (int)displayConfigPathInfoArray1.Length; i++)
            {
                xmlSerializer.Serialize(xmlWriter, displayConfigPathInfoArray1[i]);
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("modeInfoArray");
            for (int j = 0; j < (int)displayConfigModeInfoArray.Length; j++)
            {
                xmlWriter.WriteStartElement("modeInfo");
                CCDWrapper.DisplayConfigModeInfo displayConfigModeInfo = displayConfigModeInfoArray[j];
                xmlWriter.WriteElementString("id", displayConfigModeInfo.id.ToString());
                xmlSerializer4.Serialize(xmlWriter, displayConfigModeInfo.adapterId);
                xmlSerializer3.Serialize(xmlWriter, displayConfigModeInfo.infoType);
                if (displayConfigModeInfo.infoType != CCDWrapper.DisplayConfigModeInfoType.Target)
                {
                    xmlSerializer2.Serialize(xmlWriter, displayConfigModeInfo.sourceMode);
                }
                else
                {
                    xmlSerializer1.Serialize(xmlWriter, displayConfigModeInfo.targetMode);
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
            return true;
        }
    }
}
