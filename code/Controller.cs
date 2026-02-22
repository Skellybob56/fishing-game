namespace FishingGame;

class Controller : Singleton<Controller>
{
    public static Controller Create()
    { return Register(new Controller()); }

    public void Update()
    {
        
    }
}
