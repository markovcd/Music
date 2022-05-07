using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Presentation.Utility;

internal class ThreadSafeList<T> : IList<T>, IList, IReadOnlyList<T>
{
  private readonly List<T> list = new();
  private readonly object syncRoot = new();
  
  public int Count
  {
    get
    {
      lock (syncRoot) return list.Count;
    }
  }
  
  public bool IsReadOnly => false;
  
  public T this[int index]
  {
    get
    {
      lock (syncRoot) return list[index];
    }
    set
    {
      lock (syncRoot) list[index] = value;
    }
  }
  
  object? IList.this[int index]
  {
    get
    {
      lock (syncRoot) return ((IList)list)[index];
    }
    set
    {
      lock (syncRoot) ((IList)list)[index] = value;
    }
  }
  
  bool IList.IsFixedSize => false;

  bool ICollection.IsSynchronized => true;

  object ICollection.SyncRoot => syncRoot;
  
  public void Add(T item)
  {
    lock (syncRoot) list.Add(item);
  }

  public void AddRange(IEnumerable<T> collection)
  {
    lock (syncRoot) list.AddRange(collection);
  }

  public void Clear()
  {
    lock (syncRoot) list.Clear();
  }
  
  public bool Contains(T item)
  {
    lock (syncRoot) return list.Contains(item);
  }
  
  public void CopyTo(T[] array, int arrayIndex)
  {
    lock (syncRoot) list.CopyTo(array, arrayIndex);
  }
  
  public int IndexOf(T item)
  {
    lock (syncRoot) return list.IndexOf(item);
  }

  public void Insert(int index, T item)
  {
    lock (syncRoot) list.Insert(index, item);
  }

  public bool Remove(T item)
  {
    lock (syncRoot) return list.Remove(item);
  } 
  
  public void RemoveAt(int index)
  {
    lock (syncRoot) list.RemoveAt(index);
  }
  
  public IEnumerator<T> GetEnumerator()
  {
    IEnumerable<T> listCopy;
    lock (syncRoot) listCopy = list.ToImmutableList();
    return listCopy.GetEnumerator();
  }
  
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
  
  void ICollection.CopyTo(Array array, int index)
  {
    CopyTo((T[])array, index);
  }
  
  int IList.Add(object? value)
  {
    lock (syncRoot) return ((IList)list).Add(value);
  }
  
  bool IList.Contains(object? value)
  {
    lock (syncRoot) return ((IList)list).Contains(value);
  }

  int IList.IndexOf(object? value)
  {
    lock (syncRoot) return ((IList)list).IndexOf(value);
  }
  
  void IList.Insert(int index, object? value)
  {
    lock (syncRoot) ((IList)list).Insert(index, value);
  }
  
  void IList.Remove(object? value)
  {
    lock (syncRoot) ((IList)list).Remove(value);
  }
}
