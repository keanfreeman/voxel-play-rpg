using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Saving {
    public static class FileManager {
        public static void WriteSaveJson(string json) {
            string path = GetSaveDestination();
            FileStream file = File.Exists(path) ? File.OpenWrite(path) : File.Create(path);

            byte[] writeArr = Encoding.UTF8.GetBytes(json);
            file.Write(writeArr);
            file.Close();
        }

        public static string ReadSaveJson() {
            string path = GetSaveDestination();
            if (File.Exists(GetSaveDestination())) {
                return File.ReadAllText(path);
            }
            return null;
        }

        public static bool SaveExists() {
            return File.Exists(GetSaveDestination());
        }

        public static void Delete() {
            string path = GetSaveDestination();
            File.Delete(path);
        }

        private static string GetSaveDestination() {
            string path = Application.persistentDataPath + Path.PathSeparator + "save.json";
            return path;
        }
    }
}
