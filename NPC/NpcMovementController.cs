using Godot;
using ShroomJamGame.CharacterBody;
using ShroomJamGame.Interaction;
using ShroomJamGame.Tasks;
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
        private Vector3 targetPosition;
        [Export]
        private CharacterBody3D? characterBody3D;
        [Export]
        private NavigationAgent3D? _navigationAgent;
        [Export]
        NpcVisualController? _visualController;
        [Export]
        RayCast3D? interactionRay;
        [Export]
        Godot.Collections.Array<Node3D> ownedItems;
        Godot.Collections.Array<Vector3> ownedItemsStartingPositions = new Godot.Collections.Array<Vector3>();
        Godot.Collections.Array<Vector3> ownedItemsStartingRotations = new Godot.Collections.Array<Vector3>();
        public bool busy = false;
        private bool grabbingObject = false;
        private bool placingObject = false;
        private bool reachedTarget = false;
        private Node3D heldObject;
        double decideTimer = 0;
        [Signal]
        public delegate void ReachedDestinationEventHandler();
        [Export]
        Node3D speechBubbleNode;
        [Export]
        Label speechBubbleText;
        [Export]
        Sprite3D speechBubbleSprite;
        [Export]
        SubViewport speechBubbleViewPort;

        public void SetSpeechBubble(string content)
        {
            speechBubbleText.Text = content;
            speechBubbleNode.Visible = content != "";
        }
        private void _SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            reachedTarget = false;
            _navigationAgent.TargetPosition = position;
        }
        private Vector3 oldTargetPos = Vector3.Zero;
        public override void _Ready()
        {
            _visualController.RandomizeOutfit();
            foreach (var item in ownedItems)
            {
                ownedItemsStartingPositions.Add(item.GlobalPosition);
                ownedItemsStartingRotations.Add(item.Rotation);
            }
            this.ReachedDestination += NpcMovementController_ReachedDestination;
            speechBubbleSprite.Texture = speechBubbleViewPort.GetTexture();

            DecideTime();
            SetSpeechBubble("");
        }
        public override void _Process(double delta)
        {
            _visualController.SetAnimationTreeState(characterBody3D.Velocity.Normalized().Length(), false);
            decideTimer += delta;
            if (decideTimer > 2)
            {
                DecideTime();
                decideTimer = 0;
            }
        }
        private void DecideTime()
        {
            if (busy || grabbingObject || placingObject)
            {
                return;
            }
            for (int i = 0; i < ownedItems.Count; i++)
            {
                if (ownedItems[i].GlobalPosition.DistanceTo(ownedItemsStartingPositions[i]) > 1)
                {
                    targetNode = ownedItems[i];
                    SetSpeechBubble($"Why did you throw my {targetNode.Name}");
                    grabbingObject = true;
                    return;
                }
            }
            targetNode = ownedItems.PickRandom();

        }

        private void NpcMovementController_ReachedDestination()
        {
            if (placingObject)
            {
                placingObject = false;
                heldObject.GlobalPosition = ownedItemsStartingPositions[ownedItems.IndexOf(heldObject)];
                heldObject.Rotation = ownedItemsStartingRotations[ownedItems.IndexOf(heldObject)];
                (heldObject as PhysicsInteractable).Freeze = false;
                (heldObject as PhysicsInteractable).isHeld = true;
                heldObject = null;
                _visualController.Interact();
                SetSpeechBubble($"");
            }
            if (grabbingObject && IsInstanceValid(targetNode) && !(targetNode as PhysicsInteractable).isHeld)
            {
                grabbingObject = false;
                placingObject = true;
                _SetTargetPosition(ownedItemsStartingPositions[ownedItems.IndexOf(targetNode)]);
                heldObject = targetNode;
                heldObject.GlobalPosition = new Vector3(-9999, -9999, -9999);
                (heldObject as PhysicsInteractable).Freeze = true;
                (heldObject as PhysicsInteractable).isHeld = false;
                targetNode = null;
                _visualController.Interact();
                SetSpeechBubble($"Let me put it pack");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (IsInstanceValid(targetNode) && oldTargetPos != targetNode.GlobalPosition)
            {
                oldTargetPos = targetNode.GlobalPosition;
                _SetTargetPosition(oldTargetPos);
            }
            if (_navigationAgent.IsNavigationFinished())
            {
                _characterMovementController.InputMovement = new Vector2(0, 0);
                return;
            }
            if (_navigationAgent.DistanceToTarget() < 2)
            {
                if (!reachedTarget)
                {
                    reachedTarget = true;
                    EmitSignal(SignalName.ReachedDestination);
                }
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
                    _visualController.Interact();
                }
            }
        }
    }
}
