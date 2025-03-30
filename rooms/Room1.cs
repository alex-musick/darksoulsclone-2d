using Godot;
using System;

public partial class Room1 : Node2D
{

    [Export] public int maxMobs {get; set;} = 50;
    [Export] public double difficultyTimeout {get; set;} = 30;
    [Export] public int difficultyStep {get; set;} = 10;
    [Export] public int hardMaxMobs {get; set;} = 300;
    private int mobCount = 0;
    private bool spawningAllowed = true;
    private double counter = 0;
    [Signal] public delegate void AllowSpawningEventHandler();
    [Signal] public delegate void BanSpawningEventHandler();

    private void OnMobSpawnerAiSpawned()
    {
        mobCount += 1;
    }

    private void OnMobSpawnerAiDestroyed()
    {
        mobCount -= 1;
    }

    private void SetSpawningState()
    {
        if (spawningAllowed && mobCount >= maxMobs)
        {
            GD.Print("Banning spawning (mob max met)");
            EmitSignal(SignalName.BanSpawning);
            spawningAllowed = false;
        }
        if (!spawningAllowed && mobCount < maxMobs)
        {
            EmitSignal(SignalName.AllowSpawning);
            GD.Print("Allowing spawning (mob max met)");
            spawningAllowed = true;
        }
    }

    private void SetMaxMobs()
    {
        if (counter >= difficultyTimeout && maxMobs < hardMaxMobs)
        {
            if (maxMobs + difficultyStep > hardMaxMobs)
            {
                maxMobs = hardMaxMobs;
                counter = 0;
                GD.Print("Hard max mobs has been set and reached.");
            }
            else
            {
                maxMobs += difficultyStep;
                counter = 0;
                GD.Print($"Increasing max mobs to {maxMobs}");
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        SetSpawningState();
        counter += delta;
        SetMaxMobs();
    }
}
