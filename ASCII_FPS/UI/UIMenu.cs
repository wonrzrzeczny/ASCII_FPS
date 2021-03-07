﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace ASCII_FPS.UI
{
    public class UIMenu : UIElement
    {
        private Point boundsStart, boundsEnd;

        private readonly List<MenuEntry> entries;
        private int option = 0;

        public MenuEntry SelectedEntry
        {
            get
            {
                if (entries.Count == 0)
                {
                    return null;
                }
                return entries[option];
            }
        }


        public UIMenu(Point boundsStart, Point boundsEnd)
        {
            this.boundsStart = boundsStart;
            this.boundsEnd = boundsEnd;
            entries = new List<MenuEntry>();
        }

        public UIMenu() : this(Point.Zero, new Point(-1, -1)) { }


        public override void Update(KeyboardState keyboard, KeyboardState keyboardPrev)
        {
            while (entries[option].IsHidden || !entries[option].IsCallable)
            {
                option = (option + 1) % entries.Count;
            }

            if (keyboard.IsKeyDown(Keys.Down) && !keyboardPrev.IsKeyDown(Keys.Down))
            {
                do
                {
                    option = (option + 1) % entries.Count;
                }
                while (entries[option].IsHidden || !entries[option].IsCallable);
            }
            else if (keyboard.IsKeyDown(Keys.Up) && !keyboardPrev.IsKeyDown(Keys.Up))
            {
                do
                {
                    option = (option + entries.Count - 1) % entries.Count;
                }
                while (entries[option].IsHidden || !entries[option].IsCallable);
            }
            else if (keyboard.IsKeyDown(Keys.Enter) && !keyboardPrev.IsKeyDown(Keys.Enter))
            {
                entries[option].Call();
            }
        }

        public override void Draw(Console console)
        {
            Point start = TranslatePoint(console, boundsStart);
            Point end = TranslatePoint(console, boundsEnd);
            for (int x = start.X; x <= end.X; x++)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    console.Data[x, y] = ' ';
                }
            }

            int c = console.Width / 2;
            foreach (MenuEntry entry in entries)
            {
                if (!entry.IsHidden)
                {
                    byte color = entry == entries[option] ? entry.ColorSelected : entry.Color;
                    Text(console, c, entry.Position, entry.Text, color);
                }
            }
        }


        public void AddEntry(MenuEntry entry)
        {
            entries.Add(entry);
            entries.Sort();
        }

        public void MoveToFirst()
        {
            option = 0;
            while (entries[option].IsHidden || !entries[option].IsCallable)
            {
                option = (option + 1) % entries.Count;
            }
        }

        public void MoveToLast()
        {
            option = entries.Count - 1;
            while (entries[option].IsHidden || !entries[option].IsCallable)
            {
                option = (option + entries.Count - 1) % entries.Count;
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

        private Point TranslatePoint(Console console, Point point)
        {
            int x = point.X;
            int y = point.Y;
            x += point.X < 0 ? console.Width : 0;
            y += point.Y < 0 ? console.Height : 0;
            return new Point(x, y);
        }
    }
}
