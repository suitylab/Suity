using System;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Attribute to mark classes that support direct instance asset access.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class InstanceAssetAccessAttribute : Attribute
{
}