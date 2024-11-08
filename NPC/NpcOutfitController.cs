using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.NPC
{
    [GlobalClass]
    public partial class NpcOutfitController : Node
    {
        [Export]
        public NpcOutfits outfitList;
        [Export]
        public MeshInstance3D feet;
        [Export]
        public MeshInstance3D hands;
        [Export]
        public MeshInstance3D torso;
        [Export]
        public MeshInstance3D head;

        public void RandomizeParts()
        {
            feet.Mesh = outfitList.feetMeshes.PickRandom();
            hands.Mesh = outfitList.handMeshes.PickRandom();
            torso.Mesh = outfitList.torsoMeshes.PickRandom();
            head.Mesh = outfitList.headMeshes.PickRandom();
        }
    }
}
