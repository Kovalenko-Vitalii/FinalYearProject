using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GUI/Stats/Stat Library")]
public class StatLibrary : ScriptableObject
{
    public List<StatDescriptor> descriptors;
    Dictionary<StatId, StatDescriptor> _map;
    public StatDescriptor Get(StatId id)
    {
        _map ??= descriptors.ToDictionary(d => d.id, d => d);
        return _map[id];
    }
}