using Godot;

namespace ShroomJamGame.Player;

[Tool]
[GlobalClass]
public partial class StepHelper : Node3D
{
    private float _maxStepHeight = 0.5f;

    [Export]
    public float MaxStepHeight
    {
        get => _maxStepHeight;
        set
        {
            _maxStepHeight = value;
            UpdateShapeCasts();
        }
    }
    
    private float _minStepDepth = 0.35f;

    [Export]
    public float MinStepDepth
    {
        get => _minStepDepth;
        set
        {
            _minStepDepth = value;
            UpdateShapeCasts();
        }
    }
    
    private float _stepTolerance = 0.05f;

    [Export(PropertyHint.Range, "0, 0.1")]
    public float StepTolerance
    {
        get => _stepTolerance;
        set
        {
            _stepTolerance = value;
            UpdateShapeCasts();
        }
    }
    
    private float _floorTolerance = 0.01f;

    [Export(PropertyHint.Range, "0, 0.1")]
    public float FloorTolerance
    {
        get => _floorTolerance;
        set
        {
            _floorTolerance = value;
            UpdateShapeCasts();
        }
    }

    private Vector3 _minClearance = new(1, 2, 0.5f);

    [Export]
    public Vector3 MinClearance
    {
        get => _minClearance;
        set
        {
            _minClearance = value;
            UpdateShapeCasts();
        }
    }
    
    private CollisionObject3D? _playerBody;
    
    [Export]
    public CollisionObject3D? PlayerBody
    {
        get => _playerBody;
        set
        {
            _playerBody = value;
            EnsureExceptions();
        }
    }

    [Export]
    private ShapeCast3D? _stepShapeCast;
    [Export]
    private ShapeCast3D? _clearanceShapeCast;

    public override void _Ready()
    {
        if (_stepShapeCast is null)
        {
            _stepShapeCast = new ShapeCast3D();
            _stepShapeCast.Shape = new BoxShape3D();

            AddChild(_stepShapeCast);
            _stepShapeCast.SetName("StepShapeCast");
            _stepShapeCast.SetOwner(GetTree().GetEditedSceneRoot());
        }

        if (_clearanceShapeCast is null)
        {
            _clearanceShapeCast = new ShapeCast3D();
            _clearanceShapeCast.Shape = new BoxShape3D();

            AddChild(_clearanceShapeCast);
            _clearanceShapeCast.SetName("ClearanceShapeCast");
            _clearanceShapeCast.SetOwner(GetTree().GetEditedSceneRoot());
        }
        
        UpdateShapeCasts();
        EnsureExceptions();
    }

    private void UpdateShapeCasts()
    {
        if (_stepShapeCast is null || _clearanceShapeCast is null)
            return;

        var stepShape = (BoxShape3D)_stepShapeCast.Shape;
        stepShape.Size = new Vector3(1, _maxStepHeight - (_floorTolerance / 2f), _minStepDepth);
        
        _stepShapeCast.Shape = stepShape;
        _stepShapeCast.TargetPosition = new Vector3(0, _maxStepHeight / 2f, _minStepDepth / 2f);
        _stepShapeCast.Position = new Vector3(0, -_minClearance.Y / 2f + _floorTolerance / 4f, _minClearance.Z);
        
        var clearanceShape = (BoxShape3D)_clearanceShapeCast.Shape;
        clearanceShape.Size = new Vector3(_minClearance.X, _minClearance.Y, _minStepDepth);
        
        _clearanceShapeCast.Shape = clearanceShape;
        _clearanceShapeCast.TargetPosition = new Vector3(0, 0, _minStepDepth / 2f);
        _clearanceShapeCast.Position = new Vector3(0, _maxStepHeight + _stepTolerance, _minClearance.Z);
    }

    public bool IsTouchingValidStep()
    {
        if (_stepShapeCast is null || _clearanceShapeCast is null)
            return false;
        
        return _stepShapeCast.IsColliding() && !_clearanceShapeCast.IsColliding();
    }

    private void EnsureExceptions()
    {
        _stepShapeCast?.ClearExceptions();
        _clearanceShapeCast?.ClearExceptions();

        _stepShapeCast?.AddException(_playerBody);
        _clearanceShapeCast?.AddException(_playerBody);
    }

    public void AddException(CollisionObject3D? collisionObject)
    {
        _stepShapeCast?.AddException(collisionObject);
        _clearanceShapeCast?.AddException(collisionObject);
    }
    
    public void RemoveException(CollisionObject3D? collisionObject)
    {
        _stepShapeCast?.RemoveException(collisionObject);
        _clearanceShapeCast?.RemoveException(collisionObject);
    }
}