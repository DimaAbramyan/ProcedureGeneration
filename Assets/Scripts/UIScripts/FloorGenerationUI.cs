using UnityEngine;
using TMPro;
using System;

public class FloorGenerationUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputSeed;
    [SerializeField] private TMP_InputField inputSize;
    [SerializeField] private TMP_InputField inputFrom;
    [SerializeField] private TMP_InputField inputTo;
    [SerializeField] private TMP_InputField inputCellSize;
    [SerializeField] private TMP_InputField inputPercent;
    [SerializeField] private TMP_InputField inputCoridorPercent;

    [Header("Visuals")]
    [SerializeField] private CellularTextureApplier cellularTextureApplier;
    [SerializeField] private GameObject tilePrefab;

    [SerializeField] private GameObject LevelHandler;
    private uint seed;
    private int size; 
    private void Start()
    {
        inputSeed.onEndEdit.AddListener(OnSeedChanged);
        inputSize.onEndEdit.AddListener(OnSizeChanged);
        inputFrom.onEndEdit.AddListener(OnFromChanged);
        inputTo.onEndEdit.AddListener(OnToChanged);
        inputCellSize.onEndEdit.AddListener(OnCellSizeChanged);
        inputPercent.onEndEdit.AddListener(OnPercentChanged);
        inputCoridorPercent.onEndEdit.AddListener(OnCoridorPercentChanged);
    }

    #region UI Callbacks

    private void OnSeedChanged(string text)
    {
        if (uint.TryParse(text, out uint newSeed))
        {
            seed = newSeed;
            Debug.Log($"Seed изменён на: {seed}");
        }
        else
        {
            Debug.LogWarning($"Неверный формат сида: {text}");
        }
    }
    private void OnSizeChanged(string text)
    {
        if (int.TryParse(text, out int newSeed))
        {
            size = newSeed;
            Debug.Log($"Seed изменён на: {seed}");
        }
        else
        {
            Debug.LogWarning($"Неверный формат сида: {text}");
        }
    }
    private void OnFromChanged(string text) { }
    private void OnToChanged(string text) { }
    private void OnCellSizeChanged(string text) { }
    private void OnPercentChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp01(value);

            inputPercent.text = value.ToString("0.##");
        }
        else
        {
            inputPercent.text = "0";
        }
    }
private void OnCoridorPercentChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp01(value);

            inputCoridorPercent.text = value.ToString("0.##");
        }
        else
        {
            inputCoridorPercent.text = "0";
        }
    }

    #endregion
    public void ApplyNewSeed()
    {
        if (cellularTextureApplier != null)
        {
            cellularTextureApplier.GenerateTexture(seed, size);
            Debug.Log("Новая текстура сгенерирована");
        }
        else
        {
            Debug.LogError("CellularTextureApplier не назначен!");
        }
    }
    public void GenerateFloor()
    {
        Debug.Log("Старт генерации пола");
        foreach (Transform child in LevelHandler.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject coridors = GameObject.Find("Corridors");

        if (coridors != null)
        {
            Destroy(coridors);
        }
        FloorContext context = new FloorContext
        {
            floorData = new FloorData(),
            source = cellularTextureApplier,
            tilePrefab = tilePrefab,
            fromColor = float.TryParse(inputFrom.text, out float fFrom) ? fFrom : 0f,
            toColor = float.TryParse(inputTo.text, out float fTo) ? fTo : 1f,
            coridorPercent = float.TryParse(inputCoridorPercent.text, out float corPerc)
    ? Mathf.Clamp01(corPerc)
    : 1f,
            seed = seed
        };
        context.rasterization = new Rasterization(context)
        {
            CellSize = int.TryParse(inputCellSize.text, out int cs) ? cs : 8,

            Percent = float.TryParse(inputPercent.text, out float perc)
    ? Mathf.Clamp01(perc)
    : 0.5f
        };
        Debug.Log("CellSize: "+inputCellSize);
        Debug.Log(int.TryParse(inputCellSize.text, out int f) ? f : 8);

        Debug.Log($"Контекст готов: source={context.source}, rasterization={context.rasterization}");
        FloorGenerationPipeline pipeLine = new FloorGenerationPipeline(context);

        pipeLine.GenerateAsync();
        // --- 3. Генерация комнат ---
        //RoomGenerator roomGen = new RoomGenerator(context);
        //roomGen.Run();

        //// --- 4. Растеризация (уже в контексте) ---
        //context.rasterization.Run();

        //// --- 5. Триангуляция ---
        //new TriangulationGenerator(context).Run();

        //// --- 6. MinOstTree и блокировки ---
        //new MinOstTreeGenerator(context).Run();
        //new ResolveBlockedEdges(context).Run();

        //// --- 7. Визуализация ---
        //new LevelGenerator(context).Run();

        Debug.Log("Генерация завершена!");
    }


}
