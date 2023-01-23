using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node<T>
{
    public T value { get; private set; }
    public List<Node<T>> children { get; private set; }

    public Node(T value) {
        this.value = value;
        this.children = new List<Node<T>>();
    }

    public Node(T value, List<Node<T>> children) {
        this.value = value;
        this.children = children;
    }
}
