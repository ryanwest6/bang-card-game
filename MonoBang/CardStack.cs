using System;
using System.Collections.Generic;
using System.Linq;
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
    [DebuggerDisplay("Size: {size}")]
    class CardStack : Draggable
    {
        #region Fields

        public const int DECK_SIZE = 52;
        public const int SPACE_BETWEEN_CARDS = 3;

        public override Vector2 TopLeft { get { return new Vector2(location.X - (size / 3), location.Y - (size / 3)); } }

        private Card[] cards;
        //The number of cards in the hand.
        private int size = 0;
        //Used to appropriately time card giving and taking actions.
        private int timer = 0;

        //Which pile to deal to next, used in the Deal() method.
        private int nextPileToDeal = 0;

        //The layout of the cardStack, whether a deck or fanned out.
        private Layout layout;

        public Layout Layout { get { return layout; } set { layout = value; } }

        public Card this[int index] { get { return cards[index]; } }
        public int Size { get { return size; } }

        #endregion

        #region Constructors

        public CardStack(int x, int y, bool newDeck = false)
            :base(x, y)
        {
            cards = new Card[DECK_SIZE];
            if (newDeck)
                Fill();
        }

        public CardStack(int x, int y, Layout cardLayout)
            : base(x, y)
        {
            cards = new Card[DECK_SIZE];
            layout = cardLayout;
        }

        /* Other constructors
        public CardStack(Card[] startingCards, int x, int y)
            :base(x, y)
        {
            cards = new Card[DECK_SIZE];
            if (startingCards != null)
            {
                for (int i = 0; i < startingCards.Length; i++)
                { cards[i] = startingCards[i]; size++; }
            }
        }

        public CardStack(CardStack startingCards)
        {
            cards = new Card[DECK_SIZE];
            size = startingCards.size;

            this.location.X = startingCards.location.X;
            this.location.Y = startingCards.location.Y;

            for (int i = 0; i < startingCards.size; i++)
                cards[i] = startingCards[i];
        }*/

        #endregion

        //Fills the deck with 52 cards.
        private void Fill()
        {
            int currentType = 1; //2 for 1 Gatling card.
            int currentSuit = 0;

            int index = 0;

            cards[0] = new Card(CardType.Indians, (CardValue)0, (Suit)currentSuit);
            cards[1] = new Card(CardType.Indians, (CardValue)1, (Suit)currentSuit);
            cards[2] = new Card(CardType.Indians, (CardValue)2, (Suit)currentSuit);
            cards[3] = new Card(CardType.Gatling, (CardValue)3, (Suit)currentSuit);
            cards[4] = new Card(CardType.Beer, (CardValue)4, (Suit)currentSuit);
            cards[5] = new Card(CardType.Beer, (CardValue)5, (Suit)currentSuit);
            cards[6] = new Card(CardType.Beer, (CardValue)6, (Suit)currentSuit);
            cards[7] = new Card(CardType.Beer, (CardValue)7, (Suit)currentSuit);
            cards[8] = new Card(CardType.Beer, (CardValue)8, (Suit)currentSuit);
            cards[9] = new Card(CardType.Beer, (CardValue)9, (Suit)currentSuit);
            cards[10] = new Card(CardType.Saloon, (CardValue)10, (Suit)currentSuit);
            cards[11] = new Card(CardType.Schofield, (CardValue)11, (Suit)currentSuit);
            cards[12] = new Card(CardType.Schofield, (CardValue)12, (Suit)currentSuit);
            cards[13] = new Card(CardType.Schofield, (CardValue)0, (Suit)0);
            cards[14] = new Card(CardType.Winchester, (CardValue)1, (Suit)1);
            cards[15] = new Card(CardType.Remington, (CardValue)2, (Suit)1);
            cards[16] = new Card(CardType.RevCarabine, (CardValue)3, (Suit)1);
            cards[17] = new Card(CardType.Mustang, (CardValue)4, (Suit)1);
            cards[18] = new Card(CardType.Mustang, (CardValue)5, (Suit)1);
            cards[19] = new Card(CardType.Scope, (CardValue)6, (Suit)1);
            cards[20] = new Card(CardType.Scope, (CardValue)7, (Suit)1);
            cards[21] = new Card(CardType.CatBalou, (CardValue)8, (Suit)1);
            cards[22] = new Card(CardType.CatBalou, (CardValue)9, (Suit)1);
            cards[23] = new Card(CardType.CatBalou, (CardValue)10, (Suit)1);
            cards[24] = new Card(CardType.CatBalou, (CardValue)11, (Suit)1);
            cards[25] = new Card(CardType.Barrel, (CardValue)12, (Suit)1);
            cards[26] = new Card(CardType.Barrel, (CardValue)0, (Suit)2);
            cards[27] = new Card(CardType.Stagecoach, (CardValue)1, (Suit)2);
            cards[28] = new Card(CardType.Stagecoach, (CardValue)2, (Suit)2);
            cards[29] = new Card(CardType.WellsFargo, (CardValue)3, (Suit)2);
            cards[30] = new Card(CardType.Panic, (CardValue)4, (Suit)2);
            cards[31] = new Card(CardType.Panic, (CardValue)5, (Suit)2);
            cards[32] = new Card(CardType.Panic, (CardValue)6, (Suit)2);
            cards[33] = new Card(CardType.Panic, (CardValue)7, (Suit)2);
            cards[34] = new Card(CardType.Duel, (CardValue)8, (Suit)2);
            cards[35] = new Card(CardType.Duel, (CardValue)9, (Suit)2);
            cards[36] = new Card(CardType.Duel, (CardValue)10, (Suit)2);
            index = 11;
            currentSuit = 2;

            while (currentSuit < 4)
            {
                for (int i = index + (currentSuit * 13); i < 13 + (currentSuit * 13); i++)
                {
                    cards[i] = new Card((CardType)currentType, (CardValue)(i - currentSuit * 13), (Suit)currentSuit);
                    currentType++;
                    if (currentType > 1) 
                        currentType = 0;
                }
                currentSuit++;
                index = 0;
            }
            size = DECK_SIZE;
        }

        //Randomizes the order of the stack.
        public void Shuffle()
        {
            Card[] filler = new Card[DECK_SIZE];
            int index;

            for (int i = 0; i < size; i++)
            {
                index = MainProgram.random.Next(size - 1 - i);
                filler[i] = cards[index];
                RemoveCard(index, false);
                MainProgram.Swap(cards, index, size - 1 - i);
            }
            cards = filler;
        }

        //Returns true if the cardstack contains that type of card, and false if not.
        public bool Contains(CardType whichType)
        {
            for (int i = 0; i < size; i++)
                if (cards[i].CType == whichType)
                    return true;
            return false;
        }

        //Returns true if the cardstack contains that type of card (and returns the index of that card), and false if not.
        public bool Contains(CardType whichType, out int index)
        {
            for (int i = 0; i < size; i++)
                if (cards[i].CType == whichType)
                { index = i; return true; }
            index = -1;
            return false;
        }

        //Returns true if the cardstack contains that type of card (and returns the indexes of those), and false if not.
        public bool Contains(CardType whichType, out int[] indexes)
        {
            indexes = new int[DECK_SIZE];
            int num = 0;

            for (int i = 0; i < size; i++)
                if (cards[i].CType == whichType)
                { indexes[num] = i; num++; }

            if (indexes.Length == 0)
                return false;

            int[] finalIndexes = new int[num];
            for (int i = 0; i < num; i++)
                finalIndexes[i] = indexes[i];
            indexes = finalIndexes;
            return true;
        }

        //Deals all cards out of the deck cardStack and into the piles cardStacks, over time.
        public TravelingCard Deal(int timeBetween, int cardSpeed, CardStack[] piles)
        {
            timer++;
            if (timeBetween < 1)
                timeBetween = 1;

            if (timer % timeBetween == 0)
            {
                nextPileToDeal++;
                if (nextPileToDeal >= piles.Length)
                    nextPileToDeal = 0;
                return new TravelingCard(this, piles[nextPileToDeal], cardSpeed, CardState.FaceDown);
            }

            return null;
        }

        #region Add/Remove Methods

        //Adds given cards to the hand.
        public void Add(Card[] newCards)
        {
            if (newCards == null || newCards.Length == 0 || size >= 52)
                return;

            foreach (Card c in newCards)
            {
                if (c != null)
                {
                    cards[size] = c;
                    size++;
                }
            }
        }

        //Adds given cards to the hand.
        public void Add(CardStack newCards)
        {
            if (newCards == null || newCards.size == 0 || size >= 52)
                return;

            foreach (Card c in newCards.cards)
            {
                if (c != null)
                {
                    cards[size] = c;
                    size++;
                }
            }
        }

        //Adds a single card to the hand.
        public void Add(Card newCard, bool onTop = true)
        {
            if (newCard == null || size >= 52)
                return;
            if (onTop)
            {
                cards[size] = newCard;
                size++;
            }
            else
            {
                object[] re = cards;
                MainProgram.Insert(ref re, newCard, 0);
                size++;
                for (int i = 0; i < size; i++)
                {
                    if (re[i] == null)
                    { size--; }
                    cards[i] = (Card)re[i];
                }
            }
        }

        //Removes the top card from the deck and returns it.
        public Card RemoveTop()
        {
            if (size == 0)
                return null;
            
            Card top = RemoveCard(size - 1);
            return top;
        }

        //Removes the bottom card from the deck and returns it.
        public Card RemoveBottom()
        {
            if (size == 0)
                return null;

            Card bottom = RemoveCard(0);
            MainProgram.MoveNullsToBack(cards, size);
            return bottom;
        }

        //Removes a specific card from the deck and returns it.
        public Card Remove(int index)
        {
            if (size == 0)
                return null;

            Card c = RemoveCard(index);
            MainProgram.MoveNullsToBack(cards, size);
            return c;

        }

        //Removes a card, making sure to subtract from the size as well.
        private Card RemoveCard(int index, bool decreaseSize = true)
        {
            Card c = cards[index];
            cards[index] = null;
            if (decreaseSize)
                size--;
            return c;
        }


        #endregion

        //Lets the user move the cardstack.
        public override void Update(MouseState mouse, MouseState oldMouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && size > 0 && new Rectangle((int)TopLeft.X, (int)TopLeft.Y,(int) Card.cardSizeX + (int)(location.X - TopLeft.X) + ((CardStack.SPACE_BETWEEN_CARDS + (int)Card.cardSizeX) * (layout == Layout.Hand ? size - 1 : 0)), (int)Card.cardSizeY + (int)(location.Y - TopLeft.Y)).Contains(new Point(oldMouse.X, oldMouse.Y)))
                selected = true;
            if (mouse.LeftButton == ButtonState.Released)
                selected = false;

            if (selected && mouse.LeftButton == ButtonState.Pressed && size >= 0 && new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)Card.cardSizeX + (int)(location.X - TopLeft.X) + ((CardStack.SPACE_BETWEEN_CARDS + (int) Card.cardSizeX) * (layout == Layout.Hand ? size - 1 : 0)), (int)Card.cardSizeY + (int)(location.Y - TopLeft.Y)).Contains(new Point(oldMouse.X, oldMouse.Y)))
                Move(mouse.X - oldMouse.X, mouse.Y - oldMouse.Y);
        }

        //Draws all the values of the cards to the screen.
        public void DisplayCards(int x)
        {
            for (int i = 0; i < size; i++)
            {
                MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, cards[i].ToString(), new Microsoft.Xna.Framework.Vector2(x, i * 12), Microsoft.Xna.Framework.Color.White);
                switch (cards[i].Suit)
                {
                    case Suit.Clubs:
                        MainProgram.spriteBatch.Draw(MainProgram.game.clubs, new Rectangle(x + (int)MainProgram.game.smallFont.MeasureString(cards[i].ToString()).X + 3, i * 12, MainProgram.game.clubs.Bounds.Width, MainProgram.game.clubs.Bounds.Height), Color.White);
                        break;
                    case Suit.Hearts:
                        MainProgram.spriteBatch.Draw(MainProgram.game.hearts, new Rectangle(x + (int)MainProgram.game.smallFont.MeasureString(cards[i].ToString()).X + 3, i * 12, MainProgram.game.hearts.Bounds.Width, MainProgram.game.hearts.Bounds.Height), Color.White);
                        break;
                    case Suit.Diamonds:
                        MainProgram.spriteBatch.Draw(MainProgram.game.diamonds, new Rectangle(x + (int)MainProgram.game.smallFont.MeasureString(cards[i].ToString()).X + 3, i * 12, MainProgram.game.diamonds.Bounds.Width, MainProgram.game.diamonds.Bounds.Height), Color.White);
                        break;
                    case Suit.Spades:
                        MainProgram.spriteBatch.Draw(MainProgram.game.spades, new Rectangle(x + (int)MainProgram.game.smallFont.MeasureString(cards[i].ToString()).X + 3, i * 12, MainProgram.game.spades.Bounds.Width, MainProgram.game.spades.Bounds.Height), Color.White);
                        break;
                }
            }
        }

        //Draws a visual representation of the deck.
        public void DrawDeck(CardState face, bool showSize = false)
        {
            int curX = (int)location.X, curY = (int)location.Y;
            Rectangle cardRect = new Rectangle(curX, curY, (int)Card.cardSizeX, (int)Card.cardSizeY);
            if (layout == Layout.Deck)
                for (int i = 0; i < size; i++)
                {
                    cardRect.X = curX;
                    cardRect.Y = curY;
                    if (face == CardState.FaceDown)
                        MainProgram.spriteBatch.Draw(MainProgram.game.faceDownCard, new Rectangle(curX, curY, (int)Card.cardSizeX, (int)Card.cardSizeY), Color.White);
                    else if (i + 1 == size)
                        cards[i].Draw(new Point(curX, curY), true);
                    else cards[i].Draw(new Point(curX, curY), false);
                    if (i % 3 == 0)
                    {
                        curX--;
                        curY--;
                    }
                }
            else
                for (int i = 0; i < size; i++)
                {
                    cardRect.X = curX;
                    cardRect.Y = curY;
                    if (face == CardState.FaceDown)
                        MainProgram.spriteBatch.Draw(MainProgram.game.faceDownCard, new Rectangle(curX, curY, (int)Card.cardSizeX, (int) Card.cardSizeY), Color.White);
                    else
                        cards[i].Draw(new Point(curX, curY), true);
                    curX += ((int)Card.cardSizeX + SPACE_BETWEEN_CARDS);
                }

            if (showSize) MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, size.ToString(), new Vector2(location.X, location.Y + (float)Card.cardSizeY), Color.White);
        }
    }
}