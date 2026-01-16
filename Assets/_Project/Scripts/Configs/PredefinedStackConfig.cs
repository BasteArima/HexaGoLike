using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PredefinedStackConfig", menuName = "Configs/Predefined Stack Config")]
public class PredefinedStackConfig : ScriptableObject
{
    [SerializeField] private List<PredefinedStack> _hexagonTypes;

    public List<PredefinedStack> HexagonTypes => _hexagonTypes;
}

[System.Serializable]
public struct PredefinedStack
{
    public List<ColorType> HexagonTypes;
}