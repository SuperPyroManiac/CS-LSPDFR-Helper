namespace ULSS_Helper.Objects;

public abstract class Cache
{
    private DateTime CreatedAt { get; }
    public DateTime ModifiedAt { get; private set; }

    public Cache()
    {
        CreatedAt = DateTime.Now;
        ModifiedAt = CreatedAt;
    }

    public void Update()
    {
        ModifiedAt = DateTime.Now;
    }
}
