using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Configuration for target .NET frameworks
/// </summary>
public class TargetFrameworkConfig : IViewObject
{
    /// <summary>
    /// Default netstandard framework name
    /// </summary>
    public const string Name_NetStandardDefault = "netstandard2.0";

    /// <summary>
    /// .NET Standard 1.3 framework name
    /// </summary>
    public const string Name_NetStandard13 = "netstandard1.3";
    /// <summary>
    /// .NET Standard 2.0 framework name
    /// </summary>
    public const string Name_NetStandard20 = "netstandard2.0";
    /// <summary>
    /// .NET Standard 2.1 framework name
    /// </summary>
    public const string Name_NetStandard21 = "netstandard2.1";
    /// <summary>
    /// .NET Framework 2.0 name
    /// </summary>
    public const string Name_Net2 = "net2";
    /// <summary>
    /// .NET Framework 3.5 name
    /// </summary>
    public const string Name_Net35 = "net35";
    /// <summary>
    /// .NET Framework 4.6.1 name
    /// </summary>
    public const string Name_Net461 = "net461";
    /// <summary>
    /// .NET Framework 4.7.2 name
    /// </summary>
    public const string Name_Net472 = "net472";
    /// <summary>
    /// .NET Framework 4.8 name
    /// </summary>
    public const string Name_Net48 = "net48";
    /// <summary>
    /// .NET 5.0 name
    /// </summary>
    public const string Name_Net5 = "net5";
    /// <summary>
    /// .NET 6.0 name
    /// </summary>
    public const string Name_Net6 = "net6";

    private bool _netStandard13;
    private bool _netStandard20;
    private bool _netStandard21;
    private bool _net2;
    private bool _net35;
    private bool _net461;
    private bool _net472;
    private bool _net48;
    private bool _net5;
    private bool _net6;

    private readonly string _fixedFramework;

    private readonly List<string> _list = [];
    private string _code;

    /// <summary>
    /// Event raised when a framework value changes
    /// </summary>
    public event EventHandler ValueChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="TargetFrameworkConfig"/>
    /// </summary>
    public TargetFrameworkConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance with a fixed framework
    /// </summary>
    /// <param name="fixedFramework">The fixed framework name that cannot be changed</param>
    public TargetFrameworkConfig(string fixedFramework)
    {
        _fixedFramework = fixedFramework;

        switch (fixedFramework)
        {
            case Name_NetStandard13:
                _netStandard13 = true;
                break;

            case Name_NetStandard20:
                _netStandard20 = true;
                break;

            case Name_NetStandard21:
                _netStandard21 = true;
                break;

            case Name_Net2:
                _net2 = true;
                break;

            case Name_Net35:
                _net35 = true;
                break;

            case Name_Net461:
                _net461 = true;
                break;

            case Name_Net472:
                _net472 = true;
                break;

            case Name_Net48:
                _net48 = true;
                break;

            case Name_Net5:
                _net5 = true;
                break;

            case Name_Net6:
                _net6 = true;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Gets the fixed framework if set, otherwise null
    /// </summary>
    public string FixedFramework => _fixedFramework;

    /// <summary>
    /// Gets or sets whether .NET Standard 1.3 is targeted
    /// </summary>
    public bool NetStandard13
    {
        get => _fixedFramework == Name_NetStandard13 ? true : _netStandard13;
        set => SetValue(value, Name_NetStandard13, ref _netStandard13);
    }

    /// <summary>
    /// Gets or sets whether .NET Standard 2.0 is targeted
    /// </summary>
    public bool NetStandard20
    {
        get => _fixedFramework == Name_NetStandard20 ? true : _netStandard20;
        set => SetValue(value, Name_NetStandard20, ref _netStandard20);
    }

    /// <summary>
    /// Gets or sets whether .NET Standard 2.1 is targeted
    /// </summary>
    public bool NetStandard21
    {
        get => _fixedFramework == Name_NetStandard21 ? true : _netStandard21;
        set => SetValue(value, Name_NetStandard21, ref _netStandard21);
    }

    /// <summary>
    /// Gets or sets whether .NET Framework 2.0 is targeted
    /// </summary>
    public bool Net2
    {
        get => _fixedFramework == Name_Net2 ? true : _net2;
        set => SetValue(value, Name_Net2, ref _net2);
    }

    /// <summary>
    /// Gets or sets whether .NET Framework 3.5 is targeted
    /// </summary>
    public bool Net35
    {
        get => _fixedFramework == Name_Net35 ? true : _net35;
        set => SetValue(value, Name_Net35, ref _net35);
    }

    /// <summary>
    /// Gets or sets whether .NET Framework 4.6.1 is targeted
    /// </summary>
    public bool Net461
    {
        get => _fixedFramework == Name_Net461 ? true : _net461;
        set => SetValue(value, Name_Net461, ref _net461);
    }

    /// <summary>
    /// Gets or sets whether .NET Framework 4.7.2 is targeted
    /// </summary>
    public bool Net472
    {
        get => _fixedFramework == Name_Net472 ? true : _net472;
        set => SetValue(value, Name_Net472, ref _net472);
    }

    /// <summary>
    /// Gets or sets whether .NET Framework 4.8 is targeted
    /// </summary>
    public bool Net48
    {
        get => _fixedFramework == Name_Net48 ? true : _net48;
        set => SetValue(value, Name_Net48, ref _net48);
    }

    /// <summary>
    /// Gets or sets whether .NET 5.0 is targeted
    /// </summary>
    public bool Net5
    {
        get => _fixedFramework == Name_Net5 ? true : _net5;
        set => SetValue(value, Name_Net5, ref _net5);
    }

    /// <summary>
    /// Gets or sets whether .NET 6.0 is targeted
    /// </summary>
    public bool Net6
    {
        get => _fixedFramework == Name_Net6 ? true : _net6;
        set => SetValue(value, Name_Net6, ref _net6);
    }

    #region IViewObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        NetStandard13 = sync.Sync(nameof(NetStandard13), NetStandard13);
        NetStandard20 = sync.Sync(nameof(NetStandard20), NetStandard20);
        NetStandard21 = sync.Sync(nameof(NetStandard21), NetStandard21);
        Net2 = sync.Sync(nameof(Net2), Net2);
        Net35 = sync.Sync(nameof(Net35), Net35);
        Net461 = sync.Sync(nameof(Net461), Net461);
        Net472 = sync.Sync(nameof(Net472), Net472);
        Net48 = sync.Sync(nameof(Net48), Net48);
        Net5 = sync.Sync(nameof(Net5), Net5);
        Net6 = sync.Sync(nameof(Net6), Net6);
    }

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        setup.AllInspectorField(this);
    }

    #endregion

    /// <summary>
    /// Gets the minimal .NET Standard framework selected
    /// </summary>
    /// <returns>The minimal .NET Standard framework name, or null if none selected</returns>
    public string GetMinimalNetStandard()
    {
        if (NetStandard13)
        {
            return Name_NetStandard13;
        }
        if (NetStandard20)
        {
            return Name_NetStandard20;
        }
        if (NetStandard21)
        {
            return Name_NetStandard21;
        }

        return null;
    }

    /// <summary>
    /// Gets the array of selected framework names
    /// </summary>
    /// <returns>Array of framework names</returns>
    public string[] GetFrameworks()
    {
        if (_code is null)
        {
            Update();
        }

        return _list.ToArray();
    }

    /// <summary>
    /// Returns a semicolon-separated string of selected frameworks
    /// </summary>
    public override string ToString()
    {
        if (_code is null)
        {
            Update();
        }

        return _code;
    }

    private void SetValue(bool value, string framework, ref bool targetValue)
    {
        if (_fixedFramework == framework)
        {
            return;
        }

        if (targetValue != value)
        {
            targetValue = value;

            OnChanged();
        }
    }

    private void OnChanged()
    {
        _code = null;

        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        _list.Clear();

        if (_fixedFramework != null)
        {
            _list.Add(_fixedFramework);
        }

        if (_netStandard13)
        {
            AddFramework(Name_NetStandard13);
        }
        if (_netStandard20)
        {
            AddFramework(Name_NetStandard20);
        }
        if (_netStandard21)
        {
            AddFramework(Name_NetStandard21);
        }
        if (_net2)
        {
            AddFramework(Name_Net2);
        }
        if (_net35)
        {
            AddFramework(Name_Net35);
        }
        if (_net461)
        {
            AddFramework(Name_Net461);
        }
        if (_net472)
        {
            AddFramework(Name_Net472);
        }
        if (_net48)
        {
            AddFramework(Name_Net48);
        }
        if (_net5)
        {
            AddFramework(Name_Net5);
        }
        if (_net6)
        {
            AddFramework(Name_Net6);
        }

        if (_list.Count == 0)
        {
            AddFramework(Name_NetStandardDefault);
        }

        _code = string.Join(";", _list);
    }

    private void AddFramework(string framework)
    {
        if (framework != _fixedFramework)
        {
            _list.Add(framework);
        }
    }
}