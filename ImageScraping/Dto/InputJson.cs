namespace ImageScraping.Dto;

public class InputJson : IJson
{
    public int Count { get; set; }
    public int Parallelism { get; set; }
    public string SavePath { get; set; }

    public void CheckValues()
    {
        if (Count < Parallelism)
        {
            throw new Exception("Count can not be smaller than parallelism");
        }
        
        if (Count < 1 || Parallelism < 1)
        {
            throw new Exception("Count / parallelism should be more than zero");
        }

        if (string.IsNullOrWhiteSpace(SavePath))
        {
            throw new Exception("Save path can not be null or empty");
        }
    }
}