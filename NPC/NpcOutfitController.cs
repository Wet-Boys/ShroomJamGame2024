using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShroomJamGame.NPC.NpcOutfitController;

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
        Material prevMaterial;
        public enum Eyes
        {
            normal,
            wink,
            crying,
            angry,
            moody,
            glee,
            pissing,
            hurt,
            dead,
            closed,
            wide,
            hearts
        }
        public enum Mouths
        {
            smile,
            frown,
            colonD,
            slanted,
            dColon,
            flat,
            swiggly,
            missing
        }
        public enum Feets
        {
            boots,
            crocs,
            heels,
            grippers
        }
        public enum Hands
        {
            balls
        }
        public enum Torsos
        {
            egg,
            jetsons,
            cone,
            roundCube
        }
        public enum Heads
        {
            balloon,
            blockHead,
            bow,
            coffee,
            cylinder,
            gear,
            gem,
            mushroom,
            sphere,
            star
        }
        public void RandomizeParts()
        {
            feet.Mesh = outfitList.feetMeshes.PickRandom();
            hands.Mesh = outfitList.handMeshes.PickRandom();
            torso.Mesh = outfitList.torsoMeshes.PickRandom();
            head.Mesh = outfitList.headMeshes.PickRandom();
            prevMaterial = (Material)head.Mesh.SurfaceGetMaterial(1).Duplicate();
            head.Mesh.SurfaceSetMaterial(1, prevMaterial);
            head.Mesh.SurfaceGetMaterial(1).Set("shader_parameter/Eyes", outfitList.eyeTextures.PickRandom());
            head.Mesh.SurfaceGetMaterial(1).Set("shader_parameter/Mouth", outfitList.mouthTextures.PickRandom());
        }
        public void SetEyes(Eyes eyeID)
        {
            head.Mesh.SurfaceGetMaterial(1).Set("shader_parameter/Eyes", outfitList.eyeTextures[(int)eyeID]);
        }
        public void SetMouth(Mouths mouthID)
        {
            head.Mesh.SurfaceGetMaterial(1).Set("shader_parameter/Mouth", outfitList.eyeTextures[(int)mouthID]);
        }
        public void SetFeet(Feets feetID)
        {
            feet.Mesh = outfitList.feetMeshes[(int)feetID];
        }
        public void SetHands(Hands handID)
        {
            hands.Mesh = outfitList.handMeshes[(int)handID];
        }
        public void SetTorsos(Torsos torsoID)
        {
            torso.Mesh = outfitList.torsoMeshes[(int)torsoID];
        }
        public void SetHead(Heads headID)
        {
            head.Mesh = outfitList.headMeshes[(int)headID];
        }
    }
}
