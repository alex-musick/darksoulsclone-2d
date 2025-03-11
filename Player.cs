using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400; // How fast the player will move (pixels/sec).

    public Vector2 ScreenSize;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
    }
    public override void _Process(double delta)
    {
        movePlayer(delta);

        if (Input.IsActionPressed("Attack"))
        {
            attack();
        }
    }
    public void attack(){
        var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        
        if (!animatedSprite2D.IsPlaying()) {
            animatedSprite2D.Animation = "attack";
            animatedSprite2D.Play();
        }
    }
    //delta is time passed on screen
    public void movePlayer(double delta) 
    {
        var velocity = Vector2.Zero; 

        if (Input.IsActionPressed("move_right"))
        {
            velocity.X += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            velocity.X -= 1;
        }

        if (Input.IsActionPressed("move_down"))
        {
            velocity.Y += 1;
        }

        if (Input.IsActionPressed("move_up"))
        {
            velocity.Y -= 1;
        }

        var animatedSprite2D = GetNode<AnimatedSprite2D>("walkAnimation");

        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * Speed;
            animatedSprite2D.Play();
        }
        else
        {
            animatedSprite2D.Stop();
        }
        // Position += velocity * (float)delta;
        Velocity = velocity;
        MoveAndSlide();
        Position = new Vector2(
            x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
            y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
        );
        if (velocity.X != 0)
        {
            animatedSprite2D.Animation = "walk";
            animatedSprite2D.FlipV = false;
            animatedSprite2D.FlipH = velocity.X < 0;
        }
        else if (velocity.Y != 0)
        {
            animatedSprite2D.Animation = "up";
            animatedSprite2D.FlipV = velocity.Y > 0;
        }
    }
}
