﻿using ASCII_FPS.GameComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;


namespace ASCII_FPS.UI
{
    public class UIKeybinds : UIElement
    {
        private readonly FieldInfo[] fields;
        private bool waitingForKey = false;
        private int option = 0;


        public Action BackAction { private get; set; }


        public UIKeybinds()
        {
            fields = typeof(Keybinds).GetFields();
        }


        public override void Update(KeyboardState keyboard, KeyboardState keyboardPrev)
        {
            if (waitingForKey)
            {
                Keys[] keys = keyboard.GetPressedKeys();
                if (keys.Length > 0 && !keyboardPrev.IsKeyDown(keys[0]))
                {
                    fields[option - 1].SetValue(null, keys[0]);
                    Assets.dingDing.Play();
                    waitingForKey = false;
                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Down) && !keyboardPrev.IsKeyDown(Keys.Down))
                {
                    Assets.ding.Play();
                    option = (option + 1) % (fields.Length + 1);
                }
                else if (keyboard.IsKeyDown(Keys.Up) && !keyboardPrev.IsKeyDown(Keys.Up))
                {
                    Assets.ding.Play();
                    option = (option + fields.Length) % (fields.Length + 1);
                }
                else if (keyboard.IsKeyDown(Keys.Enter) && !keyboardPrev.IsKeyDown(Keys.Enter))
                {
                    if (option == 0)
                    {
                        Assets.ding.Play();
                        BackAction.Invoke();
                    }
                    else
                    {
                        waitingForKey = true;
                    }
                }
            }
        }

        public override void Draw(Console console)
        {
            for (int x = 0; x < console.Width; x++)
            {
                for (int y = 0; y < console.Height; y++)
                {
                    console.Data[x, y] = ' ';
                }
            }

            int c = console.Width / 2;

            UIUtils.Text(console, c, 12, "Back", option == 0 ? UIUtils.colorLightBlue : UIUtils.colorGray);
            for (int i = 0; i < fields.Length; i++)
            {
                string keyName = fields[i].GetCustomAttribute<KeyNameAttribute>().Name;
                string text = keyName + " - " + (waitingForKey && option == i + 1 ? "< Press key >" : fields[i].GetValue(null));
                byte color = option == i + 1 ? UIUtils.colorLightBlue : UIUtils.colorGray;
                UIUtils.Text(console, c, 16 + 2 * i, text, color);
            }
        }
    }
}