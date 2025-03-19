using Godot;
using System;

public partial class AiMob : CharacterBody2D
{
	[Export] public int MaxHealth = 100;
	[Export] public int AttackDamage = 10;
	private int currentHealth;

	[Export] public float Speed = 75f;
	[Export] public float DetectionRange = 200f;
	[Export] public float PatrolTime = 2f;

	private Node2D player;
	private NavigationAgent2D navAgent;
	private AnimationPlayer animationPlayer;
	private RayCast2D visionRay;
	private float patrolTimer;
	private RayCast2D wallRay;
	private Area2D hitbox;
	private Vector2 targetPatrolDirection;
	private Vector2 patrolDirection;
	private static readonly Random random = new Random();
	private enum AiAnimation { Idle, Up, Down, Left, Right }
	private AiAnimation currentAnimation;
	private Sprite2D idleSprite;
	private Sprite2D hurtSprite;
	private AnimationPlayer idleAnimationPlayer;
	private AnimationPlayer hurtAnimationPlayer;

	public override void _Ready()
	{
		navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
		visionRay = GetNode<RayCast2D>("VisionRay");
		player = GetNodeOrNull<Node2D>("/root/TestRoom/Player");
		wallRay = GetNode<RayCast2D>("WallRay");
		hitbox = GetNode<Area2D>("Hitbox");

		idleSprite = GetNodeOrNull<Sprite2D>("IdleSprite");
		hurtSprite = GetNodeOrNull<Sprite2D>("HurtSprite");

		if (idleSprite != null)
			idleAnimationPlayer = idleSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (hurtSprite != null)
			hurtAnimationPlayer = hurtSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (idleSprite == null || hurtSprite == null || idleAnimationPlayer == null || hurtAnimationPlayer == null)
		{
			GD.PrintErr("Error: Missing required nodes in AiMob.");
			return;
		}

		currentHealth = MaxHealth;

		idleSprite.Visible = true;
		hurtSprite.Visible = false;

		hitbox.Connect("body_entered", Callable.From((Node body) => OnPlayerEntered(body)));
		hitbox.Connect("body_exited", Callable.From((Node body) => OnPlayerExited(body)));
		patrolTimer = PatrolTime;
		ChooseNewPatrolDirection();

		Area2D hurtbox = GetNodeOrNull<Area2D>("Hurtbox");
		if (hurtbox != null)
		{
			hurtbox.Connect("area_entered", Callable.From((Area2D area) => OnHit(area)));
		}

		UpdateAnimation(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player != null && CanSeePlayer())
		{
			// Chase the player if it's not within attack range
			ChasePlayer();
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

	private void UpdateAnimation(bool isHurt)
	{
		if (idleSprite == null || hurtSprite == null || idleAnimationPlayer == null || hurtAnimationPlayer == null)
		{
			GD.PrintErr("UpdateAnimation: Sprite or AnimationPlayer is null.");
			return;
		}

		if (isHurt)
		{
			hurtSprite.Visible = true;
			idleSprite.Visible = false;
			hurtAnimationPlayer.Play("hurt");
		}
		else
		{
			hurtSprite.Visible = false;
			idleSprite.Visible = true;
			idleAnimationPlayer.Play("idle");
		}
	}

	private void MoveAI(Vector2 direction)
	{
		if (direction != Vector2.Zero)
		{
			Velocity = direction * Speed;
			MoveAndSlide();
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

	public void TakeDamage(int damage)
	{
		currentHealth -= damage;
		GD.Print($"AiMob took {damage} damage! Current health: {currentHealth}");

		if (currentHealth <= 0)
		{
			Die();
		}
		else
		{
			UpdateAnimation(true);
			GetTree().CreateTimer(0.5f).Timeout += () => UpdateAnimation(false);
		}
	}

	private void Die()
	{
		GD.Print("AiMob has died.");
		QueueFree(); // Remove the AI from the scene
	}

	private void OnHit(Area2D area)
	{
		TakeDamage(10); // AI takes a fixed amount of damage when hit
	}
}
