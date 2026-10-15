namespace PatternsLab.Problems.Prototype;

public class Weapon
{
    public string Name { get; set; } = "";
    public int Damage { get; set; }

    public Weapon Clone()
    {
        return new Weapon { Name = Name, Damage = Damage };
    }
}

public abstract class Enemy
{
    private string _modelData;

    public string Name { get; set; } = "";
    public int Health { get; set; }
    public Weapon Weapon { get; set; } = new Weapon();
    public List<string> Abilities { get; set; } = new List<string>();
    public string ModelId => _modelData;

    protected Enemy()
    {
        Console.WriteLine("   ...loading 3D model (slow)...");
        Thread.Sleep(500);
        _modelData = "MODEL_" + Guid.NewGuid().ToString("N")[..6];
    }

    protected Enemy(Enemy other)
    {
        _modelData = other._modelData;
        Name = other.Name;
        Health = other.Health;
        Weapon = other.Weapon.Clone();
        Abilities = new List<string>(other.Abilities);
    }

    public abstract Enemy Clone();
}

public class Orc : Enemy
{
    public Orc()
    {
        Name = "Orc";
        Health = 100;
        Weapon = new Weapon { Name = "Axe", Damage = 25 };
        Abilities.Add("Rage");
    }

    private Orc(Orc other) : base(other)
    {
    }

    public override Enemy Clone()
    {
        return new Orc(this);
    }
}

public class Elf : Enemy
{
    public Elf()
    {
        Name = "Elf";
        Health = 70;
        Weapon = new Weapon { Name = "Bow", Damage = 18 };
        Abilities.Add("Stealth");
    }

    private Elf(Elf other) : base(other)
    {
    }

    public override Enemy Clone()
    {
        return new Elf(this);
    }
}

public class EnemyPrototypeRegistry
{
    private readonly Dictionary<string, Enemy> _prototypes = new Dictionary<string, Enemy>();

    public void Add(string name, Enemy enemy)
    {
        _prototypes[name] = enemy;
    }

    public Enemy Create(string name)
    {
        if (!_prototypes.ContainsKey(name))
        {
            throw new InvalidOperationException("Prototype was not found.");
        }

        return _prototypes[name].Clone();
    }
}
