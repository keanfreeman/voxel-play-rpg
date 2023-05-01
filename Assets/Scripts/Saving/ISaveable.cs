using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saving {
    public interface ISaveable
    {
        public void PopulateSaveData(SaveData saveData);
        public IEnumerator LoadFromSaveData(SaveData saveData);
    }
}
