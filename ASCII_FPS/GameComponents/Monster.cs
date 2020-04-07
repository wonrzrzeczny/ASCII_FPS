﻿using Microsoft.Xna.Framework;
using System;

namespace ASCII_FPS.GameComponents
{
    public class Monster : GameObject
    {
        private readonly Scene scene;
        private readonly MeshObject meshObject;
        private readonly Random random;

        public Vector3 Position
        {
            get { return meshObject.Position; }
            set { meshObject.Position = value; }
        }

        public float HitRadius { get; }



        public Monster(Scene scene, MeshObject meshObject, float hitRadius)
        {
            random = new Random();
            this.scene = scene;
            this.meshObject = meshObject;
            HitRadius = hitRadius;
        }
        
        public override void Update(float deltaTime)
        {
            meshObject.Rotation += deltaTime * (float)Math.PI * 0.3f;
        }

        public void DealDamage()
        {
            Destroy = true;
            scene.RemoveDynamicMesh(meshObject);
        }
    }
}
