using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer.Game.File
{
    public class FileReader
    {
        public List<FileDatas> Reader(string filePath)
        {
            List<FileDatas> result = new List<FileDatas>();
            using (StreamReader _streamReader = new StreamReader(filePath))
            {
                string line;
                FileDatas item = null;
                while ((line = _streamReader.ReadLine()) != null)
                {
                    if (line.IndexOf(":") > 0)
                    {
                        item = new FileDatas();
                        item.key = line.TrimEnd(':');
                        item.datas = new List<string>();
                        result.Add(item);
                    }
                    else 
                    {
                        item.datas.Add(line.Trim());
                    }

                }
            }
            return result;
        }
    }


    public class FileDatas
    {
        public string key;
        public List<string> datas;
    }
    public enum ReadStat
    {
        None,
        Start,
        Reading
    }
}
