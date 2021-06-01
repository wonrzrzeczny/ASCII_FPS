﻿using ASCII_FPS.GameComponents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ASCII_FPS.Scenes
{
    public static class SceneGenUtils
    {
        public static List<Triangle> MakeWall(Vector2[] points, float low, float high, AsciiTexture texture)
        {
            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < points.Length - 1; i++)
            {
                float x0 = points[i].X;
                float z0 = points[i].Y;
                float x1 = points[i + 1].X;
                float z1 = points[i + 1].Y;
                float ratio = (new Vector2(x0, z0) - new Vector2(x1, z1)).Length() / (high - low);

                Triangle triangle1 = new Triangle(new Vector3(x0, high, z0), new Vector3(x1, high, z1), new Vector3(x0, low, z0), texture,
                    new Vector2(0f, 0f), new Vector2(ratio, 0f), new Vector2(0f, 1f));
                Triangle triangle2 = new Triangle(new Vector3(x0, low, z0), new Vector3(x1, high, z1), new Vector3(x1, low, z1), texture,
                    new Vector2(0f, 1f), new Vector2(ratio, 0f), new Vector2(ratio, 1f));
                triangles.Add(triangle1);
                triangles.Add(triangle2);
            }

            return triangles;
        }

        public static void AddWalls(Scene scene, Zone zone, List<Vector2[]> walls, float h, ObstacleLayer layer, AsciiTexture texture, Vector3 offset)
        {
            AddWalls(scene, zone, walls, -h, h, layer, texture, offset);
        }

        public static void AddWalls(Scene scene, Zone zone, List<Vector2[]> walls, float low, float high, ObstacleLayer layer, AsciiTexture texture, Vector3 offset)
        {
            Vector2 offset2D = new Vector2(offset.X, offset.Z);
            foreach (Vector2[] wall in walls)
            {
                List<Triangle> wallTriangles = MakeWall(wall, low, high, texture);
                MeshObject meshObject = new MeshObject(wallTriangles, offset, 0f);
                zone.AddMesh(meshObject);
                for (int i = 0; i < wall.Length - 1; i++)
                {
                    scene.AddObstacle(wall[i] + offset2D, wall[i + 1] + offset2D, layer);
                }
            }
        }

        public static MeshObject MakeFloor(float left, float right, float bottom, float top, float height, AsciiTexture texture, bool floor, float uvDensity = 100f)
        {
            Vector3 tr = new Vector3(right, height, top);
            Vector3 tl = new Vector3(left, height, top);
            Vector3 br = new Vector3(right, height, bottom);
            Vector3 bl = new Vector3(left, height, bottom);
            
            Vector2 uvtl = new Vector2(left, top) / uvDensity;
            Vector2 uvtr = new Vector2(right, top) / uvDensity;
            Vector2 uvbl = new Vector2(left, bottom) / uvDensity;
            Vector2 uvbr = new Vector2(right, bottom) / uvDensity;

            List<Triangle> triangles = floor
                ? new List<Triangle>
                {
                    new Triangle(tl, tr, br, texture, uvtl, uvtr, uvbr),
                    new Triangle(tl, br, bl, texture, uvtl, uvbr, uvbl)
                }
                : new List<Triangle>
                {
                    new Triangle(tl, br, tr, texture, uvtl, uvbr, uvtr),
                    new Triangle(tl, bl, br, texture, uvtl, uvbl, uvbr)
                };
            return new MeshObject(triangles, Vector3.Zero, 0f);
        }

        public static void AddFloor(Zone zone, Vector2 v0, Vector2 v1, float height, AsciiTexture texture, bool floor, Vector3 offset, float uvDensity = 100f)
        {
            float left = offset.X + Math.Min(v0.X, v1.X);
            float right = offset.X + Math.Max(v0.X, v1.X);
            float bottom = offset.Z + Math.Min(v0.Y, v1.Y);
            float top = offset.Z + Math.Max(v0.Y, v1.Y);
            MeshObject mesh = MakeFloor(left, right, bottom, top, height + offset.Y, texture, floor, uvDensity);
            zone.AddMesh(mesh);
        }

        public static List<Vector2[]> MakeRoomWalls(float width, float height, float[] corridors, float wallThickness)
        {
            List<Vector2[]> result = new List<Vector2[]>();

            float x = (width - wallThickness) / 2;
            float x1 = width / 2;
            float y0 = (height - wallThickness) / 2;
            float y1 = -y0;

            Vector2 vecTopFront = new Vector2(x, y0);
            Vector2 vecTopBack = new Vector2(x1, y0);
            Vector2 vecBottomFront = new Vector2(x, y1);
            Vector2 vecBottomBack = new Vector2(x1, y1);
            Vector2 unit = -Vector2.UnitY;

            for (int t = 0; t < 4; t++)
            {
                if (corridors[t] > 0f)
                {
                    float wallLength = x - corridors[t] / 2;

                    result.Add(new Vector2[2] { vecTopFront, vecTopFront + unit * wallLength });
                    result.Add(new Vector2[2] { vecTopFront + unit * wallLength, vecTopBack + unit * wallLength });

                    result.Add(new Vector2[2] { vecBottomBack - unit * wallLength, vecBottomFront - unit * wallLength });
                    result.Add(new Vector2[2] { vecBottomFront - unit * wallLength, vecBottomFront });
                }
                else
                {
                    result.Add(new Vector2[2] { vecTopFront, vecBottomFront });
                }

                vecTopFront = new Vector2(-vecTopFront.Y, vecTopFront.X);
                vecTopBack = new Vector2(-vecTopBack.Y, vecTopBack.X);
                vecBottomFront = new Vector2(-vecBottomFront.Y, vecBottomFront.X);
                vecBottomBack = new Vector2(-vecBottomBack.Y, vecBottomBack.X);
                unit = new Vector2(-unit.Y, unit.X);
            }

            return result;
        }

        public static List<Vector2[]> MakeRect(float left, float right, float bottom, float top)
        {
            return new List<Vector2[]>
            {
                new Vector2[]
                {
                    new Vector2(left, top),
                    new Vector2(left, bottom),
                    new Vector2(right, bottom),
                    new Vector2(right, top),
                    new Vector2(left, top)
                }
            };
        }

        public static bool[,,] GenerateCorridorLayout(int sizeX, int sizeY, out bool[,] visited)
        {
            float loopChance = 2f / (sizeX * sizeY);

            int roomsLeft = sizeX * sizeY;

            Random rand = new Random();
            bool[,,] corridorLayout = new bool[sizeX, sizeY, 4];
            visited = new bool[sizeX, sizeY];
            List<Point> BFSnext = new List<Point>();

            int startX = sizeX / 2;
            int startY = sizeY / 2;
            BFSnext.Add(new Point(startX, startY));
            visited[startX, startY] = true;

            while (BFSnext.Count > 0)
            {
                int idx = rand.Next(BFSnext.Count);
                Point p = BFSnext[idx];
                BFSnext.RemoveAt(idx);
                roomsLeft--;

                Point[] shift = new Point[4] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

                for (int i = 0; i < 4; i++)
                {
                    Point q = p + shift[i];
                    if (q.X >= 0 && q.X < sizeX && q.Y >= 0 && q.Y < sizeY)
                    {
                        if (!visited[q.X, q.Y])
                        {
                            float pruneChance = (float)Math.Max(0f, 1 - 0.3f * roomsLeft / (sizeX + sizeY));
                            if (rand.NextDouble() > pruneChance)
                            {
                                visited[q.X, q.Y] = true;
                                corridorLayout[p.X, p.Y, i] = true;
                                corridorLayout[q.X, q.Y, (i + 2) % 4] = true;
                                BFSnext.Add(q);
                            }
                        }
                        else if (rand.NextDouble() < loopChance)
                        {
                            corridorLayout[p.X, p.Y, i] = true;
                            corridorLayout[q.X, q.Y, (i + 2) % 4] = true;
                        }
                    }
                }
            }

            return corridorLayout;
        }

        public static Collectible.Type?[,] DistributeCollectibles(Random rand, int size, Point exitRoom, bool[,] accessible)
        {
            // Distribute collectibles (3 x skill point, 2 x armor refill, 1 x hp refill)
            // 10 attempts per each collectible
            Collectible.Type?[,] collectibles = new Collectible.Type?[size, size];
            for (int b = 0; b < 6; b++)
            {
                for (int t = 0; t < 10; t++)
                {
                    int x = rand.Next(size);
                    int y = rand.Next(size);
                    if ((x != size / 2 || y != size / 2) && (x != exitRoom.X || y != exitRoom.Y) && accessible[x, y] && collectibles[x, y] == null)
                    {
                        collectibles[x, y] = b < 3 ? Collectible.Type.Skill
                            : b < 5 ? Collectible.Type.Armor : Collectible.Type.Health;
                        break;
                    }
                }
            }
            return collectibles;
        }
    }
}
