using Godot;
using System;

public partial class AiMob : CharacterBody2D
{
    [Export] public float Speed = 75f;
    [Export] public float DetectionRange = 200f;
    [Export] public float PatrolTime = 2f;

    private Node2D player;
    private NavigationAgent2D navAgent;
    private AnimationPlayer animationPlayer;
    private RayCast2D visionRay;
    private float patrolTimer;
    private string currentAnimation = "";
    private RayCast2D wallRay;
    private Area2D detectionArea;
    private Vector2 targetPatrolDirection;
    private Vector2 patrolDirection;
    private Random random;

    public override void _Ready()
    {
        random = new Random();
        navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        visionRay = GetNode<RayCast2D>("VisionRay");
        player = GetNode<Node2D>("/root/TestRoom/Player");
        wallRay = GetNode<RayCast2D>("WallRay");
        detectionArea = GetNode<Area2D>("DetectionArea");

        detectionArea.Connect("body_entered", Callable.From((Node body) => OnPlayerEntered(body)));
        detectionArea.Connect("body_exited", Callable.From((Node body) => OnPlayerExited(body)));
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

    private void OnPlayerEntered(Node body)
    {
        if (body == player)
        {
            player = body as Node2D;
        }
    }

    private void OnPlayerExited(Node body)
    {
        if (body == player)
        {
            player = null;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        if (GlobalPosition.DistanceTo(player.GlobalPosition) > DetectionRange) return false;

        visionRay.TargetPosition = ToLocal(player.GlobalPosition);
        visionRay.ForceRaycastUpdate();

        return !visionRay.IsColliding();
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
        GD.Print("Patrol method running"); // Debug statement

        if (wallRay == null)
        {
            GD.Print("wallRay is null!");
            return;
        }

        if (animationPlayer == null)
        {
            GD.Print("animationPlayer is null!");
            return;
        }

        patrolTimer -= delta;

        // Smoothly transition to the new patrol direction
        patrolDirection = patrolDirection.Lerp(targetPatrolDirection, 0.1f);

        // Check for wall collision
        wallRay.TargetPosition = patrolDirection * 50;
        wallRay.ForceRaycastUpdate();

        if (patrolTimer <= 0 || wallRay.IsColliding())
        {
            ChooseNewPatrolDirection();
        }

        Velocity = patrolDirection * Speed;
        MoveAndSlide();
        UpdateAnimation(patrolDirection);
    }

    private void ChooseNewPatrolDirection()
    {
        int randomDirection = random.Next(4);

        switch (randomDirection)
        {
            case 0: targetPatrolDirection = Vector2.Up; break;
            case 1: targetPatrolDirection = Vector2.Down; break;
            case 2: targetPatrolDirection = Vector2.Left; break;
            case 3: targetPatrolDirection = Vector2.Right; break;
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