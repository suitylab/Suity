using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Suity.NodeQuery;

/// <summary>
/// INodeReader implementation for reading XML elements
/// </summary>
public class XmlNodeReader(XmlElement element) : MarshalByRefObject, INodeReader
{
    public delegate T ValueParser<T>(string str, T defaultValue);

    readonly XmlElement _element = element ?? throw new ArgumentNullException(nameof(element));

    public bool Exist => true;

    public string NodeName => _element.Name;

    public int ChildCount => _element.ChildNodes.OfType<XmlElement>().Count();

    public string NodeValue => (_element.FirstChild as XmlText)?.Value;

    public object NodeValueObj => (_element.FirstChild as XmlText)?.Value;

    public INodeReader Node(int index)
    {
        if (index < _element.ChildNodes.Count)
        {
            if (_element.ChildNodes[index] is XmlElement child)
            {
                return new XmlNodeReader(child);
            }
        }

        return EmptyNodeReader.Empty;
    }
    public INodeReader Node(string name)
    {
        if (_element != null)
        {
            var child = _element.ChildNodes.OfType<XmlElement>().FirstOrDefault(o => o.Name == name);
            if (child != null)
            {
                return new XmlNodeReader(child);
            }
        }

        return EmptyNodeReader.Empty;
    }
    public IEnumerable<INodeReader> Nodes(string name)
    {
        return _element.ChildNodes
            .OfType<XmlElement>()
            .Where(o => o.Name == name)
            .Select(o => new XmlNodeReader(o))
            .OfType<INodeReader>();
    }
    public IEnumerable<INodeReader> Nodes()
    {
        return _element.ChildNodes
            .OfType<XmlElement>()
            .Select(o => new XmlNodeReader(o))
            .OfType<INodeReader>();
    }
    public IEnumerable<string> NodeNames 
        => _element.ChildNodes
            .OfType<XmlElement>()
            .Select(o => o.Name);

    public IEnumerable<KeyValuePair<string, string>> Attributes
    {
        get
        {
            for (int i = 0; i < _element.Attributes.Count; i++)
            {
                var attr = _element.Attributes[i];
                yield return new KeyValuePair<string, string>(attr.Name, attr.Value);
            }
        }
    }
    public string GetAttribute(string name)
    {
        return _element.GetAttributeNode(name)?.Value;
    }

    public override string ToString()
    {

        return _element?.OuterXml ?? string.Empty;
    }


    public static INodeReader FromDocument(XmlDocument doc)
    {
        return new XmlNodeReader(doc.DocumentElement);
    }
    public static INodeReader FromFile(string fileName, bool safe = true)
    {
        var doc = new XmlDocument();

        try
        {
            doc.Load(fileName);
            return new XmlNodeReader(doc.DocumentElement);
        }
        catch (FileNotFoundException)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
        catch (Exception)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
    }
    public static INodeReader FromStream(Stream stream, bool safe = true)
    {
        if (stream is null)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw new ArgumentNullException(nameof(stream));
            }
        }

        var doc = new XmlDocument();
        try
        {
            doc.Load(stream);
            return new XmlNodeReader(doc.DocumentElement);
        }
        catch (FileNotFoundException)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
        catch (Exception)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
    }
    public static INodeReader FromXml(string xml, bool safe = true)
    {
        if (string.IsNullOrEmpty(xml))
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw new ArgumentNullException(nameof(xml));
            }
        }

        var doc = new XmlDocument();
        try
        {
            doc.LoadXml(xml);
            return new XmlNodeReader(doc.DocumentElement);
        }
        catch (Exception)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
    }
}