namespace ImageScraping;

class Program
{
    private const string Url = "https://picsum.photos/200/300";
    private const string InputFolderName = "input";
    private const string DefaultOutputFolderName = "outputs";

    static void Main(string[] args)
    {
        var imageScraper = new ImageScraper(InputFolderName, DefaultOutputFolderName, Url);
        imageScraper.ProgressEvent += ShowProgress;
        imageScraper.Start();
        imageScraper.ProgressEvent -= ShowProgress;
    }

    private static void ShowProgress(int progressValue, int totalCount)
    {
        Console.Write("\rProgress: {0}/{1}  ", progressValue, totalCount);
    }
}