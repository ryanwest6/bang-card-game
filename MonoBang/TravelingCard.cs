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
    class TravelingCard : Card
    {
        #region Fields

        //How fast the card will move.
        float speed, xMove, yMove;
        //How much time the card has to move from one place to the other.
        int time, offset;
        //Remembers index for offset.
        int offsetIndex;
        //The starting point for the card.
        Vector2 startVector;
        //Where the center of the card currently is.
        Vector2 current;

        //The finishing stack for the card.
        CardStack finish, startDeck;
        //Where the card will be placed when it has finished animating.
        CardStack finishDeck;

        //If the card is faceUp or faceDown.
        CardState face;

        //If the card has finished moving.
        bool done = false;
        //If finished, transfer this card to the new deck once, then stop.
        bool transferredCard = false;
        //For offsets only, true when the offset reachs 0.
        bool started = true;

        //Which side of the finish deck to send the card to. Top is right, bottom is left (for layouts)
        Side finishSide;

        public bool Finished { get { return done; } }

        #endregion

        #region Constructors

        //Best one, offset is how long to wait before actually moving the card and which card index.
        public TravelingCard(CardStack cardStart, int index, CardStack cardFinish, int timeToTravel, CardState newFace, int newOffset)
            : base(CardType.Bang, CardValue.Ace, Suit.Spades)
        {
            if (newOffset < 0) newOffset = 0;
            offset = newOffset;
            if (offset > 0) started = false;
            if (offset == 0)
                CreateObject(index, cardStart, cardFinish, timeToTravel, newFace, Side.TopRight, Side.TopRight);
            else CreateObjectWithOffset(index, cardStart, cardFinish, timeToTravel, newFace, Side.TopRight);
        }

        //Best one, offset is how long to wait before actually moving the card and which card index.
        public TravelingCard(CardStack cardStart, Card actualCard, CardStack cardFinish, int timeToTravel, CardState newFace, int newOffset)
            : base(CardType.Bang, CardValue.Ace, Suit.Spades)
        {
            int index = -1;
            for (int i = 0; i < cardStart.Size; i++)
                if (cardStart[i] == actualCard)
                { index = i; break; }

            if (index < 0)
                throw new Exception("The given card was not found in the cardStack.");

            if (newOffset < 0) newOffset = 0;
            offset = newOffset;
            if (offset > 0) started = false;
            if (offset == 0)
                CreateObject(index, cardStart, cardFinish, timeToTravel, newFace, Side.TopRight, Side.TopRight);
            else CreateObjectWithOffset(index, cardStart, cardFinish, timeToTravel, newFace, Side.TopRight);
        }

        //For cards that instantaneously move. (no offset required)
        public TravelingCard(CardStack cardStart, CardStack cardFinish, int timeToTravel, CardState newFace)
            : base(CardType.Bang, CardValue.Ace, Suit.Spades)
        {
            CreateObject(-1, cardStart, cardFinish, timeToTravel, newFace, Side.TopRight, Side.TopRight);
        }

        #endregion

        //Used by both constructors to create the object and save space.
        private void CreateObject(int index, CardStack cardStart, CardStack cardFinish, int timeToTravel, CardState newFace, Side startDeckSide, Side finishDeckSide)
        {
            if (cardStart == cardFinish)
            { done = true; transferredCard = true; return; }
            Card c = null;
            if (index != -1)
                c = cardStart.Remove(index);
            else if (startDeckSide == Side.TopRight)
                c = cardStart.RemoveTop();
            else
                c = cardStart.RemoveBottom();
            finishDeck = cardFinish;
            finish = cardFinish;
            if (cardStart.Layout == Layout.Deck)
                startVector = cardStart.TopLeft;
            else
                startVector = new Vector2(cardStart.Location.X + (((int)Card.cardSizeX + CardStack.SPACE_BETWEEN_CARDS) * (index > -1 ? cardStart.Size : index)), cardStart.Location.Y);
            current = startVector;
            time = timeToTravel;
            face = newFace;
            finishSide = finishDeckSide;

            if (c != null)
            {
                suit = c.Suit;
                value = c.Value;
                cardType = c.CType;

                CalculateMoveAmount();
            }
            else
            { done = true; transferredCard = true; }
        }

        private void CreateObjectWithOffset(int index, CardStack cardStart, CardStack cardFinish, int timeToTravel, CardState newFace, Side finishDeckSide)
        {
            startDeck = cardStart;
            if (cardStart == cardFinish)
            { done = true; transferredCard = true; return; }
            Card c = cardStart[index];
            offsetIndex = index;
            finish = cardFinish;
            time = timeToTravel;
            face = newFace;
            finishSide = finishDeckSide;
            finishDeck = cardFinish;

            if (c != null)
            {
                suit = c.Suit;
                value = c.Value;
                cardType = c.CType;
            }
            else
            { done = true; transferredCard = true; }
        }

        //Moves the card towards the finish point the correct amount of pixels.
        public void Update(int speed)
        {
            if (!started)
            {
                offset--;
                time = speed;
                if (offset == 0)
                {
                    if (startDeck.Layout == Layout.Deck)
                        startVector = startDeck.TopLeft;
                    else
                        startVector = new Vector2(startDeck.Location.X + (((float)Card.cardSizeX + CardStack.SPACE_BETWEEN_CARDS) * (offsetIndex)), startDeck.Location.Y);
                    current = startVector;
                    startDeck.Remove(offsetIndex);
                    started = true;
                }
            }
            else if (!done)
            {
                time--;
                CalculateMoveAmount();

                current.X += xMove;
                current.Y += yMove;

                if (time < 0)
                    done = true;
            }
            if (done && !transferredCard)
            {
                if (finishSide == Side.TopRight) finishDeck.Add(this, true);
                else finishDeck.Add(this, false);
                transferredCard = true;
            }
        }

        //Calculates the amount needed to move to end at the right pile position.
        private void CalculateMoveAmount()
        {
            float fX, fY;

            if (finish.Layout == Layout.Hand)
            {
                fX = finish.Location.X + (((float)Card.cardSizeX + CardStack.SPACE_BETWEEN_CARDS) * (finish.Size));
                fY = finish.Location.Y;
            }
            else if (finishSide == Side.TopRight) { fX = finish.TopLeft.X; fY = finish.TopLeft.Y; }
            else { fX = finish.Location.X; fY = finish.Location.Y; }

            float xLength = -(current.X - finish.TopLeft.X);
            float yLength = -(current.Y - finish.TopLeft.Y);

            if (finish.Layout == Layout.Hand)
            {
                xLength = (-current.X + finish.Location.X + (((float)Card.cardSizeX + CardStack.SPACE_BETWEEN_CARDS) * (finish.Size)));
                yLength = -(current.Y - finish.Location.Y);
            }

            float hypoLenth = (float)Math.Sqrt(((current.X - fX) * (current.X - fX)) + ((current.Y - fY) * (current.Y - fY)));
            speed = hypoLenth / time;
            xMove = xLength / time;
            yMove = yLength / time;

            if (float.IsNaN(xMove)) //Solves 1 tick invisible card glitch.
            {
                xMove = 0;
                yMove = 0;
                speed = 0;
            }
        }

        //Draws the moving card.
        public void Draw()
        {
            if (offset == 0)
            {
                if (face == CardState.FaceDown)
                    MainProgram.spriteBatch.Draw(MainProgram.game.faceDownCard, new Rectangle((int)current.X, (int)current.Y, (int)cardSizeX, (int)cardSizeY), Color.White);
                else
                {
                    MainProgram.spriteBatch.Draw(MainProgram.game.card, new Rectangle((int)current.X, (int)current.Y, (int)cardSizeX, (int)cardSizeY), Color.White);
                    switch (Suit)
                    {
                        case Suit.Clubs:
                            DrawTextOnCard(MainProgram.game.clubs, new Point((int)current.X, (int)current.Y));
                            break;
                        case Suit.Hearts:
                            DrawTextOnCard(MainProgram.game.hearts, new Point((int)current.X, (int)current.Y));
                            break;
                        case Suit.Diamonds:
                            DrawTextOnCard(MainProgram.game.diamonds, new Point((int)current.X, (int)current.Y));
                            break;
                        case Suit.Spades:
                            DrawTextOnCard(MainProgram.game.spades, new Point((int)current.X, (int)current.Y));
                            break;
                    }
                }
            }
        }
    }
}