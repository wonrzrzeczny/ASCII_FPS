﻿using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace ASCII_FPS.UI
{
    public class Menu
    {
        private List<MenuEntry> callableEntries;
        private List<MenuEntry> nonCallableEntries;
        private int option = 0;


        public void Update(KeyboardState keyboard, KeyboardState keyboardPrev)
        {
            if (keyboard.IsKeyDown(Keys.Down) && !keyboardPrev.IsKeyDown(Keys.Down))
            {
                do
                {
                    option = (option + 1) % callableEntries.Count;
                }
                while (callableEntries[option].IsHidden);
            }
            else if (keyboard.IsKeyDown(Keys.Up) && !keyboardPrev.IsKeyDown(Keys.Up))
            {
                do
                {
                    option = (option + callableEntries.Count - 1) % callableEntries.Count;
                }
                while (callableEntries[option].IsHidden);
            }
            else if (keyboard.IsKeyDown(Keys.Enter) && !keyboardPrev.IsKeyDown(Keys.Enter))
            {
                callableEntries[option].Call();
            }
        }

        public void Draw(Console console)
        {
            int x = console.Width / 2;
            foreach (MenuEntry entry in callableEntries)
            {
                Text(console, x, entry.Position, entry.Text, entry.Color);
            }
            foreach (MenuEntry entry in nonCallableEntries)
            {
                Text(console, x, entry.Position, entry.Text, entry.Color);
            }
        }


        public void AddEntry(MenuEntry entry)
        {
            if (entry.IsCallable)
            {
                callableEntries.Add(entry);
                callableEntries.Sort();
            }
            else
            {
                nonCallableEntries.Add(entry);
                callableEntries.Sort();
            }
        }


        private void Text(Console console, int x, int y, string text, byte color)
        {
            if (x < 0) x += console.Width;
            if (y < 0) y += console.Height;

            int start = x - text.Length / 2;
            for (int xx = start; xx < start + text.Length; xx++)
            {
                if (xx >= 0 && xx < console.Width)
                {
                    console.Data[xx, y] = text[xx - start];
                    console.Color[xx, y] = color;
                }
            }
        }
    }
}
