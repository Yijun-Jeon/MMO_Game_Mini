using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> where T : IComparable<T>
{
    List<T> _heap = new List<T>();

    // O(logN)
    public void Push(T data)
    {
        // ���� �� ���� ���ο� �����͸� �����Ѵ�
        _heap.Add(data);

        int now = _heap.Count - 1;
        // ������⸦ ����
        while (now > 0)
        {
            // ������⸦ �õ�
            int next = (now - 1) / 2;
            if (_heap[now].CompareTo(_heap[next]) < 0)
                break; // ����

            // �� ���� ��ü�Ѵ�
            T temp = _heap[now];
            _heap[now] = _heap[next];
            _heap[next] = temp;

            // �˻� ��ġ�� �̵��Ѵ�
            now = next;
        }
    }

    // O(logN)
    public T Pop()
    {
        // ��ȯ�� �����͸� ���� ����
        T ret = _heap[0];

        // ������ �����͸� ��Ʈ�� �̵��Ѵ�
        int lastIndex = _heap.Count - 1;
        _heap[0] = _heap[lastIndex];
        _heap.RemoveAt(lastIndex);
        lastIndex--;

        // ������ �������� ������� ����
        int now = 0;
        while (true)
        {
            int left = 2 * now + 1;
            int right = 2 * now + 2;

            int next = now;
            // ���ʰ��� ���簪���� ũ��, �������� �̵�
            if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                next = left;
            // �������� ���簪(���� �̵� ����)���� ũ��, ���������� �̵�
            if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)
                next = right;

            // ����/������ ��� ���簪���� ������ ����
            if (next == now)
                break;

            // �� ���� ��ü�Ѵ�
            T temp = _heap[now];
            _heap[now] = _heap[next];
            _heap[next] = temp;
            // �˻� ��ġ�� �̵��Ѵ�
            now = next;
        }

        return ret;
    }

    public int Count { get { return _heap.Count; } }
}
