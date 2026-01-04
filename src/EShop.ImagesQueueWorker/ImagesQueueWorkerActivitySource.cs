using System.Diagnostics;

namespace EShop.ImagesQueueWorker;

public static class ImagesQueueWorkerActivitySource
{
    public const string SourceName = "EShop.ImagesQueueWorker";
    public static readonly ActivitySource ActivitySource = new(SourceName);
}