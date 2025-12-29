using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CellularTextureApplier))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CellularTextureApplier generator = (CellularTextureApplier)target;

        if (GUILayout.Button("Generate"))
        {
            generator.ApplyTexture();
        }
        if (GUILayout.Button("New Seed"))
        {
            generator.GenerateSeed();
        }
    }
}