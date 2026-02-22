namespace FishingGame;

class Controller
{
    private static Controller? Instance;

    public static Controller Create()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Controller already exists.");
        }
        Instance = new Controller();

        return Instance;
    }

    public static void Destroy()
    {
        Instance = null;
    }

    public void Update()
    {
        
    }
}
