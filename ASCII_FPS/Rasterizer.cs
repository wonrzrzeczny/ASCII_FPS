﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ASCII_FPS
{
    public class Rasterizer
    {
        private Console console;
        private float[,] zBuffer;

        private const string fogString = "@&#8o*,:.";

        public Rasterizer(Console console)
        {
            this.console = console;
            zBuffer = new float[console.Width, console.Height];
        }

        public void Raster(Scene scene, Camera camera)
        {
            // Reset console
            for (int i = 0; i < console.Width; i++)
            {
                for (int j = 0; j < console.Height; j++)
                {
                    console.Data[i, j] = ' ';
                    console.Color[i, j] = 255;
                    zBuffer[i, j] = 1;
                }
            }

            // Clipping triangles with near plane - warning, very ugly code ahead
            List<Triangle> triangles = new List<Triangle>();

            foreach (Triangle triangle in scene.triangles)
            {
                bool s0 = triangle.V0.Z > camera.Near;
                bool s1 = triangle.V1.Z > camera.Near;
                bool s2 = triangle.V2.Z > camera.Near;

                float t0 = (camera.Near - triangle.V2.Z) / (triangle.V1.Z - triangle.V2.Z);
                float t1 = (camera.Near - triangle.V0.Z) / (triangle.V2.Z - triangle.V0.Z);
                float t2 = (camera.Near - triangle.V1.Z) / (triangle.V0.Z - triangle.V1.Z);

                Vector3 v0 = t0 * triangle.V1 + (1 - t0) * triangle.V2; // near plane intersection with v1--v2
                Vector3 v1 = t1 * triangle.V2 + (1 - t1) * triangle.V0; // near plane intersection with v2--v0
                Vector3 v2 = t2 * triangle.V0 + (1 - t2) * triangle.V1; // near plane intersection with v0--v1

                Vector2 uv0 = t0 * triangle.UV1 + (1 - t0) * triangle.UV2; // uv corresponding to v0
                Vector2 uv1 = t1 * triangle.UV2 + (1 - t1) * triangle.UV0; // uv corresponding to v1
                Vector2 uv2 = t2 * triangle.UV0 + (1 - t2) * triangle.UV1; // uv corresponding to v2

                if (s0 && s1 && s2) // fully beyond
                    triangles.Add(triangle);
                else if (s0 && !s1 && !s2) // one vertex beyond
                    triangles.Add(new Triangle(triangle.V0, v2, v1, triangle.Color, triangle.UV0, uv2, uv1));
                else if (s1 && !s2 && !s0)
                    triangles.Add(new Triangle(triangle.V1, v0, v2, triangle.Color, triangle.UV1, uv0, uv2));
                else if (s2 && !s0 && !s1)
                    triangles.Add(new Triangle(triangle.V2, v1, v0, triangle.Color, triangle.UV2, uv1, uv0));
                else if (!s0 && s1 && s2) // two vertices beyond
                {
                    triangles.Add(new Triangle(triangle.V2, v1, triangle.V1, triangle.Color, triangle.UV2, uv1, triangle.UV1));
                    triangles.Add(new Triangle(v1, v2, triangle.V1, triangle.Color, uv1, uv2, triangle.UV1));
                }
                else if (!s1 && s2 && s0)
                {
                    triangles.Add(new Triangle(triangle.V0, v2, triangle.V2, triangle.Color, triangle.UV0, uv2, triangle.UV2));
                    triangles.Add(new Triangle(v2, v0, triangle.V2, triangle.Color, uv2, uv0, triangle.UV2));
                }
                else if (!s2 && s0 && s1)
                {
                    triangles.Add(new Triangle(triangle.V1, v0, triangle.V0, triangle.Color, triangle.UV1, uv0, triangle.UV0));
                    triangles.Add(new Triangle(v0, v1, triangle.V0, triangle.Color, uv0, uv1, triangle.UV0));
                }
            }

            // Render
            Matrix m = camera.ProjectionMatrix;
            foreach (Triangle triangle in triangles)
            {
                Vector4 v0 = Vector4.Transform(new Vector4(triangle.V0, 1), m);
                Vector4 v1 = Vector4.Transform(new Vector4(triangle.V1, 1), m);
                Vector4 v2 = Vector4.Transform(new Vector4(triangle.V2, 1), m);

                Vector2 p0 = new Vector2(v0.X, -v0.Y) / v0.W;
                Vector2 p1 = new Vector2(v1.X, -v1.Y) / v1.W;
                Vector2 p2 = new Vector2(v2.X, -v2.Y) / v2.W;

                float z0 = v0.Z / v0.W;
                float z1 = v1.Z / v1.W;
                float z2 = v2.Z / v2.W;

                float minX = Math.Min(p0.X, Math.Min(p1.X, p2.X));
                float maxX = Math.Max(p0.X, Math.Max(p1.X, p2.X));
                float minY = Math.Min(p0.Y, Math.Min(p1.Y, p2.Y));
                float maxY = Math.Max(p0.Y, Math.Max(p1.Y, p2.Y));
                int minI = Math.Max(0, (int)((minX + 1f) * 0.5f * console.Width));
                int maxI = Math.Min(console.Width, (int)((maxX + 1f) * 0.5f * console.Width) + 1);
                int minJ = Math.Max(0, (int)((minY + 1f) * 0.5f * console.Height));
                int maxJ = Math.Min(console.Height, (int)((maxY + 1f) * 0.5f * console.Height) + 1);

                // Values for barycentric coordinates calculation
                float dot11 = Vector2.Dot(p1 - p0, p1 - p0);
                float dot22 = Vector2.Dot(p2 - p0, p2 - p0);
                float dot12 = Vector2.Dot(p1 - p0, p2 - p0);
                float det = dot11 * dot22 - dot12 * dot12;

                for (int i = minI; i < maxI; i++)
                {
                    for (int j = minJ; j < maxJ; j++)
                    {
                        float x = 2f * i / console.Width - 1;
                        float y = 2f * j / console.Height - 1;

                        Vector3 bar = Mathg.Barycentric(new Vector2(x, y), p0, p1, p2, dot11, dot22, dot12, det);

                        if (bar.X >= 0 && bar.Y >= 0 && bar.Z >= 0)
                        {
                            float z = z0 * bar.X + z1 * bar.Y + z2 * bar.Z;
                            
                            if (z > -1 && z < 1 && z < zBuffer[i, j])
                            {
                                zBuffer[i, j] = z;
                                console.Data[i, j] = fogString[(int)(Math.Max(0f, z * z * z) * fogString.Length)];

                                // Sample from texture
                                Vector2 uv = bar.X * triangle.UV0 / v0.W + bar.Y * triangle.UV1 / v1.W + bar.Z * triangle.UV2 / v2.W;
                                uv /= bar.X / v0.W + bar.Y / v1.W + bar.Z / v2.W;
                                console.Color[i, j] = Mathg.ColorTo8Bit(ASCII_FPS.texture[(int)(uv.X * 256) & 0xff, (int)(uv.Y * 256) & 0xff].ToVector3());
                            }
                        }
                    }
                }
            }
        }
    }
}