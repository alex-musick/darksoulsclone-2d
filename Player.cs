using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 75; // How fast the player will move (pixels/sec).
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
    public void attack(string attackName){
        var aniPlayer = GetNode<AnimationPlayer>("attackAnimation");
        
            aniPlayer.Play(attackName);
    }
    private void _on_hit_box_area_entered(Area2D col) {
        GD.Print("damage taken");
    }
    public void hideSprite(string spriteName) {
        GetNode<Sprite2D>(spriteName).Visible = false;
            }
    public void showSprite (string spriteName) {
        GetNode<Sprite2D>(spriteName).Visible = true;
    }
    // delta is time passed on screen
    public void dodgeAttack(string dodgeName) {
        var aniPlayer = GetNode<AnimationPlayer>("dodgeAnimation");

        aniPlayer.Play(dodgeName);
    }
    public void physicsPlayer(double delta) 
    {
        Vector2 velocity = Vector2.Zero; 
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        // Sprite2D walkUpSprite = GetNode<Sprite2D>(SprPlayerUpWalk);
        var aniPlayerMoving = GetNode<AnimationPlayer>("walkAnimation");
        if (Input.IsActionJustPressed("Attack"))
        {
            isAttacking = true;
            if (facingDirection == FacingDirection.up)
            {
                attack("attackUp");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                showSprite("SprPlayerUpAttack");
                isAttacking = false;
                
            }
            if (facingDirection == FacingDirection.down)
            {
                attack("attackDown");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                showSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");
                isAttacking = false;
                

            }
            if (facingDirection == FacingDirection.left)
            {
                attack("attackLeft");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                showSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");
                isAttacking = false;
            }
            if (facingDirection == FacingDirection.right)
            {
                attack("attackRight");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                showSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");
                isAttacking = false;
            }
        }
        else if (direction != Vector2.Zero)
        {
            if (direction.X > 0)
            {
                aniPlayerMoving.Play("walkRight");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                showSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");
            }
            else if (direction.X < 0)
            {
                aniPlayerMoving.Play("walkLeft");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                showSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");

            }
            else if (direction.Y > 0)
            {
                aniPlayerMoving.Play("down");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                showSprite("SprPlayerDownAttack");
                hideSprite("SprPlayerUpAttack");
                
            }
            else if (direction.Y < 0)
            {
                aniPlayerMoving.Play("up");
                hideSprite("SprPlayerDownWalk");
                hideSprite("SprPlayerUpWalk");
                hideSprite("SprPlayerLeftWalk");
                hideSprite("SprPlayerRightWalk");
                hideSprite("SprPlayerLeftAttack");
                hideSprite("SprPlayerRightAttack");
                hideSprite("SprPlayerDownAttack");
                showSprite("SprPlayerUpAttack");

            }
            velocity = direction.Normalized() * Speed;
        } else if (!isAttacking)
        {
            aniPlayerMoving.Pause();
        }
        Velocity = velocity;
        MoveAndSlide();
    }
    
    public override void _Process(double delta)
    {
        physicsPlayer(delta);
    
    }

    
}
