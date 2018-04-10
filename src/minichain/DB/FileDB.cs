using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace minichain
{
    public class FileDB
    {
        public string key;

        public FileDB()
        {
            key = Path.GetRandomFileName();
            Directory.CreateDirectory(key);
        }

        private string GetRelPath(string path)
        {
            return Path.Combine(key, path);
        }

        public void Write(string key, object value)
        {
            var path = GetRelPath(key);
            if (key.Contains("/"))
            {
                path = key.Split('/')[0];
                if (Directory.Exists(GetRelPath(path)) == false)
                    Directory.CreateDirectory(GetRelPath(path));

                path = Path.Combine(GetRelPath(path), key.Split('/')[1]);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(value));
        }
    }
}
