using Suity.NodeQuery;
using System;
using System.Drawing;

namespace Suity.Views;

[Flags]
public enum ViewPropertyFlags
{
    None = 0,

    Expand = 1 << 0,
    WriteBack = 1 << 1,
    ReadOnly = 1 << 2,
    Disabled = 1 << 3,
    Optional = 1 << 4,
    IsAbstract = 1 << 5,
    IsConnector = 1 << 6,
    HideTitle = 1 << 7,
    Navigation = 1 << 8,
}

[Serializable]
public sealed class ViewProperty
{
    private ViewPropertyFlags _flags;

    /// <summary>
    /// Specify the editor to hide fields of the specified type
    /// </summary>
    public const string HiddenFieldTypeAttribute = "HiddenFieldType";

    /// <summary>
    /// Specify editor to hide connector
    /// </summary>
    public const string HideConnectorAttribute = "HideConnector";

    /// <summary>
    /// Specify editor to hide link type field
    /// </summary>
    public const string HideDataLinkAttribute = "HideDataLink";


    public string Name { get; }
    public string Description { get; set; }
    public object Icon { get; set; }

    public int ViewId { get; set; }

    public TextStatus Status { get; set; }

    public bool Expand
    {
        get => _flags.HasFlag(ViewPropertyFlags.Expand);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.Expand;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.Expand;
            }
        }
    }

    public bool WriteBack
    {
        get => _flags.HasFlag(ViewPropertyFlags.WriteBack);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.WriteBack;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.WriteBack;
            }
        }
    }

    public bool ReadOnly
    {
        get => _flags.HasFlag(ViewPropertyFlags.ReadOnly);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.ReadOnly;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.ReadOnly;
            }
        }
    }

    public bool Disabled
    {
        get => _flags.HasFlag(ViewPropertyFlags.Disabled);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.Disabled;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.Disabled;
            }
        }
    }

    public bool Optional
    {
        get => _flags.HasFlag(ViewPropertyFlags.Optional);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.Optional;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.Optional;
            }
        }
    }

    public bool IsAbstract
    {
        get => _flags.HasFlag(ViewPropertyFlags.IsAbstract);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.IsAbstract;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.IsAbstract;
            }
        }
    }

    public bool IsConnector
    {
        get => _flags.HasFlag(ViewPropertyFlags.IsConnector);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.IsConnector;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.IsConnector;
            }
        }
    }

    public bool HideTitle
    {
        get => _flags.HasFlag(ViewPropertyFlags.HideTitle);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.HideTitle;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.HideTitle;
            }
        }
    }

    public bool Navigation
    {
        get => _flags.HasFlag(ViewPropertyFlags.Navigation);
        set
        {
            if (value)
            {
                _flags |= ViewPropertyFlags.Navigation;
            }
            else
            {
                _flags &= ~ViewPropertyFlags.Navigation;
            }
        }
    }

    public Color? Color { get; set; }

    public INodeReader Styles { get; set; }

    public IAttributeGetter Attributes { get; set; }

    public string DisplayName => !string.IsNullOrEmpty(Description) ? Description : Name;

    public ViewProperty()
    {
        Name = string.Empty;
    }

    public ViewProperty(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
    }

    public ViewProperty(string name, string description)
        : this(name)
    {
        Description = description;
    }

    public ViewProperty(string name, string description, object icon)
        : this(name, description)
    {
        Icon = icon;
    }

    public static implicit operator ViewProperty(string name)
    {
        return new ViewProperty(name);
    }
}