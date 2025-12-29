using Unity.Mathematics;
using UnityEngine;

public enum GenerationStyle { PerlinNoise, VoronoiNoise };
public class CellularTextureApplier : MonoBehaviour
{
    [SerializeField]
    private float scale = 3;
    [SerializeField]
    private int textureSize = 255;
    public Transform mapSize;
    public GenerationStyle generationStyle;
    public uint Seed;
    private float2 offset;
    public Texture2D NoiseMap { get; private set; }

    public float GetTextureScale()
    {
        return scale;
    }
    public int GetTextureSize() { return textureSize; }
    public void GenerateTexture()
    {
        GenerateSeed();
        ApplyTexture();
    }

    public void GenerateSeed()
    {
        uint2 hashed = math.hash(new uint2(Seed, 0x9E3779B9u));

        float2 normalized = new float2(
            hashed.x / (float)uint.MaxValue,
            hashed.y / (float)uint.MaxValue
        );

        offset = normalized * 100f;
    }

    public void ApplyTexture()
    {
        NoiseMap = CreateCellularTexture();
        
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = NoiseMap;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = material;
    }

    Texture2D CreateCellularTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float2 coord =
                    ((float2)xz(x, y) / texture.width) * scale +
                    offset;
                float2 cellular;
                    cellular = Mathf.PerlinNoise(coord.x, coord.y);
                if (generationStyle == GenerationStyle.VoronoiNoise)
                {
                    cellular = noise.cellular(coord);
                }
                float value = cellular.x;

                texture.SetPixel(x, y, new Color(value, value, value));
                /*if ((ColourComparison.ColourCheck(texture.GetPixel(x, y), 0.6f)))
                {
                    GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    room.transform.position = new Vector3(x, y);
                    room.name = room.name + $"(x:{x} y:{y})";
                }*/
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.Apply();
        //float mapScale = textureSize / scale;
        //mapSize.localScale = new Vector3(mapScale, 1, mapScale);
        return texture;
    }

    private float2 xz(int x, int y) => new float2(x, y);
}
