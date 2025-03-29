using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Player : CharacterBody2D
{
    // make sure death stays dead
    [Export]
    public int Speed { get; set; } = 75; // How fast the player will move (pixels/sec).
    public int dodgeSpeed { get; set; } = 200;
    public int damage { get; private set; } = 50;
    private double maxHealth = 100;
    private double currentHealth = 100;
    private double maxStamina = 100;
    private double currentStamina = 5;
    [Export] private bool takingDamage = false;
    public enum FacingDirection
    {
        up, down, left, right
    }
    [Export] public FacingDirection facingDirection = new();
    [Export] public bool isAttacking = false;
    [Export] public bool isDodging = false;
    public static Player instance;
    // Variant dodgingDirection;


    public Vector2 ScreenSize;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        instance = this;
        idlePlayer("idleUp");
        hideAndShowAni("SprPlayerUpIdle");
        var healthBar = GetNode<ProgressBar>("healthBar");
        var staminaBar = GetNode<ProgressBar>("staminaBar");
        healthBar.MaxValue = maxHealth;
        healthBar.Value = maxHealth;
        staminaBar.MaxValue = maxStamina;
        isDodging = false;
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
    public void _on_death_animation_animation_finished(string aniName)
    {
        if (aniName == "deathAni")
        {
            QueueFree();
        }
    }
    private void _on_hit_box_area_entered(Area2D col)
    {
        // takingDamage = true;
        if (currentHealth <= 0)
        {
            var deathAni = GetNode<AnimationPlayer>("deathAnimation");
            deathAni.Play("deathAnimation");
            _on_death_animation_animation_finished("deathAni");
        }
        else if (col.Name == "attackBox")
        {
            takingDamage = true;
            currentHealth = currentHealth - AiMob.instance.damage;
            var healthBar = GetNode<ProgressBar>("healthBar");
            healthBar.Value = currentHealth;
            if (facingDirection == FacingDirection.up)
            {
                hitTaken("hitUp");
                hideAndShowAni("SprPlayerUpHit");
            }
            else if (facingDirection == FacingDirection.down)
            {
                hitTaken("hitDown");
                hideAndShowAni("SprPlayerDownHit");
            }
            else if (facingDirection == FacingDirection.left)
            {
                hitTaken("hitLeft");
                hideAndShowAni("SprPlayerLeftHit");
            }
            else if (facingDirection == FacingDirection.right)
            {
                hitTaken("hitRight");
                hideAndShowAni("SprPlayerRightHit");
            }
        }
    }
    public void hideSprite(string spriteName)
    {
        if (HasNode(spriteName))
        {
            GetNode<Sprite2D>(spriteName).Visible = false;
        }
        else
        {
            GD.PrintErr($"Node '{spriteName}' not found!");
        }
    }
    public void showSprite(string spriteName)
    {
        if (HasNode(spriteName))
        {
            GetNode<Sprite2D>(spriteName).Visible = true;
        }
        else
        {
            GD.PrintErr($"Node '{spriteName}' not found!");
        }
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
        // if (currentHealth <= 0)
        // {
        //     var deathAni = GetNode<AnimationPlayer>("deathAnimation");
        //     deathAni.Play("deathAnimation");
        // }
        if (takingDamage == false)
        {
            // GD.Print(isDodging);
            Vector2 velocity = Vector2.Zero;
            Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            var healthBar = GetNode<ProgressBar>("healthBar");
            var staminaBar = GetNode<ProgressBar>("staminaBar");
            var aniPlayerMoving = GetNode<AnimationPlayer>("walkAnimation");
            if (Input.IsActionJustPressed("Attack"))
            {
                if (currentStamina < 25)
                {
                    GD.Print("Cant Attack");
                }
                else
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
                    currentStamina -= 25;
                    staminaBar.Value = currentStamina;
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
                else if (isDodging == false)
                {
                    velocity = direction.Normalized() * Speed;
                }
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
            if (Input.IsActionJustPressed("Heal"))

                if (currentHealth < maxHealth)
                {
                    currentHealth += 25;
                    healthBar.Value = currentHealth;
                    if (currentHealth > 100)
                    {
                        currentHealth = 100;
                    }
                }
            if (currentStamina < maxStamina)
            {
                currentStamina += 0.2;
                staminaBar.Value = currentStamina;
            }
        }
    }
    public override void _Process(double delta)
    {
        physicsPlayer(delta);
    }
}
