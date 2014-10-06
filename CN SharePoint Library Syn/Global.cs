using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CN_SharePoint_Library_Syn
{  /// <summary>
    /// Global Class : 
    /// Writes the all activity in a Log file and saves it in a specified location.
    /// </summary>
    static class Global
    {
        static string output;
        static List<string> filter;
        static XDocument doc;

        /// <summary>
        /// 
        /// </summary>
        static Global()
        {
            doc = XDocument.Load(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "setting.xml"));

            //doc.Load(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "setting.xml")); 

            output = doc.Descendants("LogFilePath").First().Attribute("path").Value;
            filter = new List<string>();
            var exNodeList = doc.Descendants("filter");
            foreach (var nd in exNodeList)
                filter.Add(nd.Value);
        }
        /// <summary>
        /// Log : methods for performing the log write
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteLog(string Message, System.Diagnostics.EventLogEntryType logtype)
        {
            DateTime dateTime = System.DateTime.UtcNow.AddMinutes(330);
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            string logfileName = dateTime.ToString("dd_MMM_yy") + "_" + ".txt";
            StreamWriter streamwriter = new StreamWriter(output + logfileName, true);
            streamwriter.WriteLine(DateTime.Now + ": " + Message);
            streamwriter.Close();
            streamwriter.Dispose();

            System.Diagnostics.EventLog appLog =
    new System.Diagnostics.EventLog();
            appLog.Source = "CN SharePoint Library Syn";

            appLog.WriteEntry(Message, logtype);

            Console.WriteLine(Message);

        }
        /// <summary>
        /// DirectoryArray : List of all the directory that required to be watched.
        /// </summary>
        /// <returns>Array</returns>
        public static Boolean GetFileExtention(string filePath)
        {
            string fileEX = Path.GetExtension(filePath);
            return filter.Contains(fileEX);
        }


        public static string CleanInvalidCharacters(string name)
        {
            string cleanName = name;

            // remove invalid characters
            cleanName = cleanName.Replace(@"#", string.Empty);
            cleanName = cleanName.Replace(@"%", string.Empty);
            cleanName = cleanName.Replace(@"&", string.Empty);
            cleanName = cleanName.Replace(@":", string.Empty);
            cleanName = cleanName.Replace(@"<", string.Empty);
            cleanName = cleanName.Replace(@">", string.Empty);
            cleanName = cleanName.Replace(@"?", string.Empty);
            cleanName = cleanName.Replace(@"\", string.Empty);
            cleanName = cleanName.Replace(@"/", string.Empty);
            cleanName = cleanName.Replace(@"{", string.Empty);
            cleanName = cleanName.Replace(@"}", string.Empty);
            cleanName = cleanName.Replace(@"|", string.Empty);
            cleanName = cleanName.Replace(@"~", string.Empty);
            cleanName = cleanName.Replace(@"+", string.Empty);
            cleanName = cleanName.Replace(@",", string.Empty);

            // remove invalid start character
            if (cleanName.StartsWith("_"))
            {
                cleanName = cleanName.Substring(1);
            }

            // trim length
            if (cleanName.Length > 50)
                cleanName = cleanName.Substring(1, 50);

            // Remove leading and trailing spaces
            cleanName = cleanName.Trim();

            // Replace spaces with %20
            cleanName = cleanName.Replace(" ", "%20");

            return cleanName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<WatchedFolder> DirectoryToWatch()
        {
            List<WatchedFolder> liFolderToWatch = new List<WatchedFolder>();
            var ndWatchFolders = doc.Descendants("folder");
            foreach (var nd in ndWatchFolders)
                liFolderToWatch.Add(new WatchedFolder()
                {
                    Networklocation = nd.Attribute("networklocation").Value,
                    SpSite = nd.Attribute("spsite").Value,
                    SpLib = nd.Attribute("spLib").Value
                });
            return liFolderToWatch;
        }

        public static List<SPAccount> SPAccounts()
        {
            List<SPAccount> spaccounts = new List<SPAccount>();
            var ndWatchFolders = doc.Descendants("spaccount");
            foreach (var nd in ndWatchFolders)
            {
                if (nd.Attribute("ency").Value == "1")
                {
                    spaccounts.Add(new SPAccount()
                    {
                        SPSite = nd.Attribute("spsite").Value,
                        UserName = nd.Attribute("username").Value,
                        Password = Cryptography.Decrypt(nd.Attribute("password").Value, "P@ssw0rd")
                    });
                }
                else
                {
                    spaccounts.Add(new SPAccount()
                    {
                        SPSite = nd.Attribute("spsite").Value,
                        UserName = nd.Attribute("username").Value,
                        Password = nd.Attribute("password").Value
                    });
                }
            }
            return spaccounts;
        }
    }
}
