using Godot;
using System;

public partial class Dummy : Node2D
{
    public override void _Ready()
    {
        var name = "Godot";
        GD.PrintS("Hello", name);
    }
}
