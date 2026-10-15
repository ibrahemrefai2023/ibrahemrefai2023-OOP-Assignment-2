using System.Diagnostics;
using PatternsLab.Problems.Builder;
using PatternsLab.Problems.Prototype;
using PatternsLab.Problems.Singleton;

Console.WriteLine("=== SINGLETON ===\n");
var db = new DatabaseService();
var ui = new UiService();
db.Config.Theme = "Dark";
ui.Render();
Console.WriteLine($"Same config object? {ReferenceEquals(db.Config, ui.Config)}");
Console.WriteLine($"Times config was loaded from disk: {AppConfig.LoadCount}");

Console.WriteLine("\n=== PROTOTYPE ===\n");
Enemy original = new Orc();
original.Name = "Boss Orc";
var watch = Stopwatch.StartNew();
Enemy copy = original.Clone();
Console.WriteLine($"Clone took {watch.ElapsedMilliseconds} ms");
Console.WriteLine($"Original model id: {original.ModelId}");
Console.WriteLine($"Copy model id:     {copy.ModelId}");
copy.Weapon.Damage = 999;
copy.Abilities.Add("Fire Breath");
Console.WriteLine($"Original weapon damage: {original.Weapon.Damage}");
Console.WriteLine($"Original abilities: {string.Join(", ", original.Abilities)}");

var registry = new EnemyPrototypeRegistry();
registry.Add("starter-orc", original);
Console.WriteLine($"Registry clone name: {registry.Create("starter-orc").Name}");

Console.WriteLine("\n=== BUILDER ===\n");
Console.WriteLine(RegistrationCallSites.CreateLiveStudent());
Console.WriteLine(RegistrationCallSites.CreateVideosOnlyStudent());
try
{
    CourseRegistration.Start("test@mail.com", "SEF-101").Build();
}
catch (Exception ex)
{
    Console.WriteLine($"Validation: {ex.Message}");
}
Console.ReadLine();
