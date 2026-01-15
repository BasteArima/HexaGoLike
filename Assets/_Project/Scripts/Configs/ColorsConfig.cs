using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "ColorsConfig", menuName = "Configs/ColorsConfig")]
public class ColorsConfig : ScriptableObject
{
    [System.Serializable]
    public struct ColorMapping
    {
        public ColorType type;
        public Color color;
    }

    [SerializeField, ReorderableList] 
    private List<ColorMapping> _colorMappings = new List<ColorMapping>();

    private Dictionary<ColorType, Color> _colorsDictionary;

    private void InitDictionary()
    {
        _colorsDictionary = new Dictionary<ColorType, Color>();
        foreach (var mapping in _colorMappings)
        {
            if (!_colorsDictionary.ContainsKey(mapping.type))
            {
                _colorsDictionary.Add(mapping.type, mapping.color);
            }
        }
    }

    public Color GetColor(ColorType type)
    {
        if (_colorsDictionary == null)
            InitDictionary();

        if (_colorsDictionary.TryGetValue(type, out var color))
            return color;

        Debug.LogError($"Color for type {type} not found!");
        return Color.magenta;
    }

    public List<Color> GetAllColors()
    {
        return _colorMappings.Select(x => x.color).ToList();
    }
}

public enum ColorType
{
    White = 0,
    Blue = 1,
    Red = 2,
    Green = 3,
    Pink = 4,
    Yellow = 5,
    Black = 6
}