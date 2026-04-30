using System;
using System.IO;
using System.Xml;

namespace Suity.NodeQuery;

/// <summary>
/// INodeWriter implementation for writing XML documents
/// </summary>
public class XmlNodeWriter : MarshalByRefObject, INodeWriter
{
    readonly XmlDocument _doc = new();

    XmlElement _currentElement;

    public XmlNodeWriter(string rootName, bool decalaration = true)
    {
        if (decalaration)
        {
            XmlDeclaration xmlDeclaration = _doc.CreateXmlDeclaration("1.0", "utf-8", null);
            _doc.AppendChild(xmlDeclaration);
        }

        _doc.AppendChild(_doc.CreateElement(rootName));
        _currentElement = _doc.DocumentElement;
    }

    public void BeginElement(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();

        XmlElement newElement = _doc.CreateElement(name);
        _currentElement.AppendChild(newElement);
        _currentElement = newElement;
    }
    public void AddElement(string name, string innerText)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();

        XmlElement newElement = _doc.CreateElement(name);
        _currentElement.AppendChild(newElement);
        newElement.InnerText = innerText;
    }
    public void SetElement(string name, Action<INodeWriter> action)
    {
        BeginElement(name);

        try
        {
            action(this);
        }
        finally
        {
            EndElement(name);
        }
    }

    public void AddArrayItem(Action<INodeWriter> action)
    {
        BeginElement("Item");

        try
        {
            action(this);
        }
        finally
        {
            EndElement("Item");
        }
    }

    public void SetValue(string value)
    {
        _currentElement.InnerText = value;
    }
    public void SetValueObj(object value)
    {
        _currentElement.InnerText = value?.ToString();
    }
        
    public void EndElement(string name)
    {
        if (_currentElement.Name != name) throw new InvalidOperationException();

        _currentElement = _currentElement.ParentNode as XmlElement;
    }
    public void EndElement()
    {
        _currentElement = _currentElement.ParentNode as XmlElement;
    }
    public void RevertElement()
    {
        if (_currentElement == _doc.DocumentElement) return;

        XmlElement e = _currentElement;
        _currentElement = _currentElement.ParentNode as XmlElement;
        _currentElement?.RemoveChild(e);
    }

    public void SetAttribute(string name, object valueToString)
    {
        _currentElement.SetAttribute(name, valueToString != null ? valueToString.ToString() : "");
    }

    public int ChildNodeCount => _currentElement.ChildNodes.Count;
    public int AttributeCount => _currentElement.Attributes.Count;
    public string InnerText => _currentElement.InnerText;
    public bool IsEmptyElement => ChildNodeCount == 0 && AttributeCount == 0 && string.IsNullOrEmpty(InnerText);

    public void SaveToFile(string path)
    {
        if (_currentElement != _doc.DocumentElement)
        {
            throw new InvalidOperationException("Document is not ended.");
        }

        _doc.Save(path);
    }
    public void SaveToStream(Stream stream)
    {
        if (_currentElement != _doc.DocumentElement)
        {
            throw new InvalidOperationException("Document is not ended.");
        }

        _doc.Save(stream);
    }

    public XmlDocument GetDocument() => _doc;

    public override string ToString()
    {
        var writer = new StringWriter();
        _doc.Save(writer);

        return writer.ToString();
    }

}
