﻿using Microsoft.Xna.Framework;
using System;

namespace ASCII_FPS.GameComponents.Enemies
{
    public class Spooper : Monster
    {
        public Spooper(Vector3 position, float health, float damage)
            : base(new MeshObject(ASCII_FPS.spooperModel, ASCII_FPS.spooperTexture, position), health, damage)
        {
            random = new Random();
        }


        private readonly Random random;


        public override float HitRadius => 2f;
        protected override float AlertDistance => 90f;
        protected override float AttackDistance => 5f;
        protected override float Speed => 30f;
        protected override float ShootSpeed => 0.5f;


        protected override void Attack(Vector3 towardsTarget)
        {
            float volume = 1f - Vector3.Distance(Position, Camera.CameraPos) / 100f;
            ASCII_FPS.beep.Play(volume, (float)random.NextDouble(), 0f);
            if (Vector3.Distance(Position, Camera.CameraPos) < AttackDistance)
            {
                Game.PlayerStats.DealDamage(damage);
            }
        }
    }
}
