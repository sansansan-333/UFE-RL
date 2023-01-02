using System;
using UnityEngine;

public class ObjectBuffer<T> {
    private T[] elems;
    private int length;
    private int head; // index of where the next element should be in
    public int Count { private set; get; }

    public ObjectBuffer(int length) {
        elems = new T[length];
        this.length = length;
        head = 0;
        Count = 0;
    }

    public T this[int i] {
        set {
            if (Count > 0 && length > i && i >= 0) {
                int index = (head - 1 - i + length) % length;
                elems[index] = value;
            } else {
                throw new IndexOutOfRangeException();
            }
        }

        get { 
            if(Count > 0 && length > i && i >= 0) {
                int index = (head - 1 - i + length) % length;
                return elems[index];
            } else {
                throw new IndexOutOfRangeException();
            }
        }
    }

    public void Add(T elem) {
        elems[head] = elem;
        head = (head + 1) % length;
        Count = Math.Min(Count + 1, length);
    }

    public void Clear() {
        elems = new T[length];
        head = 0;
        Count = 0;
    }
}
