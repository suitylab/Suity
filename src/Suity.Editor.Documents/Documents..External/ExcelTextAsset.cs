using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Documents.External;

/// <summary>
/// Represents an Excel document asset that extracts text content from .xls and .xlsx files.
/// </summary>
public class ExcelTextAsset : TextAsset
{
    int? _pageCount;
    readonly List<string> _cachedStrings = [];
    /// <inheritdoc/>
    public override string GetText()
    {
        if (_cachedStrings.GetListItemSafe(0) is string t)
        {
            return t;
        }

        string text = LoadText(FileName, 0);
        return text;
    }

    /// <inheritdoc/>
    public override int PageCount
    {
        get
        {
            if (_pageCount is { } c)
            {
                return c;
            }

            return LoadPageCount(FileName);
        }
    }

    /// <inheritdoc/>
    public override string GetText(int pageIndex)
    {
        if (_cachedStrings.GetListItemSafe(pageIndex) is string t)
        {
            return t;
        }

        return LoadText(FileName, pageIndex);
    }


    /// <inheritdoc/>
    protected override void OnUpdated(EntryEventArgs args)
    {
        base.OnUpdated(args);

        _pageCount = null;
        _cachedStrings.Clear();
    }

    /// <inheritdoc/>
    protected override void OnAssetActivate(string assetKey)
    {
        base.OnAssetActivate(assetKey);

        _pageCount = null;
        _cachedStrings.Clear();
    }

    /// <inheritdoc/>
    protected override void OnAssetDeactivate(string assetKey)
    {
        base.OnAssetDeactivate(assetKey);

        _pageCount = null;
        _cachedStrings.Clear();
    }


    private int LoadPageCount(StorageLocation fileName)
    {
        if (_pageCount is { } c)
        {
            return c;
        }

        using var stroage = fileName.GetStorageItem();
        using var stream = stroage.GetInputStream();
        using var workbook = new XSSFWorkbook(stream);
        _pageCount = c = workbook.NumberOfSheets;

        return c;
    }

    private string LoadText(StorageLocation fileName, int index)
    {
        if (_cachedStrings.GetListItemSafe(index) is string t)
        {
            return t;
        }

        try
        {
            var sb = new StringBuilder();

            using (var stroage = fileName.GetStorageItem())
            using (var stream = stroage.GetInputStream())
            using (var workbook = new XSSFWorkbook(stream))
            {
                _pageCount = workbook.NumberOfSheets;
                _cachedStrings.EnsureListSize(workbook.NumberOfSheets, () => null);

                ISheet sheet = workbook.GetSheetAt(index); // Get first worksheet

                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow != null)
                    {
                        for (int col = 0; col < currentRow.LastCellNum; col++)
                        {
                            ICell currentCell = currentRow.GetCell(col);
                            if (currentCell != null)
                            {
                                sb.Append(currentCell.ToString().Replace(",", ""));
                            }
                            if (col < currentRow.LastCellNum - 1)
                            {
                                sb.Append(",");
                            }
                        }
                        sb.AppendLine();
                    }
                }
            }

            t = _cachedStrings[index] = sb.ToString();

            return t;
        }
        catch (Exception err)
        {
            err.LogError($"Load Excel document failed : {fileName}");

            return string.Empty;
        }
    }
}

/// <summary>
/// Activator for creating Excel document text assets.
/// </summary>
public class ExcelTextAssetActivator : AssetActivator
{
    private static readonly string[] _extensions = ["xls", "xlsx"];

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new ExcelTextAsset();
    }

    /// <inheritdoc/>
    public override string[] GetExtensions() => _extensions;
}
