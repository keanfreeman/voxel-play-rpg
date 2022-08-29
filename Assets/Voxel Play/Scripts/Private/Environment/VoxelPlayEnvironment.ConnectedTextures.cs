using UnityEngine;

namespace VoxelPlay
{

    public partial class VoxelPlayEnvironment : MonoBehaviour
    {

        void InitConnectedTextures ()
        {

            // Add connected textures
            ConnectedTexture [] ctt = Resources.LoadAll<ConnectedTexture> ("");
            int cttCount = ctt.Length;
            LogMessage(cttCount + " connected textures found.");
            for (int k = 0; k < cttCount; k++) {
                ConnectedTexture ct = ctt [k];
                if (ct == null) continue;
                VoxelDefinition vd = ctt [k].voxelDefinition;
                if (VoxelDefinition.IsNull(vd)) {
                    LogMessage("Connected texture " + (k + 1) + "/" + cttCount + " " + vd.name + " ignored since voxel definition is unknown or not yet loaded.");
                    continue;
                }
                LogMessage("Connected texture " + (k + 1) + "/" + cttCount + " " + vd.name + " loaded.");
                for (int j = 0; j < ct.config.Length; j++) {
                    ct.config [j].textureIndex = vd.textureArrayPacker.AddTexture (ct.config [j].texture, null, null, null);
                }
                ct.Init ();
            }
        }

    }


}
