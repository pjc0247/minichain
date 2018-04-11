using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace minichain
{
    /// <summary>
    /// Slow and unstable, but easy to implement
    /// </summary>
    public class FileDB : IDisposable
    {
        public string key;

        public FileDB()
        {
            key = Path.GetRandomFileName();
            Directory.CreateDirectory(key);
        }
        ~FileDB()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(key, true);
            }
            catch { }
        }

        private string GetRelPath(string path)
        {
            return Path.Combine(key, path);
        }
        private string GetFilePath(string key)
        {
            var path = GetRelPath(key);
            if (key.Contains("/"))
            {
                key = key.Replace('/', Path.DirectorySeparatorChar);
                var pp = key.Split(Path.DirectorySeparatorChar);
                path = string.Join(Path.DirectorySeparatorChar.ToString(), pp.Take(pp.Length-1).ToArray());

                if (Directory.Exists(GetRelPath(path)) == false)
                    Directory.CreateDirectory(GetRelPath(path));

                path = Path.Combine(GetRelPath(path), pp.Last());
            }
            return path;
        }

        public void Write(string key, object value)
        {
            File.WriteAllText(GetFilePath(key), JsonConvert.SerializeObject(value, Formatting.Indented));
        }
        public T Read<T>(string key)
        {
            try
            {
                var json = File.ReadAllText(GetFilePath(key), Encoding.UTF8);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }
    }
}
