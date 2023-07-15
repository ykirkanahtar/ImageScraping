using ImageScraping.Dto;

namespace ImageScraping;

public class ImageScraper
{
    private readonly string _inputFolderName;
    private readonly string _defaultSavePath;
    private readonly string _url;
    private static bool _keepRunning = true;

    public ImageScraper(string inputFolderName, string defaultSavePath, string url)
    {
        _inputFolderName = inputFolderName;
        _defaultSavePath = defaultSavePath;
        _url = url;
    }

    public delegate void ProgressBarHandler(int count, int totalCount);

    public event ProgressBarHandler? ProgressEvent;

    public void Start()
    {
        Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _keepRunning = false;
        };

        FolderProcessor.CheckAndCreateFolder(_inputFolderName);

        var inputJson = SelectOption();

        FolderProcessor.CheckAndCreateFolder(inputJson.SavePath);

        try
        {
            FolderProcessor.ClearFolder(inputJson.SavePath);

            Console.WriteLine(
                $"Downloading {inputJson.Count} images ({inputJson.Parallelism} parallel downloads at most)");

            DownloadFiles(inputJson);

            Console.WriteLine("Finished");
        }
        catch (OperationCanceledException)
        {
            StopProcess(inputJson.SavePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error has occured: {e.Message}");
        }
    }

    private void DownloadFiles(InputJson inputJson)
    {
        var downloadedFileCount = 0;
        var parallelIndex = 1;
        var modZero = inputJson.Count % inputJson.Parallelism == 0;
        var lockObject = new object();

        do
        {
            var cts = new CancellationTokenSource();
            var options = new ParallelOptions
                { CancellationToken = cts.Token };

            Parallel.For(0, inputJson.Parallelism, options, (p, state) =>
            {
                if (!_keepRunning)
                {
                    state.Stop();
                    cts.Cancel();
                    return;
                }

                ImageDownloader.DownloadImage(_url, inputJson.SavePath, parallelIndex + p);
                lock (lockObject)
                {
                    IncrementProgress(downloadedFileCount += 1, inputJson.Count, ProgressEvent);
                }
            });

            parallelIndex += inputJson.Parallelism;
        } while (modZero
                     ? downloadedFileCount < inputJson.Count
                     : downloadedFileCount < inputJson.Count - inputJson.Parallelism);


        if (modZero == false)
        {
            var newIndex = inputJson.Count - downloadedFileCount;
            downloadedFileCount += 1;

            Parallel.For(0, newIndex,
                (p, _) =>
                {
                    if (!_keepRunning)
                    {
                        return;
                    }

                    ImageDownloader.DownloadImage(_url, inputJson.SavePath, downloadedFileCount + p);
                    lock (lockObject)
                    {
                        IncrementProgress(downloadedFileCount + p, inputJson.Count, ProgressEvent);
                    }
                });
        }
    }

    private InputJson SelectOption()
    {
        var inputJson = new InputJson
        {
            SavePath = _defaultSavePath
        };

        var option = string.Empty;
        do
        {
            Console.WriteLine("Please select an option..");
            Console.WriteLine("1: Configure settings with a json file");
            Console.WriteLine("2: Configure settings manually");

            option = Console.ReadLine();
        } while (option is "1" or "2" == false);

        if (option == "1")
        {
            var jsonFileIsValid = false;
            do
            {
                try
                {
                    inputJson = FolderProcessor.JsonToClass<InputJson>(_inputFolderName);
                    jsonFileIsValid = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"Warning: {e.Message} Please fix the error(s) and press any key to continue...");
                    Console.ReadLine();
                }
            } while (jsonFileIsValid == false);
        }
        else
        {
            var checkValues = false;
            do
            {
                try
                {
                    do
                    {
                        Console.WriteLine("Enter the number of images to download:");
                        var countKey = Console.ReadLine();
                        int.TryParse(countKey, out var count);
                        inputJson.Count = count;
                    } while (inputJson.Count > 0 == false);

                    do
                    {
                        Console.WriteLine("Enter the maximum parallel download limit:");
                        var parallelKey = Console.ReadLine();
                        int.TryParse(parallelKey, out var parallelism);
                        inputJson.Parallelism = parallelism;
                    } while (inputJson.Parallelism > 0 == false);

                    Console.WriteLine("Enter the save path (default: ./outputs):");
                    var keyPath = Console.ReadLine();
                    inputJson.SavePath = string.IsNullOrWhiteSpace(keyPath)
                        ? _defaultSavePath
                        : keyPath;

                    inputJson.CheckValues();
                    checkValues = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"An error has occured: {e.Message} Please press any key to retry...");
                    Console.ReadLine();
                }
            } while (checkValues == false);
        }

        return inputJson;
    }

    private static void IncrementProgress(int progress, int totalCount, ProgressBarHandler? eventHandler)
    {
        eventHandler?.Invoke(progress, totalCount);
    }

    private static void StopProcess(string folderName)
    {
        if (_keepRunning) return;
        if (FolderProcessor.GetFilesCount(folderName) <= 0) return;
        Console.WriteLine("The process has been stopped");
        FolderProcessor.ClearFolder(folderName);
    }
}