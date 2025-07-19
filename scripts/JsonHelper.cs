using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    [Serializable]
    private class Wrapper<T> { public T[] Items; }

    public static string ToJson<T>(List<T> list, bool pretty = false)
    {
        var wrapper = new Wrapper<T> { Items = list.ToArray() };
        return JsonUtility.ToJson(wrapper, pretty);
    }
}


