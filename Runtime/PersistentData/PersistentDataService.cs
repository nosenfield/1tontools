using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using OneTon.Logging;
using OneTon.Utilities;

namespace OneTon.PersistentData
{
    public static class PersistentDataService
    {
        private static readonly LogService logger = LogService.GetStatic(typeof(PersistentDataService));

        public static bool SaveSerializableData<T>(T obj, string directoryPath, string fileName)
        {
            string directory = Utils.SafePathCombine(Application.persistentDataPath, directoryPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filePath = Utils.SafePathCombine(directory, fileName);
            string json = JsonConvert.SerializeObject(obj);
            StringHashPair data = new StringHashPair(json);

            try
            {
                string output = JsonConvert.SerializeObject(data, Formatting.None);
                File.WriteAllText(filePath, output);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Save failed at {filePath}: {ex}");
                return false;
            }
        }

        public static T LoadDataOfType<T>(string directoryPath, string fileName) where T : class
        {
            string filePath = Utils.SafePathCombine(Application.persistentDataPath, directoryPath, fileName);
            if (!File.Exists(filePath))
            {
                logger.Warn($"No file found at: {filePath}");
                return null;
            }

            try
            {
                string raw = File.ReadAllText(filePath);
                StringHashPair pair = JsonConvert.DeserializeObject<StringHashPair>(raw);

                if (pair == null)
                {
                    logger.Error($"Deserialization failed for file: {filePath}");
                    return null;
                }

                if (ComputeHash(pair.String) != pair.Hash)
                {
                    logger.Warn($"Hash mismatch detected for file: {filePath}");
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(pair.String);
            }
            catch (Exception ex)
            {
                logger.Error($"Load failed at {filePath}: {ex}");
                return null;
            }
        }

        public static bool DeleteFile(string directoryPath, string fileName)
        {
            string filePath = Utils.SafePathCombine(Application.persistentDataPath, directoryPath, fileName);
            if (!File.Exists(filePath))
            {
                logger.Warn($"File not found for deletion: {filePath}");
                return false;
            }

            try
            {
                File.Delete(filePath);
                logger.Debug($"File deleted: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to delete file at {filePath}: {ex}");
                return false;
            }
        }

        public static string ComputeHash(string str)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}