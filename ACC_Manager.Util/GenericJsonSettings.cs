using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Util
{
    public abstract class AbstractSettingsJson<IGenericJson>
    {
        public abstract IGenericJson Default();

        public abstract string Path { get; }
        public abstract string FileName { get; }

        public FileInfo SettingsFile => new FileInfo(Path + FileName);

        public IGenericJson LoadJson()
        {
            if (!SettingsFile.Exists)
                return Default();

            try
            {
                using (FileStream fileStream = SettingsFile.OpenRead())
                {
                    return ReadJson(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return Default();
        }

        public IGenericJson ReadJson(Stream stream)
        {
            string jsonString = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonString = reader.ReadToEnd();
                    jsonString = jsonString.Replace("\0", "");
                    reader.Close();
                    stream.Close();
                }

                return JsonConvert.DeserializeObject<IGenericJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return Default();
        }

        public void SaveJson(IGenericJson accSettings)
        {
            string jsonString = JsonConvert.SerializeObject(accSettings, Formatting.Indented);

            if (!SettingsFile.Exists)
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);

            File.WriteAllText(Path + "\\" + FileName, jsonString);
        }

        public void Delete()
        {
            if (SettingsFile.Exists)
                SettingsFile.Delete();
        }
    }

    public interface IGenericSettingsJson
    {
    }
}
