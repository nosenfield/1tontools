using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using OneTon.Logging;

namespace OneTon.PersistentData
{
    public static class PersistentDataService
    {
        private static readonly LogService logger = LogService.GetStatic(typeof(PersistentDataService));

        public static bool SaveSerializableData<T>(T obj, string directoryPath, string fileName)
        {
            // if directory does not exist, create directory
            string directory = Utilities.PathCombine.Combine(Application.persistentDataPath, directoryPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileLoc = Utilities.PathCombine.Combine(directory, fileName);
            string json = JsonConvert.SerializeObject(obj);
            StringHashPair dataStringObj = new StringHashPair(json);
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                FileStream stream = new FileStream(fileLoc, FileMode.Create);
                bf.Serialize(stream, dataStringObj);
                stream.Close();
                return true;
            }
            catch (System.Exception)
            {
                // file save error
                logger.Error($"Save error for: {fileLoc}");
                return false;
            }
        }

        internal static string ComputeHash(string str)
        {
            // NOTE
            // adding salt will go here
            // salt could be supplied as a param
            ///

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        /// <summary>
        /// Loads and deserializes the data at the supplied file path into an instance of the supplied type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The absolute file path</param>
        /// <returns>the data at the supplied file path as an instance of T</returns>
        public static T LoadDataOfType<T>(string directoryPath, string fileName) where T : class
        {
            T instance = null;
            string filePath = Utilities.PathCombine.Combine(directoryPath, fileName);
            StringHashPair stringHashPair = LoadFileContents(filePath);
            if (stringHashPair != null)
            {
                // This prevents adjusting the raw json values without rehashing the string.
                // It's not tamper-proof but creates a slightly larger "barrier to entry" than a raw json file.
                // To make it more tamperproof, we should salt the hash by adding a piece of anonymous, but reproducable data to the string.
                // We then need to remove that data from the string before deserializing to an object

                if (ComputeHash(stringHashPair.String) != stringHashPair.Hash)
                {
                    logger.Warn($"Non-matching hash for file: {filePath}");
                }
                else
                {
                    // NOTE
                    // removing salt goes here
                    ///

                    instance = JsonConvert.DeserializeObject<T>(stringHashPair.String);
                }
            }

            return instance;
        }

        /// <summary>
        /// Checks the device's persistent data path for a particular file located at the supplied path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>The file contents deserialized into an instance of StringHashPair containing a string of serialized data and the hash of that string</returns>
        private static StringHashPair LoadFileContents(string filePath)
        {
            logger.Debug($"LoadFileContents at: {filePath}");
            string fileLoc = Utilities.PathCombine.Combine(Application.persistentDataPath, filePath);
            if (!File.Exists(fileLoc))
            {
                logger.Warn($"No file at: {fileLoc}");
                return null;
            }

            try
            {
                FileStream stream = new FileStream(fileLoc, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                StringHashPair dataString = (StringHashPair)formatter.Deserialize(stream);
                stream.Close();
                return dataString;
            }
            catch (System.Exception err)
            {
                logger.Error($"FileStream read failed: {err}");
                return null;
            }
        }

        public static bool DeleteFile(string directoryPath, string fileName)
        {
            string filePath = Utilities.PathCombine.Combine(directoryPath, fileName);
            string fileLoc = Utilities.PathCombine.Combine(Application.persistentDataPath, filePath);
            logger.Debug($"DeleteFile at: {fileLoc}");

            if (!File.Exists(fileLoc))
            {
                logger.Warn($"No file at: {fileLoc}");
                return false;
            }

            try
            {
                File.Delete(fileLoc);
                logger.Debug($"Deletion complete");
                return true;
            }
            catch (System.Exception err)
            {
                logger.Error($"Deletion failed: {err}");
                return false;
            }
        }
    }
}