using System;
using System.Xml;

namespace MameTools.Net48.Helpers;
internal class XmlHelper
{

    public static void ReadElementsUntilEnd(XmlReader reader, Action<XmlReader> onElement)
    {
        // Se il nodo è vuoto, non c'è nulla da leggere
        if (reader.IsEmptyElement)
            return;

        // Crea un sotto-reader limitato al nodo corrente
        using var subReader = reader.ReadSubtree();
        // Sposta il sotto-reader sull'elemento radice
        subReader.Read();

        while (subReader.Read())
        {
            if (subReader.NodeType == XmlNodeType.EndElement &&
                subReader.LocalName.Equals(reader.LocalName, StringComparison.OrdinalIgnoreCase))
                break;

            if (!subReader.IsStartElement())
                continue;

            onElement(subReader);
        }
    }

}
