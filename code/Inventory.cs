namespace FishingGame;

class Inventory : Singleton<Inventory>
{
	public static Inventory Create()
	{ return Register(new Inventory()); }

	private Inventory() {}

	public int fishCount { get; private set; } = 0;

	public void IncrementFishCount()
	{
		fishCount++;
		Console.WriteLine($"Fish acquired! Current fish count: {fishCount}");
	}
}
