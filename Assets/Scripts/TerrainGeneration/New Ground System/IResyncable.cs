using UnityEngine;
using System.Collections.Generic;
using System;

public interface IResyncable
{
    string UID { get; set; }
    void RegisterResync();
}

[Serializable]
public class ResyncRef<T> where T : class, IResyncable
{

    [SerializeField] private string _uid;
    [NonSerialized] private bool _valueSet = false;
    [NonSerialized] private T _localValue = null;
    public bool ValueSet { get => _valueSet; set => _valueSet = value; }
    public string UID => _uid;

    public T Value
    {
        get
        {
            if(_valueSet)
            {
                return _localValue;
            }

            _localValue = (T)LevelManager.ResyncHub.GetResync(_uid, out _valueSet);
            return _localValue;
            
        }
        set
        {
            if(value == null)
            {
                _localValue = null;
                return;
            }

            if (!EqualityComparer<T>.Default.Equals(_localValue, value))
            {
                value.RegisterResync();
            }
            _valueSet = true;
            _localValue = value;
            _uid = value.UID;

            if(value.GetType() == typeof(CurvePoint))
            {
                Debug.Log($"CP uid set: " + _uid);
            }
        }
    }

    public ResyncRef(string uid)
    {
        _uid = uid;
    }

    public ResyncRef()
    {
        _localValue = default;
    }

    public ResyncRef<T> FreshCopy()
    {
        return new(UID);
    }

}
public class ResyncHub
{
    private Dictionary<string, IResyncable> _resyncDict = new();

    public void RegisterResync(IResyncable obj)
    {
        if (string.IsNullOrEmpty(obj.UID))
        {
            obj.UID = Guid.NewGuid().ToString();
            Debug.Log("Generating GUID for resync obj: " + obj.UID);
        }

        _resyncDict[obj.UID] = obj;
    }

    public IResyncable GetResync(string uid, out bool valueFound)
    {
        if (uid == null || !_resyncDict.ContainsKey(uid))
        {
            //Debug.LogWarning($"ResyncHub: No resyncable found for UID {uid}");
            valueFound = false;
            return null;
        }

        var val = _resyncDict[uid];

        if (val == null)
        {
            Debug.Log("Null value found in resyncDict for uid " +  uid);
        }
        valueFound = val != null;
        return val;
    }
}


