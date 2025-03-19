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
    private RayCast2D wallRay;
    private Area2D detectionArea;
    private Vector2 targetPatrolDirection;
    private Vector2 patrolDirection;
    private static readonly Random random = new Random();
    private enum AiAnimation { Idle, Up, Down, Left, Right }
    private AiAnimation currentAnimation;
    private Sprite2D walkSprite;
    private Sprite2D attackSprite;
    private AnimationPlayer walkAnimationPlayer;
    private AnimationPlayer attackAnimationPlayer;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
        visionRay = GetNode<RayCast2D>("VisionRay");
        player = GetNodeOrNull<Node2D>("/root/TestRoom/Player");
        wallRay = GetNode<RayCast2D>("WallRay");
        detectionArea = GetNode<Area2D>("DetectionArea");

        walkSprite = GetNodeOrNull<Sprite2D>("WalkSprite2D");
        attackSprite = GetNodeOrNull<Sprite2D>("AttackSprite2D");

        if (walkSprite != null)
            walkAnimationPlayer = walkSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

        if (attackSprite != null)
            attackAnimationPlayer = attackSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

        if (walkSprite == null || attackSprite == null || walkAnimationPlayer == null || attackAnimationPlayer == null)
        {
            GD.PrintErr("Error: Missing required nodes in AiMob.");
            return;
        }

        walkSprite.Visible = true;
        attackSprite.Visible = false;

        detectionArea.Connect("body_entered", Callable.From((Node body) => OnPlayerEntered(body)));
        detectionArea.Connect("body_exited", Callable.From((Node body) => OnPlayerExited(body)));
        patrolTimer = PatrolTime;
        ChooseNewPatrolDirection();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player != null && CanSeePlayer())
        {
            // Trigger attack if the player is within a certain range (e.g., 50 units)
            if (GlobalPosition.DistanceTo(player.GlobalPosition) < 50f)
            {
                // Trigger attack animation
                UpdateAnimation(Vector2.Zero, true); // You can customize direction if needed
            }
            else
            {
                // Chase the player if it's not within attack range
                ChasePlayer();
            }
        }
        else
        {
            Patrol((float)delta);
        }

        KeepWithinBounds();
    }

    private void OnPlayerEntered(Node body)
    {
        if (body is Node2D detectedPlayer)
        {
            player = detectedPlayer;
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

        visionRay.GlobalPosition = GlobalPosition;
        visionRay.TargetPosition = ToLocal(player.GlobalPosition).Normalized() * DetectionRange;
        visionRay.ForceRaycastUpdate();

        return !visionRay.IsColliding();
    }

    private void ChasePlayer()
    {
        navAgent.TargetPosition = player.GlobalPosition;
        Vector2 direction = (navAgent.GetNextPathPosition() - GlobalPosition).Normalized();
        MoveAI(direction);
    }

    private void Patrol(float delta)
    {
        patrolTimer -= delta;

        patrolDirection = patrolDirection.Lerp(targetPatrolDirection, 0.2f);

        wallRay.TargetPosition = patrolDirection * 50;
        wallRay.ForceRaycastUpdate();

        if (patrolTimer <= 0 || wallRay.IsColliding())
        {
            ChooseNewPatrolDirection();
        }

        MoveAI(patrolDirection);
    }

    private void ChooseNewPatrolDirection()
    {
        int randomDirection = random.Next(4); // 0 = left, 1 = right, 2 = up, 3 = down

        switch (randomDirection)
        {
            case 0: targetPatrolDirection = Vector2.Left; break;
            case 1: targetPatrolDirection = Vector2.Right; break;
            case 2: targetPatrolDirection = Vector2.Up; break;
            case 3: targetPatrolDirection = Vector2.Down; break;
        }

        patrolDirection = targetPatrolDirection;
        patrolTimer = PatrolTime;
    }

    private void UpdateAnimation(Vector2 direction, bool isAttacking = false)
    {
        if (walkSprite == null || attackSprite == null || walkAnimationPlayer == null || attackAnimationPlayer == null)
        {
            GD.PrintErr("UpdateAnimation: Sprite or AnimationPlayer is null.");
            return;
        }

        AiAnimation newAnimation = currentAnimation;

        if (isAttacking)
        {
            walkSprite.Visible = false;
            attackSprite.Visible = true;
            walkAnimationPlayer.Stop(); // Stop any ongoing walk animation

            if (direction.Y < 0) newAnimation = AiAnimation.Up;
            else if (direction.Y > 0) newAnimation = AiAnimation.Down;
            else if (direction.X > 0) newAnimation = AiAnimation.Right;
            else if (direction.X < 0) newAnimation = AiAnimation.Left;

            attackAnimationPlayer.Play("attack_" + newAnimation.ToString().ToLower());
        }
        else
        {
            walkSprite.Visible = true;
            attackSprite.Visible = false;
            attackAnimationPlayer.Stop(); // Stop any ongoing attack animation

            if (direction.Y < 0) newAnimation = AiAnimation.Up;
            else if (direction.Y > 0) newAnimation = AiAnimation.Down;
            else if (direction.X > 0) newAnimation = AiAnimation.Right;
            else if (direction.X < 0) newAnimation = AiAnimation.Left;

            walkAnimationPlayer.Play("walk_" + newAnimation.ToString().ToLower());
        }

        currentAnimation = newAnimation;
    }

    private void MoveAI(Vector2 direction)
    {
        if (direction != Vector2.Zero)
        {
            Velocity = direction * Speed;
            MoveAndSlide();
            UpdateAnimation(direction);
        }
    }

    private void KeepWithinBounds()
    {
        Vector2 screenSize = GetViewportRect().Size;

        if (GlobalPosition.X < 0 || GlobalPosition.X > screenSize.X)
            patrolDirection.X *= -1; // Reverse X direction

        if (GlobalPosition.Y < 0 || GlobalPosition.Y > screenSize.Y)
            patrolDirection.Y *= -1; // Reverse Y direction
    }

    private void AttackPlayer()
    {
        // The AI will just trigger the attack animation when close to the player
        UpdateAnimation(Vector2.Zero, true); // Trigger attack animation
    }
}
