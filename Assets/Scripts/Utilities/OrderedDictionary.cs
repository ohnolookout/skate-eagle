using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderedDictionary<TKey, TValue>
{
    private List<TKey> order = new();
    private Dictionary<TKey, TValue> dict = new();

    public void Add(TKey key, TValue value)
    {
        if (order.Contains(key))
        {
            order.Remove(key);
        }
        order.Add(key);
        dict[key] = value;
    }

    public void Remove(TKey key)
    {
        dict.Remove(key);
        order.Remove(key);
    }

    public void Remove(int index)
    {
        if(index > order.Count)
        {
            return;
        }
        if (dict.ContainsKey(order[index]))
        {
            dict.Remove(order[index]);
        }
        order.RemoveAt(index);
    }

    public TValue Value(TKey key)
    {
        return dict[key];
    }

    public TValue Value(int index)
    {
        return dict[order[index]];
    }

    public TKey Key(int index)
    {
        return order[index];
    }

    public int Count
    {
        get
        {
            return order.Count;
        }
    }

    public List<TKey> Order
    {
        get
        {
            return order;
        }
    }

    public Dictionary<TKey, TValue> Dictionary
    {
        get
        {
            return dict;
        }
    }
}
