using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayBuffer<T> {
    private T[] _array;
    public const int DEFAULT_BUFFER_SIZE = 65534; //uint16 default low memory index format for meshes

    public ArrayBuffer(int __initialLength = DEFAULT_BUFFER_SIZE) {
        this._array = new T[__initialLength];
    }

    public T[] GetBuffer(int __length) {
        if (_array.Length < __length) {
            int newLength = Mathf.RoundToInt(__length * 1.2f);
            System.Array.Resize(ref _array, newLength);
        }
        return this._array;
    }
}
