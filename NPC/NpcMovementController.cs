using Godot;
using ShroomJamGame.Animalese;
using ShroomJamGame.CharacterBody;
using ShroomJamGame.Interaction;
using ShroomJamGame.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ShroomJamGame.NPC
{

    [GlobalClass]
    public partial class NpcMovementController : Node
    {
        public static Godot.Collections.Array<NpcMovementController> npcs = new Godot.Collections.Array<NpcMovementController>();
        [Export]
        private CharacterMovementController? _characterMovementController;
        [Export]
        public Node3D? targetNode
        {
            get => _targetNode;
            set
            {
                _targetNode = value;
                oldTargetPos = Vector3.Zero;
            }
        }
        private bool _spin = false;
        public bool spin
        {
            get => _spin;
            set
            {
                _spin = value;
                _visualController.skeleton.Rotation = new Vector3(0, 0, 0);
            }
        }
        private Node3D? _targetNode;
        [Export]
        private Vector3 targetPosition;
        [Export]
        public CharacterBody3D? characterBody3D;
        [Export]
        public NavigationAgent3D? _navigationAgent;
        [Export]
        public NpcVisualController? _visualController;
        [Export]
        RayCast3D? interactionRay;
        [Export]
        public Godot.Collections.Array<Node3D> ownedItems;
        public Godot.Collections.Array<Vector3> ownedItemsStartingPositions = new Godot.Collections.Array<Vector3>();
        public Godot.Collections.Array<Vector3> ownedItemsStartingRotations = new Godot.Collections.Array<Vector3>();
        public bool freezeUntilDoneSpeaking = false;
        public bool waiting = false;
        public bool permaWait = false;
        public Godot.Collections.Array<Vector3> needToGoTo = new Godot.Collections.Array<Vector3>();
        public Godot.Collections.Array<string> needToSay = new Godot.Collections.Array<string>();
        private bool grabbingObject = false;
        private bool placingObject = false;
        private bool reachedTarget = false;
        private Node3D heldObject;
        double decideTimer = 0;
        [Signal]
        public delegate void ReachedDestinationEventHandler(bool needToSaySomething, string thingToSay);
        [Export]
        Node3D speechBubbleNode;
        [Export]
        Label speechBubbleText;
        [Export]
        Sprite3D speechBubbleSprite;
        [Export]
        SubViewport speechBubbleViewPort;
        [Export]
        public AnimalesePlayer3D? AnimalesePlayer;
        public double stuckTimer = 0;
        public int stuckCount = 0;
        private Vector3 prevPathPosition = Vector3.Zero;
        [Export]
        public NpcInteractable interactionComponent;
        public bool onlyLookAtPlayer = false;
        public Vector3 originalPosition;
        
        [Export]
        private AudioStreamPlayer3D? _popPlayer;
        [Export]
        private AudioStream? _pickUpSound;
        [Export]
        private AudioStream? _putDownSound;
        
        public void SayWords(string whatToSay)
        {
            if (AnimalesePlayer is null || speechBubbleText is null)
                return;

            AnimalesePlayer.StopSpeaking();
            SetSpeechBubble("");
            if (string.IsNullOrEmpty(whatToSay))
                return;

            AnimalesePlayer.PlayText(whatToSay);
        }

        private void SetSpeechBubble(string content)
        {
            speechBubbleText.Text = content;
            speechBubbleNode.Visible = content != "";
        }
        public void _SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            reachedTarget = false;
            _navigationAgent.TargetPosition = position;
            waiting = false;
            _visualController.PlayRandomOneShot();
        }
        private Vector3 oldTargetPos = Vector3.Zero;
        public override void _Ready()
        {
            npcs.Add(this);
            _visualController.RandomizeOutfit();
            this.ReachedDestination += NpcMovementController_ReachedDestination;
            speechBubbleSprite.Texture = speechBubbleViewPort.GetTexture();
            foreach (var item in ownedItems)
            {
                (item as PhysicsInteractable).owner = this;
                ownedItemsStartingPositions.Add(item.GlobalPosition);
                ownedItemsStartingRotations.Add(item.Rotation);
            }
            AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
            DecideTime();
            AnimalesePlayer.CharactersPlayed += AnimalesePlayer_CharactersPlayed;
            SayWords("");
            if (ownedItems.Count == 0)
                SetupJanitor();
            originalPosition = characterBody3D.GlobalPosition;
        }

        private void AnimalesePlayer_FinishedPlaying()
        {
            DoneTalking();
        }
        private async void DoneTalking()
        {
            await ToSignal(this.GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
            if (!AnimalesePlayer.isSpeaking)
            {
                freezeUntilDoneSpeaking = false;
                SayWords("");
            }
        }

        public override void _ExitTree()
        {
            npcs.Remove(this);
            base._ExitTree();
        }
        private async void SetupJanitor()
        {

            await ToSignal(this.GetTree().CreateTimer(.5f), SceneTreeTimer.SignalName.Timeout);
            foreach (var item in PhysicsInteractable.physicsObjects)
            {
                if (!IsInstanceValid(item.owner))
                {
                    ownedItems.Add(item);
                    item.owner = this;
                    ownedItemsStartingPositions.Add(item.GlobalPosition);
                    ownedItemsStartingRotations.Add(item.Rotation);
                }
            }
        }

        private void AnimalesePlayer_CharactersPlayed(string chars)
        {
            SetSpeechBubble(speechBubbleText.Text + chars);
        }
        public override void _Process(double delta)
        {
            _visualController.SetAnimationTreeState(characterBody3D.Velocity.Length(), false);
            decideTimer += delta;
            if (decideTimer > 2)
            {
                DecideTime();
                decideTimer = 0;
            }
            if (characterBody3D.GlobalPosition.Y < 4)
            {
                characterBody3D.GlobalPosition = originalPosition;
            }
            if (spin)
            {
                _visualController.skeleton.RotateY((float)delta * 15);
            }
        }
        public bool IsDoingStuff()
        {
            return grabbingObject || placingObject || freezeUntilDoneSpeaking || waiting;
        }
        private void DecideTime()
        {
            if (!reachedTarget || IsDoingStuff())
            {
                return;
            }
            if (needToGoTo.Count() != 0)
            {
                _SetTargetPosition(needToGoTo.First());
                targetNode = null;
            }
            else
            {
                if (ownedItems.Count != 0)
                {
                    for (int i = 0; i < ownedItems.Count; i++)
                    {
                        if (ownedItems[i].GlobalPosition.DistanceTo(ownedItemsStartingPositions[i]) > 1 && !(ownedItems[i] as PhysicsInteractable).offLimits)
                        {
                            targetNode = ownedItems[i];
                            SayWords($"Why did you throw my {targetNode.Name}");
                            grabbingObject = true;
                            return;
                        }
                    }
                    targetNode = ownedItems.PickRandom();
                }
                else if (PhysicsInteractable.physicsObjects.Count != 0)
                {
                    targetNode = PhysicsInteractable.physicsObjects.PickRandom();
                }
            }
        }
        private bool CanBeGrabbed(PhysicsInteractable physicsInteractable)
        {
            return !physicsInteractable.isHeld && !physicsInteractable.Freeze;
        }
        private void NpcMovementController_ReachedDestination(bool needToSaySomething, string thing)
        {
            if (placingObject && IsInstanceValid(heldObject) && (heldObject is PhysicsInteractable physicsObject))
            {
                placingObject = false;
                heldObject.GlobalPosition = ownedItemsStartingPositions[ownedItems.IndexOf(heldObject)];
                heldObject.Rotation = ownedItemsStartingRotations[ownedItems.IndexOf(heldObject)];
                physicsObject.Freeze = false;
                physicsObject.isHeld = false;
                heldObject = null;
                _visualController.Interact();
                SayWords($"");
                
                if (_popPlayer is not null && _putDownSound is not null)
                {
                    _popPlayer.Stream = _putDownSound;
                    _popPlayer.Play();
                }
            }
            if (grabbingObject && IsInstanceValid(targetNode) && (targetNode is PhysicsInteractable physicsObject2) && CanBeGrabbed(physicsObject2))
            {
                if (_popPlayer is not null && _pickUpSound is not null)
                {
                    _popPlayer.Stream = _pickUpSound;
                    _popPlayer.Play();
                }
                
                grabbingObject = false;
                placingObject = true;
                _SetTargetPosition(ownedItemsStartingPositions[ownedItems.IndexOf(targetNode)]);
                heldObject = targetNode;
                heldObject.GlobalPosition = new Vector3(-9999, -9999, -9999);
                physicsObject2.Freeze = true;
                physicsObject2.isHeld = true;
                targetNode = null;
                _visualController.Interact();
                SayWords($"Let me put it back");
            }
            if (needToGoTo.Count() != 0 && !needToSaySomething)
            {
                _SetTargetPosition(needToGoTo.First());
                needToGoTo.RemoveAt(0);
                targetNode = null;
            }
            else if (needToSaySomething)
            {
                freezeUntilDoneSpeaking = true;
                SayWords(needToSay.First());
                needToSay.RemoveAt(0);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (IsInstanceValid(targetNode) && oldTargetPos != targetNode.GlobalPosition)
            {
                oldTargetPos = targetNode.GlobalPosition;
                _SetTargetPosition(oldTargetPos);
            }
            float distanceToTarget = _navigationAgent.DistanceToTarget();
            Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
            if (distanceToTarget < 2)
            {
                ReachTarget();
            }
            if (_navigationAgent.IsNavigationFinished())
            {
                ReachTarget();
                _characterMovementController.InputMovement = new Vector2(0, 0);
                if (onlyLookAtPlayer)
                {
                    Quaternion currentYRot2 = characterBody3D.Quaternion;
                    characterBody3D.LookAt(TaskTracker.instance.player.GlobalPosition);
                    characterBody3D.RotationDegrees = new Vector3(0, characterBody3D.RotationDegrees.Y + 180, 0);
                    Quaternion newYRot2 = characterBody3D.Quaternion;
                    characterBody3D.Quaternion = currentYRot2.Slerp(newYRot2, (float)delta * 8);
                }
                return;
            }
            else if (nextPathPosition == prevPathPosition)
            {
                stuckTimer += delta;
                if (stuckTimer > 1)
                {
                    stuckTimer = 0;
                    stuckCount++;
                    if (IsInstanceValid(targetNode))
                    {
                        _SetTargetPosition(targetNode.GlobalPosition);
                    }
                }
                if (stuckCount > 10)
                {
                    ReachTarget();
                }
            }
            prevPathPosition = nextPathPosition;

            Quaternion currentYRot = characterBody3D.Quaternion;
            characterBody3D.LookAt(onlyLookAtPlayer ? TaskTracker.instance.player.GlobalPosition : nextPathPosition);
            characterBody3D.RotationDegrees = new Vector3(0, characterBody3D.RotationDegrees.Y + 180, 0);
            Quaternion newYRot = characterBody3D.Quaternion;
            characterBody3D.Quaternion = currentYRot.Slerp(newYRot, (float)delta * 8);

            _characterMovementController.InputMovement = new Vector2(0, 1);
            if (freezeUntilDoneSpeaking || onlyLookAtPlayer)
            {
                _characterMovementController.InputMovement = new Vector2(0, 0);
            }
            if (interactionRay.IsColliding() && interactionRay.GetCollisionPoint().DistanceTo(characterBody3D.GlobalPosition) < 1.3)
            {
                var hit = interactionRay?.GetCollider();
                if (hit is not CollisionObject3D targetCollision)
                    return;
                DoorInteractable door = targetCollision.GetChildOrNull<DoorInteractable>(1);
                if (door is not null && !door.IsOpen)
                {
                    door.Interact();
                    _visualController.Interact();
                }
            }
        }
        public void ReachTarget()
        {
            if (!reachedTarget)
            {
                waiting = permaWait;
                stuckCount = 0;
                reachedTarget = true;
                EmitSignal(SignalName.ReachedDestination, needToGoTo.Count < needToSay.Count, needToSay.Count > 0 ? needToSay.First() : "");
            }
        }
        public void STOP()
        {
            waiting = false;
            ReachTarget();
            stuckCount = 10;
            stuckTimer = .75f;
        }
        public void GoToPositionAndSayWords(Vector3 position, string words)
        {
            waiting = false;
            needToGoTo.Add(position);
            needToSay.Add(words);
        }
        public void LookAtPosition(Vector3 position)
        {
            characterBody3D.LookAt(position);
            characterBody3D.RotationDegrees = new Vector3(0, characterBody3D.RotationDegrees.Y + 180, 0);
        }
        public static NpcMovementController GetRandomNPCWithItem(string itemName)//why didn't I make these sooner fml
        {
            Godot.Collections.Array<NpcMovementController> validNpcs = new Godot.Collections.Array<NpcMovementController>();
            foreach (var npc in npcs)
            {
                foreach (var item in npc.ownedItems)
                {
                    if (item.Name.ToString().StartsWith(itemName))
                    {
                        validNpcs.Add(npc);
                        break;
                    }
                }
            }
            return validNpcs.PickRandom();
        }
        public PhysicsInteractable GetPhysicsObjectOfName(string objectName)
        {
            Godot.Collections.Array<PhysicsInteractable> validItems = new Godot.Collections.Array<PhysicsInteractable>();
            foreach (var item in ownedItems)
            {
                if (item.Name.ToString().StartsWith(objectName))
                {
                    validItems.Add(item as PhysicsInteractable);
                }
            }
            return validItems.PickRandom();
        }

        public void SetTimeScale(float timeScale)
        {
            if (_characterMovementController is null)
                return;
            
            _characterMovementController.PersonalTimeScale = timeScale;
        }
    }
}
