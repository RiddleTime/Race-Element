using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace RaceElement.Core.Settings;

public interface IGenericSettingsJson
{
}

public abstract class AbstractSettingsJson<T>
    where T : IGenericSettingsJson
{
    public abstract T Default();
    public abstract string Path { get; }
    public abstract string FileName { get; }

    private readonly FileInfo _settingsFile;

    public static T Cached { get; private set; }

    protected AbstractSettingsJson()
    {
        _settingsFile = new(Path + FileName);
        Cached = Get(false);
    }

    public T Get(bool cached = true)
    {
        if (cached && Cached != null)
            return Cached;

        if (!_settingsFile.Exists)
            return Default();

        try
        {
            using FileStream fileStream = _settingsFile.OpenRead();
            Cached = ReadJson(fileStream);
            return Cached;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return Default();
    }

    public void Save(T genericJson)
    {
        try
        {
            string jsonString = JsonConvert.SerializeObject(genericJson, Formatting.Indented);

            if (!_settingsFile.Exists && !Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            File.WriteAllText(Path + "\\" + FileName, jsonString);

            Cached = genericJson;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private T ReadJson(FileStream stream)
    {
        string jsonString = string.Empty;
        try
        {
            using (StreamReader reader = new(stream))
            {
                jsonString = reader.ReadToEnd();
                jsonString = jsonString.Replace("\0", "");
                reader.Close();
                stream.Close();
            }

            T t = JsonConvert.DeserializeObject<T>(jsonString);

            if (t == null)
                return Default();

            return t;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return Default();
    }

    public void Delete()
    {
        try
        {
            if (_settingsFile.Exists)
                _settingsFile.Delete();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}
