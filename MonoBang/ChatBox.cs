using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ERS
{
    [DebuggerDisplay("Line: {log[lineNumber-1]}")]
    class ChatBox : Draggable
    {
        private string[] log; //Things that have been written to the chatbox.
        private int lineNumber = 0; //Which number the log is on (logNum).

        private bool finishedVisual = true;
        private int timer = 0;
        private int initRelativeTextY = 140;

        public ChatBox(int x, int y)
            :base(new Rectangle(x, y, BangGame.SCREEN_WIDTH - 30, 144))
        {
            log = new string[100000];
        }

        //Allows things to be written to the chat.
        public void Write(string text)
        {
            log[lineNumber] = text;
            lineNumber++;
            finishedVisual = false;
            timer = 10;
            initRelativeTextY = 160;
        }

        //Clears the chat history.
        public void Clear()
        {
            log = new string[100000];
            lineNumber = 0;
        }

        public override void Update(MouseState mouse, MouseState oldMouse)
        {
            timer--;
            if (timer == 0)
                finishedVisual = true;

 	        base.Update(mouse, oldMouse);
        }

        //Draws the chatbox visuals as well as text.
        public void Draw()
        {
            int linesShownLeft = 7;

            if (finishedVisual)
                initRelativeTextY = 140;
            else
            {
                linesShownLeft++;
                initRelativeTextY -= 2;
            }

            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), Color.Black);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, rect, Color.WhiteSmoke);

            
            int y = initRelativeTextY;
            for (int i = lineNumber; linesShownLeft >= 0 && i >= 0; i--)
            {
                if (log[i] != null)
                    MainProgram.spriteBatch.DrawString(MainProgram.game.chatFont, log[i], new Vector2(rect.X + 5, rect.Y + y), Color.Black);
                y -= 20;
                linesShownLeft--;
            }

            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(rect.X, rect.Y - 20, rect.Width + 40, 18), MainProgram.game.defaultColor);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(rect.X, rect.Bottom + 2, rect.Width + 40, 18), MainProgram.game.defaultColor);
        }
    }
}
