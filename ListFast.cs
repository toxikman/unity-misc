using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;


[Il2CppSetOption(Option.ArrayBoundsChecks, false), Il2CppSetOption(Option.NullChecks, false)]
public class ListFast<T> : IEnumerable<T>
{
	public const int DEFAULT_CAPACITY = 8;

	T[] _items;
	int _size = 0;

	static readonly T[] _emptyArray = new T[0];

	public ListFast( int capacityToStart )
	{
		_items = _emptyArray;
		Capacity = capacityToStart;
	}

	public ListFast() { _items = _emptyArray; }

	public ListFast( IEnumerable<T> init )
	{
		_items = _emptyArray;
		foreach(T x in init)
			Add(x);
	}


	public int Count
	{
		get { return _size; }
		set {
			if (value > Capacity)
				Capacity = value;
			_size = value;
		}
	}

	public int Capacity
	{
		get { return _items.Length; }
		set {
			if (value != _items.Length)
			{
				if (value > 0)
				{
					T[] newItems = new T[value];
					if (_size > 0)
						Array.Copy(_items, 0, newItems, 0, _size);
					_items = newItems;
				}
				else
					_items = _emptyArray;
			}
			if (value < _size)
				_size = value;
		}
	}


	public T this[int index]
	{
		get { return _items[index]; }
		set { _items[index] = value; }
	}


	public int IndexOf( T thing)
	{
		return Array.IndexOf(_items, thing, 0, _size);
	}


	public T Find( Predicate<T> match )
	{
		if (match == null)
			throw new ArgumentNullException();
		for(int i = 0; i < _size; i++)
			if (match(_items[i]))
				return _items[i];
		return default(T);
	}


	public int CountEmUp( Predicate<T> match )
	{
		if (match == null)
			throw new ArgumentNullException();
		int cnt = 0;
		for(int i = 0; i < _size; i++)
			cnt += match(_items[i]) ? 1 : 0;
		return cnt;
	}


	public bool All( Predicate<T> match )
	{
		return CountEmUp(match) == _size;
	}


	public void Add( T item )
	{
		if (_size >= _items.Length)
			EnsureCapacity(_size + 1);
		_items[_size++] = item;
//		_version++;
	}

	// Adds the elements of the given collection to the end of this list. If
	// required, the capacity of the list is increased to twice the previous
	// capacity or the new size, whichever is larger.
	//
	public void AddRange(IEnumerable<T> collection)
	{
		InsertRange(_size, collection);
	}


	// Inserts an element into this list at a given index. The size of the list
	// is increased by one. If required, the capacity of the list is doubled
	// before inserting the new element.
	//
	public void Insert(int index, T item)
	{
		if (_size == _items.Length) EnsureCapacity(_size + 1);
		if (index < _size) {
			Array.Copy(_items, index, _items, index + 1, _size - index);
		}
		_items[index] = item;
		_size++;
//		_version++;
	}


	// Inserts the elements of the given collection at a given index. If
	// required, the capacity of the list is increased to twice the previous
	// capacity or the new size, whichever is larger.  Ranges may be added
	// to the end of the list by setting index to the List's size.
	//
	public void InsertRange(int index, IEnumerable<T> collection)
	{
		ICollection<T> c = collection as ICollection<T>;
		if( c != null ) {    // if collection is ICollection<T>
			int count = c.Count;
			if (count > 0) {
				EnsureCapacity(_size + count);
				if (index < _size) {
					Array.Copy(_items, index, _items, index + count, _size - index);
				}

				// If we're inserting a List into itself, we want to be able to deal with that.
				if (this == c) {
					// Copy first part of _items to insert location
					Array.Copy(_items, 0, _items, index, index);
					// Copy last part of _items back to inserted location
					Array.Copy(_items, index+count, _items, index*2, _size-index);
				}
				else {
					T[] itemsToInsert = new T[count];
					c.CopyTo(itemsToInsert, 0);
					itemsToInsert.CopyTo(_items, index);
				}
				_size += count;
			}
		}
		else {
			using(IEnumerator<T> en = collection.GetEnumerator()) {
				while(en.MoveNext()) {
					Insert(index++, en.Current);
				}
			}
		}
//		_version++;
	}


	// Removes the element at the given index. The size of the list is
	// decreased by one.
	//
	public bool Remove(T item)
	{
		int index = IndexOf(item);
		if (index >= 0) {
			RemoveAt(index);
			return true;
		}

		return false;
	}

	// Removes the element at the given index. The size of the list is
	// decreased by one.
	//
	public void RemoveAt(int index) {
		_size--;
		if (index < _size) {
			Array.Copy(_items, index + 1, _items, index, _size - index);
		}
		_items[_size] = default(T);
//		_version++;
	}

	// Removes a range of elements from this list.
	//
	public void RemoveRange(int index, int count)
	{
		if (count > 0) {
			_size -= count;
			if (index < _size) {
				Array.Copy(_items, index + count, _items, index, _size - index);
			}
			Array.Clear(_items, _size, count);
//			_version++;
		}
	}

	// Reverses the elements in this list.
	public void Reverse() {
		Reverse(0, Count);
	}

	// Reverses the elements in a range of this list. Following a call to this
	// method, an element in the range given by index and count
	// which was previously located at index i will now be located at
	// index index + (index + count - i - 1).
	//
	// This method uses the Array.Reverse method to reverse the
	// elements.
	//
	public void Reverse(int index, int count)
	{
		Array.Reverse(_items, index, count);
//		_version++;
	}

	// Sorts the elements in this list.  Uses the default comparer and
	// Array.Sort.
	public void Sort()
	{
		Sort(0, Count, null);
	}

	// Sorts the elements in this list.  Uses Array.Sort with the
	// provided comparer.
	public void Sort(IComparer<T> comparer)
	{
		Sort(0, Count, comparer);
	}

	// Sorts the elements in a section of this list. The sort compares the
	// elements to each other using the given IComparer interface. If
	// comparer is null, the elements are compared to each other using
	// the IComparable interface, which in that case must be implemented by all
	// elements of the list.
	//
	// This method uses the Array.Sort method to sort the elements.
	//
	public void Sort(int index, int count, IComparer<T> comparer) {
		Array.Sort<T>(_items, index, count, comparer);
//		_version++;
	}


	// Clears the contents of List.
	public void Clear()
	{
		if (_size > 0)
		{
			Array.Clear(_items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
			_size = 0;
		}
//		_version++;
	}


	public T[] ToArray()
	{
		T[] result = new T[_size];
		Array.Copy(_items, result, _size);
		return result;
	}


	public IEnumerator<T> GetEnumerator()
	{
		for(int i = 0; i < _size; i++)
			yield return _items[i];
	}


	/** Non-generic enumerator */
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
		for(int i = 0; i < _size; i++)
			yield return _items[i];
    }


	// Ensures that the capacity of this list is at least the given minimum
	// value. If the currect capacity of the list is less than min, the
	// capacity is increased to twice the current capacity or to min,
	// whichever is larger.
	private void EnsureCapacity(int min)
	{
		if (_items.Length < min)
		{
			int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;
			// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
//			if ((uint)newCapacity > Array.MaxArrayLength) newCapacity = Array.MaxArrayLength;
			if (newCapacity < min) newCapacity = min;
			Capacity = newCapacity;
		}
	}
}
