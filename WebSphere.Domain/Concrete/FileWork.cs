using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;

namespace WebSphere.Domain.Concrete
{
    public class FileWork : IFileWork
    {
        public List<string> getDriversName ()
        {
            //DirectoryInfo dir = new DirectoryInfo(@"F:\WINNT");
            //FileInfo[] bmpfiles = dir.GetFiles("*.bmp);
            string dirName = "C:\\ForStudy";
            List<string> DriversName = new List<string>();
            if(Directory.Exists(dirName))
            {
                string[] files = Directory.GetFiles(dirName);
                foreach(string s in files)
                {
                    //string path = dirName + "\\" + s;
                    FileInfo fileInfo = new FileInfo(s);
                    if(fileInfo.Extension==".png")
                    {
                        string name = s.Replace(dirName + "\\", "");
                        DriversName.Add(name);
                    }

                }
            }
            return DriversName;
        }
    }
}
