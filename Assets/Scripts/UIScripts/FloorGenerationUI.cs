using UnityEngine;
using TMPro;
using System;

public class FloorGenerationUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputSeed;
    [SerializeField] private TMP_InputField inputFrom;
    [SerializeField] private TMP_InputField inputTo;
    [SerializeField] private TMP_InputField inputCellSize;
    [SerializeField] private TMP_InputField inputPercent;

    [Header("Visuals")]
    [SerializeField] private CellularTextureApplier cellularTextureApplier;
    [SerializeField] private GameObject tilePrefab;
    private uint seed;
    private void Start()
    {
        Debug.Log("залупа блять запустилась");
        inputSeed.onEndEdit.AddListener(OnSeedChanged);
        inputFrom.onEndEdit.AddListener(OnFromChanged);
        inputTo.onEndEdit.AddListener(OnToChanged);
        inputCellSize.onEndEdit.AddListener(OnCellSizeChanged);
        inputPercent.onEndEdit.AddListener(OnPercentChanged);
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

    private void OnFromChanged(string text) { }
    private void OnToChanged(string text) { }
    private void OnCellSizeChanged(string text) { }
    private void OnPercentChanged(string text) { }

    #endregion
    public void ApplyNewSeed()
    {
        if (cellularTextureApplier != null)
        {
            cellularTextureApplier.GenerateTexture(seed);
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

        // --- 1. Создаем контекст ---
        FloorContext context = new FloorContext
        {
            floorData = new FloorData(),
            source = cellularTextureApplier, // обязательно назначаем CellularTextureApplier
            tilePrefab = tilePrefab,
            fromColor = float.TryParse(inputFrom.text, out float fFrom) ? fFrom : 0f,
            toColor = float.TryParse(inputTo.text, out float fTo) ? fTo : 1f,
            seed = seed // <-- используем текущий seed
        };

        // --- 2. Инициализируем Rasterization ---
        context.rasterization = new Rasterization(context)
        {
            CellSize = int.TryParse(inputCellSize.text, out int cs) ? cs : 8,
            Percent = float.TryParse(inputPercent.text, out float perc) ? perc : 0.5f
        };

        Debug.Log($"Контекст готов: source={context.source}, rasterization={context.rasterization}");

        // --- 3. Генерация комнат ---
        RoomGenerator roomGen = new RoomGenerator(context);
        roomGen.Run();

        // --- 4. Растеризация (уже в контексте) ---
        context.rasterization.Run();

        // --- 5. Триангуляция ---
        new TriangulationGenerator(context).Run();

        // --- 6. MinOstTree и блокировки ---
        new MinOstTreeGenerator(context).Run();
        new ResolveBlockedEdges(context).Run();

        // --- 7. Визуализация ---
        new LevelGenerator(context).Run();

        Debug.Log("Генерация завершена!");
    }


}
