using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Random = UnityEngine.Random;

public class StackSpawnController : MonoBehaviour
{
    public static StackSpawnController Instance { get; private set; }
    public static event Action OnStacksGenerated;

    [Serializable]
    public struct PredefinedStack
    {
        public List<ColorType> HexagonTypes;
    }

    [Header("Scripted Scenarios")]
    [SerializeField] private List<PredefinedStack> _scriptedStacks;

    [Header("Random Configs")]
    [SerializeField] private ColorsConfig _colorsConfig;
    [SerializeField, MinMaxSlider(2, 8)] private Vector2Int _minMaxHexCount;
    
    [Header("References")]
    [SerializeField] private Transform _stackPositionsParent;
    [SerializeField] private Hexagon _hexagonPrefab;
    [SerializeField] private HexagonStack _hexagonStackPrefab;

    private int _stackCounter;
    private int _globalSpawnIndex;

    public List<HexagonStack> ActiveStacks
    {
        get
        {
            var stacks = new List<HexagonStack>();
            foreach (Transform child in _stackPositionsParent)
            {
                var stack = child.GetComponentInChildren<HexagonStack>();
                if (stack != null) stacks.Add(stack);
            }
            return stacks;
        }
    }
    
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        StackController.OnStackPlaced += StackPlacedCallback;
    }

    private void OnDestroy()
    {
        StackController.OnStackPlaced -= StackPlacedCallback;
    }

    private void StackPlacedCallback(FieldSlot fieldSlot)
    {
        _stackCounter++;

        if (_stackCounter >= 3)
        {
            _stackCounter = 0;
            GenerateStacks();
        }
    }

    private void Start()
    {
        GenerateStacks();
    }

    private void GenerateStacks()
    {
        for (int i = 0; i < _stackPositionsParent.childCount; i++)
            GenerateStack(_stackPositionsParent.GetChild(i));
        
        OnStacksGenerated?.Invoke();
    }

    private void GenerateStack(Transform parent)
    {
        var hexStack = Instantiate(_hexagonStackPrefab, parent.position, Quaternion.identity, parent);
        hexStack.name = $"Stack {parent.GetSiblingIndex()}";

        var typesForThisStack = new List<ColorType>();

        if (_globalSpawnIndex < _scriptedStacks.Count)
        {
            typesForThisStack.AddRange(_scriptedStacks[_globalSpawnIndex].HexagonTypes);
        }
        else
        {
            var amount = Random.Range(_minMaxHexCount.x, _minMaxHexCount.y);
            var firstColorHexagonCount = Random.Range(0, amount);
            var typesArray = GetRandomTypes();

            for (int k = 0; k < amount; k++)
            {
                var type = k < firstColorHexagonCount ? typesArray[0] : typesArray[1];
                typesForThisStack.Add(type);
            }
        }

        _globalSpawnIndex++;

        for (int i = 0; i < typesForThisStack.Count; i++)
        {
            var currentType = typesForThisStack[i];
            
            var hexagonLocalPos = Vector3.up * i * .2f;
            var spawnPosition = hexStack.transform.TransformPoint(hexagonLocalPos);

            var hexagonInstance =
                Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity, hexStack.transform);
            
            var visualColor = _colorsConfig.GetColor(currentType);

            hexagonInstance.Init(currentType, visualColor);
            hexagonInstance.Configure(hexStack);

            hexStack.Add(hexagonInstance);
        }
    }

    private ColorType[] GetRandomTypes()
    {
        var allTypes = new List<ColorType>((ColorType[])Enum.GetValues(typeof(ColorType)));

        if (allTypes.Count < 2)
        {
            Debug.LogError("Not enough ColorTypes defined in Enum!");
            return new[] { ColorType.White, ColorType.Blue };
        }

        var firstType = allTypes[Random.Range(0, allTypes.Count)];
        allTypes.Remove(firstType);
        var secondType = allTypes[Random.Range(0, allTypes.Count)];

        return new[] { firstType, secondType };
    }
}