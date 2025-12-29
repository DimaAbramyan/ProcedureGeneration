using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class SeedController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputSeed;

    [SerializeField] private TMP_InputField inputFrom;
    [SerializeField] private TMP_InputField inputTo;

    [SerializeField] private TMP_InputField inputCellSize;
    [SerializeField] private TMP_InputField inputPercent;

    [SerializeField] private CellularTextureApplier cellularTextureApplier;

    [SerializeField] private GenerateRooms generateRooms;

    private void Start()
    {
        cellularTextureApplier.Seed = Convert.ToUInt32(inputSeed.text); 
        generateRooms.fromColor = Convert.ToSingle(inputFrom.text); 
        generateRooms.toColor = Convert.ToSingle(inputTo.text); 
        generateRooms.rasterization.CellSize = Convert.ToInt32(inputCellSize.text); 
        generateRooms.rasterization.Percent = Convert.ToSingle(inputPercent.text);

        inputSeed.onEndEdit.AddListener(OnSeedChanged);
        inputFrom.onEndEdit.AddListener(OnFromChanged);
        inputTo.onEndEdit.AddListener(OnToChanged);
        inputCellSize.onEndEdit.AddListener(OnCellSizeChanged);
        inputPercent.onEndEdit.AddListener(OnPercentChanged);
    }

    private void OnSeedChanged(string seedText)
    {
        if (uint.TryParse(seedText, out uint seed))
        {
            cellularTextureApplier.Seed = seed;
            Debug.Log($"Seed установлен: {seed}");
        }
    }
    private void OnFromChanged(string fromText)
    {
        if (float.TryParse(fromText, out float fromColor))
        {
            generateRooms.fromColor = fromColor;
            Debug.Log($"Seed установлен: {fromColor}");
        }
    }

    private void OnToChanged(string toColorText)
    {
        if (float.TryParse(toColorText, out float toColor))
        {
            generateRooms.toColor = toColor;
        }
    }
    private void OnCellSizeChanged(string cellSizeText)
    {
        if (int.TryParse(cellSizeText, out int cellSize))
        {
            generateRooms.rasterization.CellSize = cellSize;
        }
    }
    private void OnPercentChanged(string percentText)
    {
        if (float.TryParse(percentText, out float percent))
        {
            generateRooms.rasterization.Percent = percent;
        }
    }
    public void ApplyNewSeed()
    {
        cellularTextureApplier.GenerateTexture();
    }
    public void GenerateRoom()
    {
        generateRooms.ClearFloor();
        generateRooms.Generate();
    }
}