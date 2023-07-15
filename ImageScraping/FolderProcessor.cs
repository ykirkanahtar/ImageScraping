using System.Text.Json;
using ImageScraping.Dto;

namespace ImageScraping;

public static class FolderProcessor
{
    public static void CheckAndCreateFolder(string folderName)
    {
        if (Directory.Exists(folderName) == false)
        {
            Directory.CreateDirectory(folderName);
        }
    }

    public static void ClearFolder(string folderName)
    {
        if (!Directory.Exists(folderName)) return;
        var files = Directory.GetFiles(folderName);
        Parallel.ForEach(files, File.Delete);
    }

    public static int GetFilesCount(string folderName)
    {
        return !Directory.Exists(folderName) ? 0 : Directory.GetFiles(folderName).Length;
    }

    public static T JsonToClass<T>(string folderName) where T : IJson
    {
        const string extension = ".json";
        
        var files = Directory.GetFiles(folderName);

        if (files.Any(p => p.EndsWith(extension)) == false)
        {
            throw new Exception("Not found any json file");
        }
        
        if (files.Count(p => p.EndsWith(extension)) > 1)
        {
            throw new Exception("Found multiple json files");
        }

        var jsonFile = files.Single(p => p.EndsWith(extension));

        try
        {
            using var file = new StreamReader(Path.Combine(folderName, Path.GetFileName(jsonFile)));
            var jsonText = file.ReadToEnd();
            var input = JsonSerializer.Deserialize<T>(jsonText);
            if (input == null)
            {
                throw new JsonException();
            }
            
            input.CheckValues();
            return input;
        }
        catch (JsonException)
        {
            throw new Exception("Invalid json file");
        }
    }
}