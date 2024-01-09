namespace ULSS_Helper.Objects;

internal abstract class Cache
{
    private DateTime CreatedAt { get; }
    internal DateTime ModifiedAt { get; private set; }

    internal Cache()
    {
        CreatedAt = DateTime.Now;
        ModifiedAt = CreatedAt;
    }

    internal void Update()
    {
        ModifiedAt = DateTime.Now;
    }
}
