using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Models;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace Suity.Editor.AIGC;

/// <summary>
/// Represents a collection of AI models returned from the API.
/// </summary>
public class ModelList
{
    /// <summary>
    /// Gets or sets the list of models.
    /// </summary>
    [JsonProperty("models")]
    public List<Model> Models { get; set; }
}

/// <summary>
/// Helper class for managing API model lists and URL formatting.
/// </summary>
internal static class OkGoDoItHelper
{
    /// <summary>
    /// The filename used to store cached model lists.
    /// </summary>
    public const string MODEL_LIST_FILE = "model_list.json";

    /// <summary>
    /// Resolves the API URL format by ensuring the base URL ends with a trailing slash placeholder.
    /// </summary>
    /// <param name="baseUrl">The base URL to format.</param>
    /// <returns>The formatted URL string with placeholders.</returns>
    public static string ResolveApiUrlFormat(string baseUrl)
    {
        return baseUrl.TrimEnd('/') + "/{0}/{1}";
    }

    /// <summary>
    /// Loads a cached model list for the specified manufacturer.
    /// </summary>
    /// <param name="manufacture">The manufacturer identifier.</param>
    /// <returns>The list of models, or null if not found or an error occurs.</returns>
    public static List<Model> LoadModelList(string manufacture)
    {
        if (string.IsNullOrWhiteSpace(manufacture))
        {
            throw new ArgumentNullException(nameof(manufacture));
        }

        string listFilePath = GetManufactureDirectory(manufacture).PathAppend(MODEL_LIST_FILE);
        if (!File.Exists(listFilePath))
        {
            return null;
        }

        try
        {
            string content = File.ReadAllText(listFilePath);
            return JsonConvert.DeserializeObject<ModelList>(content).Models;
        }
        catch (Exception err)
        {
            err.LogError();

            return null;
        }
    }

    /// <summary>
    /// Saves a model list to the cache for the specified manufacturer.
    /// </summary>
    /// <param name="manufacture">The manufacturer identifier.</param>
    /// <param name="models">The list of models to save.</param>
    /// <returns>True if the save was successful; otherwise, false.</returns>
    public static bool SaveModelList(string manufacture, List<Model> models)
    {
        if (string.IsNullOrWhiteSpace(manufacture))
        {
            throw new ArgumentNullException(nameof(manufacture));
        }

        if (models is null)
        {
            throw new ArgumentNullException(nameof(models));
        }

        ModelList modeList = new ModelList() { Models = models };

        string dirPath = GetManufactureDirectory(manufacture);
        string listFilePath = dirPath.PathAppend(MODEL_LIST_FILE);

        try
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(listFilePath))
            {
                File.Delete(listFilePath);
            }

            File.WriteAllText(listFilePath, JsonConvert.SerializeObject(modeList, Formatting.Indented));
            return true;
        }
        catch (Exception err)
        {
            err.LogError();

            return false;
        }
    }

    /// <summary>
    /// Asynchronously downloads the available model list from the API.
    /// </summary>
    /// <param name="baseUrl">The API base URL.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <returns>The list of available models, or null if the request fails.</returns>
    public static async Task<List<Model>> DownloadModelList(string baseUrl, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return [];
        }

        APIAuthentication auth = apiKey;
        var api = new OpenAIAPI(auth);
        api.ApiUrlFormat = ResolveApiUrlFormat(baseUrl);

        try
        {
            var results = await api.Models.GetModelsAsync();
            return results;
        }
        catch (Exception ex)
        {
            ex.LogError();
            return [];
        }
    }

    /// <summary>
    /// Gets the directory path for storing manufacturer-specific data.
    /// </summary>
    /// <param name="manufacture">The manufacturer identifier.</param>
    /// <returns>The full directory path for the manufacturer.</returns>
    public static string GetManufactureDirectory(string manufacture)
    {
        return Project.Current.SystemDirectory.PathAppend("AIGC").PathAppend(manufacture);
    }
}