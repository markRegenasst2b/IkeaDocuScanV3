namespace IkeaDocuScan_Web.Client.Models;

public class BulkDeleteProgress
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int PercentComplete => Total > 0 ? (int)((double)Completed / Total * 100) : 0;
}
