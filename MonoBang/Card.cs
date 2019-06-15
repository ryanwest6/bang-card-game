using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERS
{
    enum Suit { Spades, Hearts, Clubs, Diamonds }
    enum CardValue { Ace, N2, N3, N4, N5, N6, N7, N8, N9, N10, Jack, Queen, King }
    enum CardType { Bang, Missed, Gatling, Indians, Beer, Saloon, Schofield, Remington, RevCarabine, Winchester, Mustang, Scope, CatBalou, Panic, Barrel,
                    Stagecoach, WellsFargo, Duel }
    enum Layout { Deck, Hand }
    enum CardState { FaceUp, FaceDown }
    enum Side { TopRight, BottomLeft }

    //[DebuggerDisplay("Suit = {suit}, Value = {value}")]
    [DebuggerDisplay("Type: {cardType}")]
    class Card
    {
        public static int CARD_SIZE_X_DEFAULT = 40, CARD_SIZE_Y_DEFAULT = 56;
        public static double cardSizeX = CARD_SIZE_X_DEFAULT * 1.35, cardSizeY = CARD_SIZE_Y_DEFAULT * 1.35;

        //The value of the card.
        protected CardValue value;
        //The suit of the card.
        protected Suit suit;
        //The type of the card.
        protected CardType cardType;

        public CardValue Value { get { return value; } }
        public Suit Suit { get { return suit; } }
        public CardType CType { get { return cardType; } }

        public Card(CardType cType, CardValue cardValue, Suit cardSuit)
        {
            value = cardValue;
            suit = cardSuit;
            cardType = cType;
        }

        public Card(Card old)
        {
            value = old.value;
            suit = old.suit;
        }

        //Returns a string with the suit and value.
        public override string ToString()
        {
            string num = value.ToString();
            if (value > (CardValue)0 && value < (CardValue)10)
                num = num.Remove(0, 1);

            //return num + " of " + suit.ToString();
            return GetFaceCardTypeString() + ", " + num + " of " + suit.ToString();
        }

        //Gets the single character version of the card value.
        public string GetShortHandValue()
        {
            switch (value)
            {
                case CardValue.Ace: return "A";
                case CardValue.Jack: return "J";
                case CardValue.King: return "K";
                case CardValue.N10: return "10";
                case CardValue.N2: return "2";
                case CardValue.N3: return "3";
                case CardValue.N4: return "4";
                case CardValue.N5: return "5";
                case CardValue.N6: return "6";
                case CardValue.N7: return "7";
                case CardValue.N8: return "8";
                case CardValue.N9: return "9";
                case CardValue.Queen: return "Q";
                default: return "NULL";
            }
        }

        //Gets the string to be written on the face card.
        public string GetFaceCardTypeString()
        {
            switch (cardType)
            {
                case CardType.Bang: return "BANG!";
                case CardType.Missed: return "MISS!";
                case CardType.Gatling: return "GATLING";
                case CardType.Indians: return "INDIANS";
                case CardType.Beer: return "BEER";
                case CardType.Saloon: return "SALOON";
                case CardType.Schofield: return "SCOF";
                case CardType.Remington: return "REMMY";
                case CardType.RevCarabine: return "CARB";
                case CardType.Winchester: return "WC";
                case CardType.Mustang: return "HORSE";
                case CardType.Scope: return "SCOPE";
                case CardType.CatBalou: return "CAT B.";
                case CardType.Barrel: return "BARREL";
                case CardType.Stagecoach: return "WAGON";
                case CardType.WellsFargo: return "FARGO";
                case CardType.Panic: return "PANIC";
                case CardType.Duel: return "DUEL";
                default: return "NULL";
            }
        }

        //Gets the full card type string (Schofield instead of scofld)
        public string GetFullCardTypeString()
        {
            switch (cardType)
            {
                case CardType.Bang: return "bang";
                case CardType.Missed: return "miss";
                case CardType.Gatling: return "gatling";
                case CardType.Indians: return "indians";
                case CardType.Beer: return "beer";
                case CardType.Saloon: return "saloon";
                case CardType.Schofield: return "schofield";
                case CardType.Remington: return "remington";
                case CardType.RevCarabine: return "rev. carabine";
                case CardType.Winchester: return "winchester";
                case CardType.Mustang: return "mustang";
                case CardType.Scope: return "scope";
                case CardType.CatBalou: return "cat balou";
                case CardType.Barrel: return "barrel";
                case CardType.Stagecoach: return "stagecoach";
                case CardType.WellsFargo: return "wells fargo";
                case CardType.Panic: return "panic";
                case CardType.Duel: return "duel";
                default: return "NULL";
            }
        }

        //Draws the card in the specified position relative to the position.
        public void Draw(Point location, bool drawText)
        {
            MainProgram.spriteBatch.Draw(MainProgram.game.card, new Rectangle(location.X, location.Y, (int)cardSizeX, (int)cardSizeY), Color.White);
            if (drawText)
                switch (suit)
                {
                    case Suit.Clubs:
                        DrawTextOnCard(MainProgram.game.clubs, location);
                        break;
                    case Suit.Hearts:
                        DrawTextOnCard(MainProgram.game.hearts, location);
                        break;
                    case Suit.Diamonds:
                        DrawTextOnCard(MainProgram.game.diamonds, location);
                        break;
                    case Suit.Spades:
                        DrawTextOnCard(MainProgram.game.spades, location);
                        break;
                }
        }

        //Draws the value and suit of the card correctly positioned in the center of the card.
        protected void DrawTextOnCard(Texture2D suit, Point location)
        {
            const double HI = 1.5;

            string val = GetShortHandValue(), typeText = GetFaceCardTypeString();

            /*int width = (int)MainProgram.game.cardFont.MeasureString(val).X + (int)(suit.Bounds.Width / HI) + 1;
            int textX = location.X + ((MainProgram.game.card.Bounds.Width - width) / 2);
            int suitX = textX + width - (int)(suit.Bounds.Width / HI);*/

            int width = (int)MainProgram.game.smallFont.MeasureString(val).X + 1;
            int textX = location.X + (int)cardSizeX - (int)((suit.Bounds.Width) / HI) - width - 2;
            int suitX = textX + width;


            if (val == "10") //This is needed so the club will not go over the edge of the card.
                textX++;

            MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, val, new Vector2(textX, location.Y + (int)cardSizeY - 14), Color.Black);
            MainProgram.spriteBatch.Draw(suit, new Rectangle(suitX, location.Y + (int)cardSizeY - 12, (int)(suit.Bounds.Width / HI), (int)(suit.Bounds.Height / HI)), Color.White);

            SpriteFont typeFont = MainProgram.game.cardFont;
            float scale = ((int)cardSizeX - 2) / typeFont.MeasureString(typeText).X;
            int typeX = location.X + 1;// (int)((CARD_SIZE_X - (MainProgram.game.typeFont.MeasureString(typeText).X / 2)) / 2);

            MainProgram.spriteBatch.DrawString(typeFont, typeText, new Vector2(typeX, location.Y), Color.Black, 0, new Vector2(0, 0),
                new Vector2(scale, ( 1.1f * (float)cardSizeY / 38.0f)), SpriteEffects.None, 0);

            int range = 0;
            switch (cardType)
            {
                case (CardType.Schofield):
                    range = 2; break;
                case (CardType.Remington):
                    range = 3; break;
                case (CardType.RevCarabine):
                    range = 4; break;
                case (CardType.Winchester):
                    range = 5; break;
            }
            string rangeStr = "Range: " + range.ToString();
            float scaleRange = ((int)cardSizeX - 16) / typeFont.MeasureString(rangeStr).X;
            if (range != 0)
                MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, rangeStr, new Vector2(typeX+2, location.Y + (int)(cardSizeY * .6f)), Color.Black);
               // MainProgram.spriteBatch.DrawString(typeFont, rangeStr, new Vector2(typeX+3, location.Y + (int)(cardSizeY*.55f)), Color.Black, 0, new Vector2(0, 0),
               //     new Vector2(scaleRange, (.4f * (float)cardSizeY / 38.0f)), SpriteEffects.None, 0);
        }

    }
}
