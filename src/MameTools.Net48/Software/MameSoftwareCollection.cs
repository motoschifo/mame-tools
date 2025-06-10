#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using MameTools.Net48.Software;

namespace MameTools.Net48.SoftwareList;

public class MameSoftwareCollection : ICollection<MameSoftware>
{
    private readonly List<MameSoftware> _list = [];
    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public void Add(MameSoftware item)
    {
        _list.Add(item);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(MameSoftware item) => _list.Contains(item);
    public void CopyTo(MameSoftware[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public IEnumerator<MameSoftware> GetEnumerator() => _list.GetEnumerator();
    public bool Remove(MameSoftware item)
    {
        // ATTENZIONE
        // Per poter rimuovere un elemento occorrerebbe fare tutti i calcoli a ritroso
        // e ripristinare così i totali.
        // Per il momento evito e restituisco un errore in fase di rimozione
        throw new NotImplementedException();
    }
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}
