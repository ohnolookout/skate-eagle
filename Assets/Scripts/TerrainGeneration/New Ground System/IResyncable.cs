using UnityEngine;
using System.Collections.Generic;
using System;

public interface IResyncable
{
    string UID { get; set; }
    void RegisterResync();
}

[Serializable]
public class ResyncRef<T> where T : IResyncable
{

    [SerializeField] private string _uid;
    [SerializeField] private bool _valueSet = false;
    [NonSerialized] private T _localValue = default;
    public bool HasValue => _valueSet;
    public string UID => _uid;

    public T Value
    {
        get
        {
            if(_valueSet)
            {
                return _localValue;
            }
            else
            {
                var val = LevelManager.ResyncHub.GetResync<T>(_uid, out _valueSet);
                if (_valueSet)
                {
                    _localValue = val;
                }
                return _localValue;
            }
        }
        set
        {
            if(value == null)
            {
                Debug.Log("Null value added to resync ref");
                return;
            }

            if (!EqualityComparer<T>.Default.Equals(_localValue, value))
            {
                value.RegisterResync();
            }
            _valueSet = true;
            _localValue = value;
            _uid = value.UID;
        }
    }

    public ResyncRef(T value)
    {
        Value = value;
    }

    public ResyncRef(string uid)
    {
        _uid = uid;
    }

    public ResyncRef()
    {
        _localValue = default;
    }

}


