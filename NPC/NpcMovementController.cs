using Godot;
using ShroomJamGame.CharacterBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.NPC
{

    [GlobalClass]
    public partial class NpcMovementController : Node
    {
        [Export]
        private CharacterMovementController? _characterMovementController;
        [Export]
        public Node3D? targetNode;
        [Export]
        private CharacterBody3D? characterBody3D;
        [Export]
        private NavigationAgent3D? _navigationAgent;

        public void _SetTargetPosition(Vector3 position)
        {
            _navigationAgent.TargetPosition = position;
        }
        private Vector3 oldPlayerPos = Vector3.Zero;
        public override void _PhysicsProcess(double delta)
        {
            if (oldPlayerPos != targetNode.GlobalPosition)
            {
                oldPlayerPos = targetNode.GlobalPosition;
                _SetTargetPosition(oldPlayerPos);
            }
            if (_navigationAgent.IsNavigationFinished())
            {
                _characterMovementController.InputMovement = new Vector2(0, 0);
                return;
            }
            Quaternion currentYRot = characterBody3D.Quaternion;
            Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
            characterBody3D.LookAt(nextPathPosition);
            characterBody3D.RotationDegrees = new Vector3(0, characterBody3D.RotationDegrees.Y + 180, 0);
            Quaternion newYRot = characterBody3D.Quaternion;
            characterBody3D.Quaternion = currentYRot.Slerp(newYRot, (float)delta * 8);
            _characterMovementController.InputMovement = new Vector2(0,1);
        }
    }
}
