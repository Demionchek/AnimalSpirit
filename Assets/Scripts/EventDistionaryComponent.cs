using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[System.Serializable]
public class UnityEventDictionary
{
    [System.Serializable]
    public class KeyValuePair
    {
        public DialogType key;
        public UnityEngine.Events.UnityEvent value;
    }

    [SerializeField]
    private List<KeyValuePair> items = new List<KeyValuePair>();

    private Dictionary<DialogType, UnityEngine.Events.UnityEvent> dictionary;

    public UnityEngine.Events.UnityEvent this[DialogType key]
    {
        get
        {
            if (dictionary == null)
                RebuildDictionary();

            dictionary.TryGetValue(key, out var value);
            return value;
        }
    }

    public void Add(DialogType key, UnityEngine.Events.UnityEvent value)
    {
        items.Add(new KeyValuePair { key = key, value = value });
        RebuildDictionary();
    }

    public bool Contains(DialogType key)
    {
        dictionary.TryGetValue(key, out var value);
        return value != null;
    }

    private void RebuildDictionary()
    {
        dictionary = new Dictionary<DialogType, UnityEngine.Events.UnityEvent>();
        foreach (var item in items)
        {
            if (!dictionary.ContainsKey(item.key))
                dictionary[item.key] = item.value;
        }
    }
}