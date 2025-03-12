using Godot;
using System;

public partial class AiMob : CharacterBody2D
{
    [Export] public float Speed = 100f;
    [Export] public float DetectionRange = 200f; // How close the player must be to be detected
    [Export] public float PatrolTime = 2f; // Time before switching patrol direction

    private Node2D player;
    private NavigationAgent2D navAgent;
    private AnimationPlayer animationPlayer;
    private RayCast2D visionRay;
    private Vector2 patrolDirection;
    private float patrolTimer;
    private string currentAnimation = "";

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        visionRay = GetNode<RayCast2D>("VisionRay"); // This will check if AI can see the player

        player = GetNode<Node2D>("/root/MainScene/Player");

        patrolTimer = PatrolTime;
        ChooseNewPatrolDirection();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player != null && CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            Patrol((float)delta);
        }

        // Keep AI within screen bounds
        Vector2 screenSize = GetViewportRect().Size;
        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, 0, screenSize.X),
            Mathf.Clamp(GlobalPosition.Y, 0, screenSize.Y)
        );
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        if (GlobalPosition.DistanceTo(player.GlobalPosition) > DetectionRange) return false;

        visionRay.TargetPosition = ToLocal(player.GlobalPosition);
        visionRay.ForceRaycastUpdate(); // Make sure raycast updates every frame

        return !visionRay.IsColliding(); // If no obstacle, AI can see the player
    }

    private void ChasePlayer()
    {
        navAgent.TargetPosition = player.GlobalPosition;
        Vector2 direction = (navAgent.GetNextPathPosition() - GlobalPosition).Normalized();

        if (direction != Vector2.Zero)
        {
            Velocity = direction * Speed;
            MoveAndSlide();
            UpdateAnimation(direction);
        }
    }

    private void Patrol(float delta)
    {
        patrolTimer -= delta;

        if (patrolTimer <= 0)
        {
            ChooseNewPatrolDirection();
        }

        Velocity = patrolDirection * Speed;
        MoveAndSlide();
        UpdateAnimation(patrolDirection);
    }

    private void ChooseNewPatrolDirection()
    {
        Random random = new Random();
        int randomDirection = random.Next(4);

        switch (randomDirection)
        {
            case 0: patrolDirection = Vector2.Up; break;
            case 1: patrolDirection = Vector2.Down; break;
            case 2: patrolDirection = Vector2.Left; break;
            case 3: patrolDirection = Vector2.Right; break;
        }

        patrolTimer = PatrolTime;
    }

    private void UpdateAnimation(Vector2 direction)
    {
        string newAnimation = currentAnimation;

        if (direction.Y < 0)
            newAnimation = "ai_up";
        else if (direction.Y > 0)
            newAnimation = "ai_down";
        else if (direction.X > 0)
            newAnimation = "ai_right";
        else if (direction.X < 0)
            newAnimation = "ai_left";

        if (newAnimation != currentAnimation)
        {
            currentAnimation = newAnimation;
            animationPlayer.Play(currentAnimation);
        }
    }
}