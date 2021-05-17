﻿using Microsoft.Xna.Framework;

namespace ASCII_FPS.GameComponents.Enemies
{
    public class PoisonMonster : Monster
    {
        public PoisonMonster(Vector3 position, float health, float damage)
            : base(PrimitiveMeshes.Tetrahedron(position, 3f, Assets.poisonMonsterTexture), health, damage) { }


        public override float HitRadius => 3f;
        protected override float AlertDistance => 70f;
        protected override float AttackDistance => 30f;
        protected override float Speed => 12f;
        protected override float ShootSpeed => 0.3f;


        protected override void Attack(Vector3 towardsTarget)
        {
            Assets.tsch.Play();
            Fire(towardsTarget);
        }

        protected override void Fire(Vector3 direction, float projectileSpeed = 40f)
        {
            MeshObject projectileMesh = PrimitiveMeshes.Octahedron(Position, 0.5f, Assets.projectile3Texture);
            Scene.AddGameObject(new EnemyProjectile(projectileMesh, direction, projectileSpeed, damage, true));
        }
    }
}
