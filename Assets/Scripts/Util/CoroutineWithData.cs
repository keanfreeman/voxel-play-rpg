using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://answers.unity.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html
public class CoroutineWithData<T> {
    public Coroutine coroutine { get; private set; }
    private object result;
    private IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target) {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run() {
        while (target.MoveNext()) {
            result = target.Current;
            yield return result;
        }
    }

    public bool HasResult() {
        return TypeUtils.IsSameTypeOrIsSubclass(result, typeof(T));
    }

    public T GetResult() {
        if (!HasResult()) {
            throw new System.ApplicationException($"Wrong type returned from coroutine: " +
                $"{result.GetType()}");
        }
        return (T)result;
    }
}
