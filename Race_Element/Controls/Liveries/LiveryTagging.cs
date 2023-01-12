using RaceElement.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static RaceElement.Controls.LiveryBrowser;

namespace RaceElement.Controls
{
    internal class LiveryTagging
    {
        public class LiveryTag
        {
            public Guid Guid { get; set; }
            public string Name { get; set; }

            /// <summary>
            /// <TeamName, CustomSkinName>
            /// </summary>
            public List<TaggedLivery> TaggedLiveries { get; set; } = new List<TaggedLivery>();
        }

        public class TaggedLivery
        {
            public int CarModelType { get; set; }
            public string TeamName { get; set; }
            public string CustomSkinName { get; set; }

            public override bool Equals(object obj)
            {

                if (obj == null)
                {
                    return false;
                }
                if (!(obj is TaggedLivery))
                {
                    return false;
                }

                TaggedLivery other = (TaggedLivery)obj;
                return CarModelType == other.CarModelType && TeamName == other.TeamName && CustomSkinName == other.CustomSkinName;
            }
        }

        public static List<LiveryTag> GetAllTags()
        {
            DirectoryInfo tagDir = GetTagDirectory();

            FileInfo[] jsonFiles = tagDir.GetFiles("*.json", SearchOption.TopDirectoryOnly);

            if (jsonFiles.Length == 0)
            {
                return new List<LiveryTag>();
            }

            List<LiveryTag> allTags = new List<LiveryTag>();

            foreach (FileInfo file in jsonFiles)
            {
                LiveryTag tag = LoadTag(file);
                allTags.Add(tag);
            }


            return allTags;
        }

        public static bool AddToTag(LiveryTag tag, LiveryTreeCar livery)
        {
            TaggedLivery taggedLivery = new TaggedLivery()
            {
                CarModelType = livery.CarsRoot.CarModelType,
                TeamName = livery.CarsRoot.TeamName,
                CustomSkinName = livery.CarsRoot.CustomSkinName,
            };

            if (!tag.TaggedLiveries.Contains(taggedLivery))
                tag.TaggedLiveries.Add(taggedLivery);
            else
                MainWindow.Instance.EnqueueSnackbarMessage($"{livery.CarsRoot.CustomSkinName} has already been tagged with {tag.Name}");

            SaveTag(tag);

            return false;
        }



        internal static void RemoveFromTag(LiveryTag tag, LiveryTreeCar livery)
        {
            TaggedLivery taggedLivery = new TaggedLivery()
            {
                CarModelType = livery.CarsRoot.CarModelType,
                TeamName = livery.CarsRoot.TeamName,
                CustomSkinName = livery.CarsRoot.CustomSkinName,
            };

            tag.TaggedLiveries.Remove(taggedLivery);
            SaveTag(tag);
        }

        public static LiveryTag CreateNewTag(string tagName)
        {
            LiveryTag tag = new LiveryTag()
            {
                Guid = Guid.NewGuid(),
                Name = tagName,
            };
            SaveTag(tag);
            return tag;
        }

        public static bool TagContainsCar(LiveryTag tag, LiveryTreeCar car)
        {
            bool found = false;

            tag.TaggedLiveries.ForEach(x =>
            {
                if (x.CarModelType == car.CarsRoot.CarModelType
                    && x.TeamName == car.CarsRoot.TeamName
                    && x.CustomSkinName == car.CarsRoot.CustomSkinName)
                    found = true;
            });

            return found;
        }

        private static LiveryTag SaveTag(LiveryTag tag)
        {
            DirectoryInfo tagDir = GetTagDirectory();

            FileInfo[] tagFiles = tagDir.GetFiles($"{tag.Name}.json");
            FileInfo liveryFile = null;

            if (tagFiles.Length == 0)
            {
                liveryFile = new FileInfo($"{FileUtil.RaceElementTagsPath}{tag.Name}.json");
            }
            else
            {
                foreach (FileInfo file in tagFiles)
                {
                    if (file.Name == $"{tag.Name}.json")
                    {
                        liveryFile = file;
                        break;
                    }
                }
            }

            string jsonString = JsonConvert.SerializeObject(tag, Formatting.Indented);

            File.WriteAllText(liveryFile.FullName, jsonString);

            return tag;
        }

        private static LiveryTag LoadTag(FileInfo file)
        {
            if (!file.Exists)
                return null;

            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    return LoadTag(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        private static LiveryTag LoadTag(Stream stream)
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

                return JsonConvert.DeserializeObject<LiveryTag>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return null;
        }

        private static DirectoryInfo GetTagDirectory()
        {
            DirectoryInfo tagDir = new DirectoryInfo(FileUtil.RaceElementTagsPath);

            if (!tagDir.Exists)
            {
                tagDir.Create();
            }

            return tagDir;
        }

        internal static void DeleteTag(LiveryTag tag)
        {
            DirectoryInfo tagDir = GetTagDirectory();

            FileInfo[] tagFiles = tagDir.GetFiles($"{tag.Name}.json");

            try
            {
                if (tagFiles.Length > 0)
                {
                    foreach (FileInfo file in tagFiles)
                    {
                        if (file.Name.Equals($"{tag.Name}.json"))
                        {
                            file.Delete();

                            break;
                        }
                    }
                }

                LiveryBrowser.Instance.FetchAllCars(true);
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
            }
        }
    }
}
