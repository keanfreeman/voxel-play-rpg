using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    [CreateAssetMenu(fileName = "New Feature", menuName = "ScriptableObjects/Features/SpellcastingFeature")]
    public class SpellcastingFeatureSO : FeatureSO
    {
        public int spellSaveDC;
    }
}
