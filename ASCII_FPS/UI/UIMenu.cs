﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace ASCII_FPS.UI
{
    public class UIMenu : UIElement
    {
        private readonly UIPosition boundsStart;
        private readonly UIPosition boundsEnd;
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


        public UIMenu(UIPosition boundsStart, UIPosition boundsEnd)
        {
            this.boundsStart = boundsStart;
            this.boundsEnd = boundsEnd;
            entries = new List<MenuEntry>();
        }

        public UIMenu() : this(UIPosition.TopLeft, UIPosition.BottomRight) { }


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
            Point start = boundsStart.GetPosition(console);
            Point end = boundsEnd.GetPosition(console);
            for (int x = start.X; x <= end.X; x++)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    console.Data[x, y] = ' ';
                }
            }

            int c = (end.X - start.X + 1) / 2;
            foreach (MenuEntry entry in entries)
            {
                if (!entry.IsHidden)
                {
                    byte color = entry == entries[option] ? entry.ColorSelected : entry.Color;
                    UIUtils.Text(console, c, entry.Position, entry.Text, color);
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
    }
}
