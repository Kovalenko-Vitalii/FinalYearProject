using System.Collections.Generic;

public struct StatValue
{
    public StatId id;
    public float value;
}

public interface IStatProvider
{
    IEnumerable<StatValue> GetStats();

    IEnumerable<StatValue> GetBaselineForCompare();
}