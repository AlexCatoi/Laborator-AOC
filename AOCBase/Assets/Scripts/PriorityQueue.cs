using System.Collections.Generic;

/// <summary>
/// Max-Priority Queue using a binary max-heap.
/// Highest priority value comes out first.
/// </summary>
public class PriorityQueue<T>
{
    private List<(T item, int priority)> heap = new();

    public int Count => heap.Count;

    /// <summary>
    /// Insert item with priority
    /// </summary>
    public void Enqueue(T item, int priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    /// <summary>
    /// Remove and return highest-priority item
    /// </summary>
    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new System.InvalidOperationException("Queue is empty");

        T root = heap[0].item;

        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count > 0)
            HeapifyDown(0);

        return root;
    }

    /// <summary>
    /// Return highest-priority item without removing
    /// </summary>
    public T Peek()
    {
        if (heap.Count == 0)
            throw new System.InvalidOperationException("Queue is empty");

        return heap[0].item;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            // MAX heap
            if (heap[index].priority <= heap[parent].priority)
                break;

            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            int largest = index;

            if (left < heap.Count &&
                heap[left].priority > heap[largest].priority)
            {
                largest = left;
            }

            if (right < heap.Count &&
                heap[right].priority > heap[largest].priority)
            {
                largest = right;
            }

            if (largest == index)
                break;

            Swap(index, largest);
            index = largest;
        }
    }

    private void Swap(int a, int b)
    {
        (heap[a], heap[b]) = (heap[b], heap[a]);
    }

    public bool Contains(T item)
    {
        foreach (var element in heap)
        {
            if (EqualityComparer<T>.Default.Equals(element.item, item))
                return true;
        }

        return false;
    }
}