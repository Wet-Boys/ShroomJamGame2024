﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.NPC
{
    [GlobalClass]
    public partial class NpcOutfits : Resource
    {
        [Export]
        public Godot.Collections.Array<Mesh> feetMeshes;
        [Export]
        public Godot.Collections.Array<Mesh> handMeshes;
        [Export]
        public Godot.Collections.Array<Mesh> headMeshes;
        [Export]
        public Godot.Collections.Array<Mesh> torsoMeshes;
        [Export]
        public Godot.Collections.Array<Mesh> allMeshes;
        [Export]
        public Godot.Collections.Array<CompressedTexture2D> eyeTextures;
        [Export]
        public Godot.Collections.Array<CompressedTexture2D> mouthTextures;
        [Export]
        public Godot.Collections.Array<CompressedTexture2D> torsoTextures;
        //[Export]
        //public Godot.Collections.Array<Material> HeadMaterials;
        //[Export]
        //public Godot.Collections.Array<Material> TorsoMaterials;
    }
}
