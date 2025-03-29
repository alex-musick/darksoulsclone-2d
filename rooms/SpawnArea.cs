using Godot;
using System;

public partial class SpawnArea : Path2D
{
    private double counter = 0;
    private static Random random;

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
        var mob = ResourceLoader.Load<PackedScene>("res://ai_mob.tscn").Instantiate();
        GD.Print("1");
        var spawnPoint = GetNode<PathFollow2D>("SpawnArea/SpawnLocation");
        GD.Print("2");
        // spawnPoint.ProgressRatio = (float)random.NextDouble();
        GD.Print("4");
        AddChild(mob);
        GD.Print("5");
    }
}
