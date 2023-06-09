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
            if (File.Exists(path)) {
                Delete(path);
            }
            FileStream file = File.Create(path);

            byte[] writeArr = Encoding.UTF8.GetBytes(json);
            file.Write(writeArr);
            file.Close();
        }

        public static string ReadSaveJson() {
            string path = GetSaveDestination();
            if (!File.Exists(GetSaveDestination())) {
                Debug.Log("No save present. Writing one from the base save.");
                TextAsset textAsset = Resources.Load<TextAsset>("SavedGame/Simpler 3D Tilemap;save");
                WriteSaveJson(textAsset.text);
            }
            return File.ReadAllText(path);
        }

        public static bool SaveExists() {
            return File.Exists(GetSaveDestination());
        }

        public static void Delete(string path) {
            File.Delete(path);
        }

        private static string GetSaveDestination() {
            string path = Application.persistentDataPath + Path.PathSeparator + "save.json";
            return path;
        }
    }
}
