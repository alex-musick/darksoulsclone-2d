using Godot;
using System;

public partial class AiMob : CharacterBody2D
{

	[Signal] public delegate void SpawnedEventHandler();
	[Signal] public delegate void DestroyedEventHandler();

    public override void _ExitTree()
    {
		EmitSignal(SignalName.Destroyed);
        base._ExitTree();
    }
    // State Machine
	private enum State
	{
		Idle,
		Patrol,
		Chase,
		Attack,
		Hurt,
		Dead
	}
	private State _currentState = State.Idle;

	// Exported Properties
	[Export] public double MaxHealth = 100;
	[Export] public float MoveSpeed = 150f;
	[Export] public float AttackRange = 50f;
	[Export] public float VisionRange = 300f;
	[Export] public float WaitTime = 2.0f;

	// Patrol variables
	private Vector2 _patrolStart;
	private Vector2 _patrolEnd;
	private Vector2 _patrolTarget;
	private bool _waiting = false;

	// Nodes
	private NavigationAgent2D _navAgent;
	private AnimationPlayer _animPlayer;
	private Node2D _player;
	
	//Sprites
	private Sprite2D _idleSprite;
	private Sprite2D _walkSprite;
	private Sprite2D _attackSprite;
	private Sprite2D _hurtSprite;
	private Sprite2D _deadSprite;
	public int damage { get; private set; } = 20;
	// Internal Variables
	private double _currentHealth = 5;
	private bool _playerVisible;
	private bool _attackCooldown;
	public static AiMob instance;
	private Area2D _attackBox;

	public override void _Ready()
	{
		EmitSignal(SignalName.Spawned);
		instance = this;
		_idleSprite = GetNode<Sprite2D>("IdleSprite");
		_walkSprite = GetNode<Sprite2D>("WalkSprite");
		_attackSprite = GetNode<Sprite2D>("AttackSprite");
		_hurtSprite = GetNode<Sprite2D>("HurtSprite");
		_deadSprite = GetNode<Sprite2D>("DeathSprite");
		
		_patrolStart = GlobalPosition;
		_patrolEnd = _patrolStart + new Vector2(200, 0);
		_patrolTarget = _patrolEnd;
	
		HideAllSprites();
		_idleSprite.Visible = true;
		
		_navAgent = GetNode<NavigationAgent2D>("NavAgent2D");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		_player = GetNode<Node2D>("/root/TestRoom/Player");
		_attackBox = GetNode<Area2D>("attackBox");
		
		var detectionArea = GetNode<Area2D>("DetectionArea");
		detectionArea.BodyEntered += OnDetectionAreaBodyEntered;
		detectionArea.BodyExited += OnDetectionAreaBodyExited;
		
		SetupNavigation();
		StartPathUpdateTimer();

		if (!_playerVisible)
		{
			_currentState = State.Patrol;
		}
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
		if (IsDead()) return;

		if (_playerVisible && _player != null)
		{
			_navAgent.TargetPosition = _player.GlobalPosition;
		}
		else if (_currentState == State.Patrol)
		{
			_navAgent.TargetPosition = _patrolTarget;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead()) return;

		var healthBar = GetNode<ProgressBar>("healthBar");
		if (_currentHealth < MaxHealth)
		{ 
			_currentHealth += 0.2; 
			healthBar.Value = _currentHealth;
		}

		UpdateState();
		UpdateAnimation();
		
		switch (_currentState)
		{
			case State.Patrol:
				HandlePatrol();
				HandleMovement();
				break;

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
		else if (_currentState != State.Patrol && !_waiting)
		{
			_currentState = State.Patrol;
		}
	}

	private async void HandlePatrol()
	{
		if (_waiting) return;

		// Check if we've reached the current patrol target
		if (GlobalPosition.DistanceTo(_patrolTarget) < 5f)
		{
			_currentState = State.Idle;
			_waiting = true;
			
			await ToSignal(GetTree().CreateTimer(WaitTime), "timeout");
			
			// Switch to the other patrol point
			_patrolTarget = (_patrolTarget == _patrolStart) ? _patrolEnd : _patrolStart;
			_navAgent.TargetPosition = _patrolTarget;
			_waiting = false;
			_currentState = State.Patrol;
		}
	}

	private void HandleMovement()
	{
		if (_navAgent.IsNavigationFinished()) 
		{
			return;
		}

		Vector2 nextPosition = _navAgent.GetNextPathPosition();
		Vector2 direction = (nextPosition - GlobalPosition).Normalized();

		Velocity = direction * MoveSpeed;
		MoveAndSlide();
	}

	private void CheckAttackRange()
	{
		if (_player != null && Position.DistanceTo(_player.GlobalPosition) <= AttackRange)
		{
			_currentState = State.Attack;
		}
	}

	private async void HandleAttack()
	{
		if (_attackCooldown) return;

		_attackCooldown = true;
		_animPlayer.Play("attack");
		_attackBox.Monitoring = true;
		
		await ToSignal(_animPlayer, "animation_finished");
		
		_attackBox.Monitoring = false;
		_attackCooldown = false;
		
		// Return to chase if player is still visible but out of attack range
		if (_playerVisible && Position.DistanceTo(_player.GlobalPosition) > AttackRange)
		{
			_currentState = State.Chase;
		}
	}
	private void  UpdateAnimation()
	{
		HideAllSprites();
		
		switch (_currentState)
		{
			case State.Idle:
				_idleSprite.Visible = true;
				_animPlayer.Play("idle");
				break;

			case State.Patrol:
			case State.Chase:
				_walkSprite.Visible = true;
				_animPlayer.Play("walk");
				break;

			case State.Attack:
				_attackSprite.Visible = true;
				_animPlayer.Play("attack");
				break;
			
			case State.Hurt:
				_hurtSprite.Visible = true;
				_animPlayer.Play("hurt");
				break;
			
			case State.Dead:
				_deadSprite.Visible = true;
				_animPlayer.Play("dead");
				break;
		}
	}

	private void TakeDamage(int damage)
	{
		if (IsDead()) return;

		_currentHealth -= Player.instance.damage;

		if (_currentHealth <= 0)
		{
			SetState(State.Dead);
			_animPlayer.Play("death");
			GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
			GetNode<Area2D>("attackBox").Monitoring = false;
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

	private void OnDetectionAreaBodyEntered(Node2D body)
	{
		GD.Print("DetectionArea Body Entered: " + body.Name);
		if (body.Name == "Player")
		{
			_playerVisible = true;
			_currentState = State.Chase;
		}
	}
	private void OnDetectionAreaBodyExited(Node2D body)
	{
		GD.Print("DetectionArea Body Exited: " + body.Name);
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
	
	private void OnHurtboxAreaEntered(Area2D col)
	{
		var healthBar = GetNode<ProgressBar>("healthBar");
		TakeDamage(damage); 
		healthBar.Value = _currentHealth;
		
	}
	
	private void HideAllSprites()
	{
		_idleSprite.Visible = false;
		_walkSprite.Visible = false;
		_attackSprite.Visible = false;
		_hurtSprite.Visible = false;
		_deadSprite.Visible = false;
	}
	
}
