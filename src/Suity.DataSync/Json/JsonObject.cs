using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ComputerBeacon.Json;

/// <summary>
/// JsonObject class
/// </summary>
[Serializable]
public class JsonObject : IJsonContainer, IDictionary<string, object>
{
    /// <summary>
    /// The properties of this JSON Object
    /// </summary>

    private readonly Dictionary<string, object> entries;

    #region Constructor

    /// <summary>
    /// Creates an empty JsonObject
    /// </summary>
    public JsonObject()
    {
        entries = [];
    }

    /// <summary>
    /// Create a new JsonObject from a string
    /// </summary>
    /// <param name="jsonString">JSON string that represents an object</param>
    /// <exception cref="FormatException">JsonString represents JsonArray instead of JsonObject</exception>
    public JsonObject(string jsonString)
    {
        var jo = Parser.Parse(jsonString) as JsonObject;
        if (jo == null) throw new FormatException("JsonString represents JsonArray instead of JsonObject");
        this.entries = jo.entries;
    }

    /// <summary>
    /// Creates a JsonObject with initial string
    /// </summary>
    /// <param name="values">Annoymous type containing initial values</param>
    public JsonObject(object values) : this()
    {
        foreach (var p in values.GetType().GetProperties())
        {
            if (!p.CanRead) continue;
            this.Add(p.Name, p.GetValue(values, null));
        }
    }

    #endregion

    #region IJsonContainer

    void IJsonContainer.InternalAdd(string key, object value)
    {
        entries.Add(key, value);
    }

    bool IJsonContainer.IsArray => false;

    #endregion

    #region Indexer

    /// <summary>
    /// Gets a property of the current JSON Object by key
    /// </summary>
    /// <param name="key">Key of property</param>
    /// <returns>Value of property. Returns null if property is not found.</returns>
    public object this[string key]
    {
        get
        {
            if (entries.TryGetValue(key, out var value)) return value;
            return null;
        }
        set
        {
            Helper.AssertValidType(value);
            entries[key] = value;
        }
    }

    #endregion

    #region Interface

    /// <summary>
    /// The number of key/value pairs contained in the JsonObject
    /// </summary>
    public int Count => entries.Count;

    /// <summary>
    /// Whether the JsonObject is read-only. This value is always true.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// All the keys in the JsonObject
    /// </summary>
    public ICollection<string> Keys => entries.Keys;

    /// <summary>
    /// All the values in the JsonObject
    /// </summary>
    public ICollection<object> Values => entries.Values;

    /// <summary>
    /// Adds the specified key and value to the JsonObject.
    /// </summary>
    /// <param name="item"></param>
    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        Helper.AssertValidType(item.Value);
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Adds the specified key and value to the JsonObject.
    /// </summary>
    /// <param name="key">Key of entry</param>
    /// <param name="value">Value of entry</param>
    public void Add(string key, object value)
    {
        Helper.AssertValidType(value);
        entries.Add(key, value);
    }

    /// <summary>
    /// Removes all keys and values from the JsonObject.
    /// </summary>
    public void Clear()
    { entries.Clear(); }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes the item with the specified key from the JsonObject.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(string key)
    { return entries.Remove(key); }

    /// <summary>
    /// Copy all the entries to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        int i = 0;
        foreach (KeyValuePair<string, object> KVP in entries)
        {
            array[arrayIndex + (i++)] = KVP;
        }
    }

    /// <summary>
    /// Determines whether the JsonObject contains the specified key/value pair.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<string, object> item)
    {
        return entries.ContainsKey(item.Key) && entries[item.Key].Equals(item.Value);
    }

    /// <summary>
    /// Determines whether the JsonObject contains the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key) => entries.ContainsKey(key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out object value)
    {
        return entries.TryGetValue(key, out value);
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        return entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return entries.GetEnumerator();
    }

    #endregion

    #region ToString

    /// <summary>
    /// Returns the shortest string representation of the current JsonObject
    /// </summary>
    /// <returns>A string</returns>
    public override string ToString()
    {
        return ToString(true);
    }

    /// <summary>
    /// Returns the string representation of the current JsonObject
    /// </summary>
    /// <param name="niceFormat">Whether the string is formatted for easy reading</param>
    /// <returns>A string representation of the current JsonObject</returns>
    public string ToString(bool niceFormat)
    {
        CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var sb = new StringBuilder();
        Stringifier.stringify(this, sb, 0, niceFormat);

        System.Threading.Thread.CurrentThread.CurrentCulture = culture;

        return sb.ToString();
    }

    #endregion
}