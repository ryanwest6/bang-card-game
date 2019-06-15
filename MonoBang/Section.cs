using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ERS
{
    class Section
    {
        //A copy of the previous section.
        protected Section prevSection;
        //Lets the user input instructions to the game with the keyboard.
        protected KeyboardState keyboard, oldKeyboard;

        protected MouseState mouse, oldMouse;
        //Lets the user input instructions with the gamepad.
        protected GamePadState pad1, oldPad1;

        public GamePadState Pad1 //Property of pad1
        {
            get { return pad1; }
            set { pad1 = value; }
        }
        public GamePadState OldPad1 //Property of oldPad1
        {
            get { return oldPad1; }
            set { oldPad1 = value; }
        }
        public KeyboardState Keys1 //Property of keyboard
        {
            get { return keyboard; }
            set { keyboard = value; }
        }
        public KeyboardState OldKeys1 //Property of oldKeyboard
        {
            get { return oldKeyboard; }
            set { oldKeyboard = value; }
        }
        public Section PrevSection //Property of prevSection
        {
            get { return prevSection; }
            set { prevSection = value; }
        }

        public Section(Section s)
        {
            prevSection = s;

            if (prevSection != null)
            {
                keyboard = Keyboard.GetState(PlayerIndex.One);
                oldKeyboard = keyboard;
                pad1 = GamePad.GetState(PlayerIndex.One);
                oldPad1 = pad1;
                mouse = Mouse.GetState();
                oldMouse = mouse;
            }
        }

        //Draws the object to the screen.
        public virtual void Draw() { }
        //Updates the methods properties each tick.
        public virtual Section Update() { return this; }
        //Prepares the section to be destroyed.
        public virtual void End() { }

        //Updates the states of the keyboard, mouse, and gamepads.
        protected void UpdateStates()
        {
            keyboard = Keyboard.GetState();
            mouse = Mouse.GetState();
            pad1 = GamePad.GetState(PlayerIndex.One);
        }

        //Updates the old states of the keyboard, mouse, and gamepads.
        protected void UpdateOldStates()
        {
            oldKeyboard = keyboard;
            oldMouse = mouse;
            oldPad1 = pad1;
        }
    }
}
