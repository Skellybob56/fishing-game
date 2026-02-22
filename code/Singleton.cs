namespace FishingGame;

public abstract class Singleton<T> where T : Singleton<T>
{
    private static T? Instance;

    protected static T Register(T newInstance)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} instance already exists.");
        }
        Instance = newInstance;
        return Instance;
    }

    public static void Destroy()
    {
        Instance = null;
    }
}
