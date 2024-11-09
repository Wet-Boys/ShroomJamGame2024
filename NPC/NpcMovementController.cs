﻿using Godot;
using ShroomJamGame.CharacterBody;
using ShroomJamGame.Interaction;
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
        [Export]
        NpcVisualController? _visualController;
        [Export]
        RayCast3D? interactionRay;

        public void _SetTargetNode(Node3D node)
        {
            targetNode = node;
        }
        private void _SetTargetPosition(Vector3 position)
        {
            _navigationAgent.TargetPosition = position;
        }
        private Vector3 oldTargetPos = Vector3.Zero;
        public override void _Ready()
        {
            _visualController.RandomizeOutfit();
        }
        public override void _Process(double delta)
        {
            _visualController.SetAnimationTreeState(characterBody3D.Velocity.Normalized().Length(), false);
        }
        public override void _PhysicsProcess(double delta)
        {
            if (!IsInstanceValid(targetNode))
            {
                _characterMovementController.InputMovement = new Vector2(0, 0);
                return;
            }
            if (oldTargetPos != targetNode.GlobalPosition)
            {
                oldTargetPos = targetNode.GlobalPosition;
                _SetTargetPosition(oldTargetPos);
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

            if (interactionRay.IsColliding() && interactionRay.GetCollisionPoint().DistanceTo(characterBody3D.GlobalPosition) < 1.3)
            {
                var hit = interactionRay?.GetCollider();
                if (hit is not CollisionObject3D targetCollision)
                    return;
                DoorInteractable door = targetCollision.GetChildOrNull<DoorInteractable>(3);
                if (door is not null && !door.IsOpen)
                {
                    door.Interact();
                }
            }
        }
    }
}
