using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Player : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 75; // How fast the player will move (pixels/sec).
    public int dodgeSpeed { get; set; } = 125;
    public int damage { get; private set; } = 1;
    private int health = 5;
    private int stamina = 5;
    public enum FacingDirection
    {
        up, down, left, right
    }
    [Export] public FacingDirection facingDirection = new();
    [Export] public bool isAttacking = false;
    [Export] public bool isDodging = false;
    public static Player instance;
    Variant dodgingDirection;

    public Vector2 ScreenSize;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        instance = this;
    }
    public void attack(string attackName)
    {
        var aniPlayer = GetNode<AnimationPlayer>("attackAnimation");

        aniPlayer.Play(attackName);
    }
    public void hitTaken(string hitName)
    {
        var aniPlayer = GetNode<AnimationPlayer>("hitAnimation");
        aniPlayer.Play(hitName);
    }
    private void _on_hit_box_area_entered(Area2D col)
    {
        GD.Print("damage taken");
        health--;
        if (facingDirection == FacingDirection.up)
        {
            hitTaken("SprPlayerUpHit");
            hideAndShowAni("SprPlayerUpHit");
        }
        else if (facingDirection == FacingDirection.down)
        {
            hitTaken("SprPlayerDownHit");
            hideAndShowAni("SprPlayerDownHit");
        }
        else if (facingDirection == FacingDirection.left)
        {
            hitTaken("SprPlayerLeftHit");
            hideAndShowAni("SprPlayerLeftHit");
        }
        else if (facingDirection == FacingDirection.right)
        {
            hitTaken("SprPlayerRightHit");
            hideAndShowAni("SprPlayerRightHit");
        }

    }
    public void hideSprite(string spriteName)
    {
        GetNode<Sprite2D>(spriteName).Visible = false;
    }
    public void showSprite(string spriteName)
    {
        GetNode<Sprite2D>(spriteName).Visible = true;
    }
    public void hideAndShowAni(string spriteSelected)
    {
        List<string> animations = new List<string> {"SprPlayerUpWalk", "SprPlayerDownWalk", "SprPlayerLeftWalk", "SprPlayerRightWalk",
        "SprPlayerUpAttack", "SprPlayerDownAttack", "SprPlayerLeftAttack", "SprPlayerRightAttack",
        "SprPlayerUpIdle", "SprPlayerDownIdle", "SprPlayerLeftIdle", "SprPlayerRightIdle",
        "SprPlayerUpDodgeroll", "SprPlayerDownDodgeroll", "SprPlayerLeftDodgeroll", "SprPlayerRightDodgeroll",
        "SprPlayerRightHit", "SprPlayerLeftHit", "SprPlayerUpHit", "SprPlayerDownHit",
        "SprPlayerDeath"};
        foreach (string name in animations)
        {
            if (name == spriteSelected)
            {
                showSprite(name);
            }
            else
            {
                hideSprite(name);
            }
        }
    }
    // delta is time passed on screen
    public void dodgeAttack(string dodgeName)
    {
        var aniPlayer = GetNode<AnimationPlayer>("dodgeAnimation");

        aniPlayer.Play(dodgeName);
    }
    public void idlePlayer(string idleName)
    {
        var aniPlayer = GetNode<AnimationPlayer>("idleAnimation");

        aniPlayer.Play(idleName);
    }
    public void physicsPlayer(double delta)
    {
        if (health <= 0)
        {
            var deathAni = GetNode<AnimationPlayer>("deathAnimation");
            deathAni.Play("deathAnimation");
        }
        else
        {
            Vector2 velocity = Vector2.Zero;
            Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            // Sprite2D walkUpSprite = GetNode<Sprite2D>(SprPlayerUpWalk);
            var aniPlayerMoving = GetNode<AnimationPlayer>("walkAnimation");
            var hitBoxArea = GetNode<Area2D>("hitBox");
            _on_hit_box_area_entered(GetNode<Area2D>("hitBox"));
            if (Input.IsActionJustPressed("Attack"))
            {
                isAttacking = true;
                if (facingDirection == FacingDirection.up)
                {
                    attack("attackUp");
                    hideAndShowAni("SprPlayerUpAttack");
                }
                else if (facingDirection == FacingDirection.down)
                {
                    attack("attackDown");
                    hideAndShowAni("SprPlayerDownAttack");
                }
                else if (facingDirection == FacingDirection.left)
                {
                    attack("attackLeft");
                    hideAndShowAni("SprPlayerLeftAttack");
                }
                else if (facingDirection == FacingDirection.right)
                {
                    attack("attackRight");
                    hideAndShowAni("SprPlayerRightAttack");
                }
            }
            else if (direction != Vector2.Zero)
            {
                if (direction.X > 0)
                {
                    // facingDirection = FacingDirection.right;
                    if (Input.IsActionJustPressed("Dodge"))
                    {
                        // var dodgingDirection = facingDirection;
                        isDodging = true;
                        dodgeAttack("rightDodge");
                        hideAndShowAni("SprPlayerRightDodgeroll");
                    }
                    else if (isDodging == false)
                    {
                        aniPlayerMoving.Play("walkRight");
                        hideAndShowAni("SprPlayerRightWalk");
                    }
                }
                else if (direction.X < 0)
                {
                    if (Input.IsActionJustPressed("Dodge"))
                    {
                        isDodging = true;
                        dodgeAttack("leftDodge");
                        hideAndShowAni("SprPlayerLeftDodgeroll");
                    }
                    else if (isDodging == false)
                    {
                        aniPlayerMoving.Play("walkLeft");
                        hideAndShowAni("SprPlayerLeftWalk");
                    }

                }
                else if (direction.Y > 0)
                {
                    if (Input.IsActionJustPressed("Dodge"))
                    {
                        isDodging = true;
                        dodgeAttack("downDodge");
                        hideAndShowAni("SprPlayerDownDodgeroll");
                    }
                    else if (isDodging == false)
                    {
                        aniPlayerMoving.Play("down");
                        hideAndShowAni("SprPlayerDownWalk");
                    }
                }
                else if (direction.Y < 0)
                {
                    if (Input.IsActionJustPressed("Dodge"))
                    {
                        isDodging = true;
                        dodgeAttack("upDodge");
                        hideAndShowAni("SprPlayerUpDodgeroll");
                    }
                    else if (isDodging == false)
                    {
                        aniPlayerMoving.Play("up");
                        hideAndShowAni("SprPlayerUpWalk");
                    }
                }
                if (isDodging == true)
                {
                    velocity = direction.Normalized() * dodgeSpeed;
                }
                else velocity = direction.Normalized() * Speed;
            }
            else if (Input.IsActionJustPressed("Dodge"))
            {
                isDodging = true;

                if (facingDirection == FacingDirection.right)
                {
                    dodgeAttack("rightDodge");
                    hideAndShowAni("SprPlayerRightDodgeroll");
                }
                else if (facingDirection == FacingDirection.left)
                {
                    dodgeAttack("leftDodge");
                    hideAndShowAni("SprPlayerLeftDodgeroll");
                }
                else if (facingDirection == FacingDirection.down)
                {
                    dodgeAttack("downDodge");
                    hideAndShowAni("SprPlayerDownDodgeroll");
                }
                else if (facingDirection == FacingDirection.up)
                {
                    dodgeAttack("upDodge");
                    hideAndShowAni("SprPlayerUpDodgeroll");
                }
            }
            else if (velocity == Vector2.Zero && !isAttacking && !isDodging)
            {
                GD.Print(!isDodging);
                if (facingDirection == FacingDirection.up)
                {
                    idlePlayer("idleUp");
                    hideAndShowAni("SprPlayerUpIdle");
                }
                else if (facingDirection == FacingDirection.down)
                {
                    idlePlayer("idleDown");
                    hideAndShowAni("SprPlayerDownIdle");
                }
                else if (facingDirection == FacingDirection.left)
                {
                    idlePlayer("idleLeft");
                    hideAndShowAni("SprPlayerLeftIdle");
                }
                else if (facingDirection == FacingDirection.right)
                {
                    idlePlayer("idleRight");
                    hideAndShowAni("SprPlayerRightIdle");
                }
            }
            else if (!isAttacking && !isDodging)
            {
                aniPlayerMoving.Pause();
            }
            Velocity = velocity;
            MoveAndSlide();
        }
    }
    public override void _Process(double delta)
    {
        physicsPlayer(delta);
    }
}
