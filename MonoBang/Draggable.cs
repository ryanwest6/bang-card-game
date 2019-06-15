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
    //Superclass for any draggable item on the screen. (Cardstack, chatbox) Not really necessary, just fun.
    class Draggable
    {

        //The physical location of the item on the screen.
        protected Vector2 location;
        //The rectangle of the item, if necessary.
        protected Rectangle rect;
        //If the deck is selected by the mouse.
        protected bool selected;
        //True if the draggable uses the rectangle instead of the vector2. 
        private bool usesRect;

        public Vector2 Location { get { return location; } set { location = value; } }
        public virtual Vector2 TopLeft { get { return new Vector2(location.X, location.Y); } }

        //If using this constuctor, the only draggable area depends on the size of a card in the top left corner.
        public Draggable(int x, int y)
        {
            location.X = x;
            location.Y = y;
            usesRect = false;
        }

        public Draggable(Rectangle r)
        {
            rect = r;
            usesRect = true;
        }

        //Moves the deck physically.
        public void Move(float x, float y)
        {
            if (!usesRect)
            {
                location.X += x;
                location.Y += y;
            }
            else
            {
                rect.X += (int)x;
                rect.Y += (int)y;
            }
        }

        //Make a virtual bool InDragBox() method to see if the mouse is in the dragbox.

        //Lets the user move the cardstack.
        public virtual void Update(MouseState mouse, MouseState oldMouse)
        {
            if (usesRect && mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && rect.Contains(new Point(oldMouse.X, oldMouse.Y)))
                selected = true;
            else if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)Card.cardSizeX + (int)(location.X - TopLeft.X), (int)Card.cardSizeY + (int)(location.Y - TopLeft.Y)).Contains(new Point(oldMouse.X, oldMouse.Y)))
                selected = true;

            if (mouse.LeftButton == ButtonState.Released)
                selected = false;

            if (selected && mouse.LeftButton == ButtonState.Pressed && rect.Contains(new Point(oldMouse.X, oldMouse.Y)))
                Move(mouse.X - oldMouse.X, mouse.Y - oldMouse.Y);
            else if (selected && mouse.LeftButton == ButtonState.Pressed && new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)Card.cardSizeX + (int)(location.X - TopLeft.X), (int)Card.cardSizeY + (int)(location.Y - TopLeft.Y)).Contains(new Point(oldMouse.X, oldMouse.Y)))
                Move(mouse.X - oldMouse.X, mouse.Y - oldMouse.Y);
        }

    }
}
