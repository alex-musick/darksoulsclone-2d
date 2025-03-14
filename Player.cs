using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400; // How fast the player will move (pixels/sec).
    public int damage {get; private set;} = 1;
    private int health = 5;
    public enum FacingDirection 
    {
        up, down, left, right
    }
    [Export] public FacingDirection facingDirection = new();
    [Export] public bool isAttacking = false;
    public static Player instance;

    public Vector2 ScreenSize;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        instance = this;
    }
    public void attack(){
        var aniPlayer = GetNode<AnimationPlayer>("attackAnimation");
        
            aniPlayer.Play("attack");
    }
    private void _on_hit_box_area_entered(Area2D col) {
        GD.Print("damage taken");
    }
    // delta is time passed on screen
    public void physicsPlayer(double delta) 
    {
        Vector2 velocity = Vector2.Zero; 
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        var aniPlayerMoving = GetNode<AnimationPlayer>("walkAnimation");
        if (Input.IsActionJustPressed("Attack"))
        {
            isAttacking = true;
            if (facingDirection == FacingDirection.up)
            {
                attack();
            }
        }
        else if (direction != Vector2.Zero)
        {
            if (direction.X > 0)
            {
                aniPlayerMoving.Play("walkRight");
            }
            else if (direction.X < 0)
            {
                aniPlayerMoving.Play("walkLeft");

            }
            else if (direction.Y > 0)
            {
                aniPlayerMoving.Play("down");

            }
            else if (direction.Y < 0)
            {
                aniPlayerMoving.Play("up");

            }
            velocity = direction.Normalized() * Speed;
        } else if (!isAttacking)
        {
            aniPlayerMoving.Pause();
        }
        Velocity = velocity;
        MoveAndSlide();
        // else if (Input.IsActionPressed("move_right"))
        // {
        //     velocity.X += 1;
        // }

        // else if (Input.IsActionPressed("move_left"))
        // {
        //     velocity.X -= 1;
        // }

        // else if (Input.IsActionPressed("move_down"))
        // {
        //     velocity.Y += 1;
        // }

        // else if (Input.IsActionPressed("move_up"))
        // {
        //     velocity.Y -= 1;
        // }

        // var aniPlayerMoving = GetNode<AnimationPlayer>("walkAnimation");

        // if (velocity.Length() > 0)
        // {
        //     velocity = velocity.Normalized() * Speed;
        //     aniPlayerMoving.Play();
        // }
        // else
        // {
        //     aniPlayerMoving.Stop();
        // }
        // Position += velocity * (float)delta;
        // Velocity = velocity;
        // MoveAndSlide();
        // Position = new Vector2(
        //     x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
        //     y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
        // );
        // if (velocity.X < 0)
        // {
        //     aniPlayerMoving.Play("walkLeft");
        // }
        // else if (velocity.X > 0) 
        // {
        //     aniPlayerMoving.Play("walkRight");

        // }
        // else if (velocity.Y < 0)
        // {
        //     aniPlayerMoving.Play("up");
        // }
        // else if (velocity.Y > 0) 
        // {
        //     aniPlayerMoving.Play("down");
        // }
    }
    
    public override void _Process(double delta)
    {
        physicsPlayer(delta);
    
    }

    
}
