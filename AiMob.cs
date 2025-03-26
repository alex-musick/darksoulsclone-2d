using Godot;
using System;

public partial class AiMob : CharacterBody2D
{
	// State Machine
	private enum State
	{
		Idle,
		Chase,
		Attack,
		Hurt,
		Dead
	}
	private State _currentState = State.Idle;

	// Exported Properties
	[Export] public int MaxHealth = 100;
	[Export] public float MoveSpeed = 150f;
	[Export] public float AttackRange = 50f;
	[Export] public float VisionRange = 300f;

	// Nodes
	private NavigationAgent2D _navAgent;
	private AnimationPlayer _animPlayer;
	private Node2D _player;

	// Internal Variables
	private int _currentHealth;
	private bool _playerVisible;
	private bool _attackCooldown;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_player = GetNode<Node2D>("/root/Player");
		
		var hitbox = GetNode<Area2D>("Hitbox");
		hitbox.BodyEntered += OnHitboxBodyEntered;
		hitbox.BodyExited += OnHitboxBodyExited;
		
		var hurtbox = GetNode<Area2D>("Hurtbox");
		hurtbox.AreaEntered += OnHurtboxAreaEntered;

		if (_player == null){
			GD.Print("Player not found");
		}

		SetupNavigation();
		StartPathUpdateTimer();
	}

	private void SetupNavigation()
	{
		_navAgent.PathDesiredDistance = 4f;
		_navAgent.TargetDesiredDistance = 4f;
	}

	private void StartPathUpdateTimer()
	{
		Timer pathUpdateTimer = new Timer();
		AddChild(pathUpdateTimer);
		pathUpdateTimer.WaitTime = 0.5f;
		pathUpdateTimer.Timeout += UpdatePath;
		pathUpdateTimer.Start();
	}

	private void UpdatePath()
	{
		if (_player != null && !IsDead())
		{
			GD.Print("Updating Path to Player: " + _player.GlobalPosition);
			_navAgent.TargetPosition = _player.GlobalPosition;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead()) return;

		UpdateState();
		UpdateAnimation();

		switch (_currentState)
		{
			case State.Chase:
				HandleMovement();
				CheckAttackRange();
				break;

			case State.Attack:
				HandleAttack();
				break;
		}
	}

	private void UpdateState()
	{
		if (_currentState == State.Hurt || _currentState == State.Dead) return;

		if (_playerVisible)
		{
			if (Position.DistanceTo(_player.GlobalPosition) <= AttackRange)
				_currentState = State.Attack;
			else
				_currentState = State.Chase;
		}
		else
		{
			_currentState = State.Idle;
		}
	}

	private void HandleMovement()
	{
		if (_navAgent.IsNavigationFinished()) 
		{
			GD.Print("Navigation Finished - No movement");
			return;
		}

		Vector2 nextPosition = _navAgent.GetNextPathPosition();
		Vector2 direction = Position.DirectionTo(nextPosition);

		if (direction.Length() > 0)
		{
			GD.Print("Moving towards: " + nextPosition);
			Velocity = direction * MoveSpeed;
			MoveAndSlide();
		}
	}

	private void CheckAttackRange()
	{
		if (Position.DistanceTo(_player.GlobalPosition) <= AttackRange)
			_currentState = State.Attack;
	}

	private async void HandleAttack()
	{
		if (_attackCooldown) return;

		_attackCooldown = true;
		_animPlayer.Play("attack");
		await ToSignal(_animPlayer, "animation_finished");
		_attackCooldown = false;
	}

	private void  UpdateAnimation()
	{
		switch (_currentState)
		{
			case State.Idle:
				_animPlayer.Play("idle");
				break;

			case State.Chase:
				_animPlayer.Play("walk");
				break;

			case State.Attack:
				_animPlayer.Play("attack");
				break;
		}
	}

	private void TakeDamage(int damage)
	{
		if (IsDead()) return;

		_currentHealth -= damage;

		if (_currentHealth <= 0)
		{
			SetState(State.Dead);
			_animPlayer.Play("death");
			GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
			GetNode<Area2D>("Hitbox").Monitoring = false;
		}
		else
		{
			SetState(State.Hurt);
			_animPlayer.Play("hurt");
			GetTree().CreateTimer(0.5f).Timeout += () => SetState(State.Chase);
		}
	}

	private void SetState(State newState)
	{
		_currentState = newState;
	}

	private bool IsDead()
	{
		return _currentState == State.Dead;
	}

	private void OnHitboxBodyEntered(Node2D body)
	{
		GD.Print("Hitbox Body Entered: " + body.Name);
		if (body.Name == "Player")
		{
			_playerVisible = true;
			_currentState = State.Chase;
		}
	}
	private void OnHitboxBodyExited(Node2D body)
	{
		GD.Print("Hitbox Body Exited: " + body.Name);
		if (body.Name == "Player")
		{
			_playerVisible = false;
			_currentState = State.Idle;
		}
	}

	private void _on_DeathAnimation_finished()
	{
		QueueFree();
	}
	private void OnHurtboxAreaEntered(Area2D area)
	{
		if (area.IsInGroup("player_attack"))
		{
			int damage = (int)area.Get("damage"); 
			TakeDamage(damage); 
		}
	}
}
