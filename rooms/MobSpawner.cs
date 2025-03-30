using Godot;
using System;

public partial class MobSpawner : Node2D
{
    [Signal] public delegate void AiSpawnedEventHandler();
    [Signal] public delegate void AiDestroyedEventHandler();
    private double counter = 0;
    private static Random random;
    private bool spawningAllowed = true;

    public override void _Process(double delta)
    {
        counter += delta;
        if (counter >= 5)
        {
            SpawnMob();
            counter = 0;
        }
    }
    private void SpawnMob()
    {
        if (!spawningAllowed)
        {
            GD.Print("MobSpawner: Not spawning mob because spawning is currently not allowed by parent room.");
            return;
        }
        var mob = ResourceLoader.Load<PackedScene>("res://ai_mob.tscn").Instantiate();
        // === These next few lines contain broken code that would slightly vary the spawn location of mobs. === //
        // === It's disabled because it breaks on the line that seets the progress ratio and I have on idea what's wrong. === //
        // GD.Print("1");
        // var spawnPoint = GetNode<PathFollow2D>("SpawnArea/SpawnLocation");
        // GD.Print("2");
        // spawnPoint.ProgressRatio = (float)random.NextDouble();
        // GD.Print("4");
        AddChild(mob);
        // mob.Connect("Spawned", new Callable(this, nameof(OnChildSpawned)));
        EmitSignal(SignalName.AiSpawned);
        mob.Connect("Destroyed", new Callable(this, nameof(OnChildDestroyed)));
        // GD.Print("5");
    }

    private void OnChildSpawned()
    {
        EmitSignal(SignalName.AiSpawned);
    }
    private void OnChildDestroyed()
    {
        EmitSignal(SignalName.AiDestroyed);
    }

    private void OnAllowMobSpawning()
    {
        spawningAllowed = true;
    }

    private void OnBanSpawning()
    {
        spawningAllowed = false;
    }
}
