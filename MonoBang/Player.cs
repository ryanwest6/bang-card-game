using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace ERS
{

    //Base class for a player that contains cards, lives, role, etc. Inhererited by Player and human player.
    class Player
    {

        //the max life for the player (3 or 4, +1 if the sheriff)
        protected int maxLife = 4;
        //current life for the player
        protected int life = 4;
        //the cards the player has in his/her hand and on the table
        protected CardStack hand, tableCards;
        protected bool alive = true, finishedTurn = false, playedCard = false, hasDrawn = false, hasThought = true;
        //If the players death functions (rewards, returning cards to the pile, etc.) are  over.
        protected bool deathFunctionsFinished = false;
        //Prevents the player from playing more than 1 bang card per turn.
        protected bool hasPlayedBang = false, hasUsedBarrel = false;
        protected string name; //What the player's name is.
        protected string genderStr; //Gender of the player, set to 'their' if neither.
                                    //The role of the Player
        protected CharacterRole role = CharacterRole.None;


        public int MaxLife { get { return maxLife; } set { maxLife = value; } }
        public int Life { get { return life; } set { life = value; } }
        public CardStack Hand { get { return hand; } }
        public CardStack TableCards { get { return tableCards; } }
        public string Name { get { return name; } }
        public string Gender { get { return genderStr; } }
        public CharacterRole Role { get { return role; } set { role = Role; } }

        public bool HasFinishedTurn { get { return finishedTurn; } set { finishedTurn = value; } }
        public bool HasPlayedCard { get { return playedCard; } set { playedCard = value; } }
        public bool HasDrawn { get { return hasDrawn; } set { hasDrawn = value; } }
        public bool HasUsedBarrel { get { return hasUsedBarrel; } set { hasUsedBarrel = value; } }
        public bool IsAlive { get { return alive; } }
        public bool FinishedDeathFunctions { get { return deathFunctionsFinished; } set { deathFunctionsFinished = value; } }

        public bool HasThought { get { return hasThought; } set { hasThought = value; } }

        public static int TABLECARDS_OFFSET = 290;//360;

        public Player(string newName, Point newLocation)
        {
            name = newName;
            hand = new CardStack(newLocation.X, newLocation.Y, Layout.Deck);
            tableCards = new CardStack(newLocation.X + TABLECARDS_OFFSET, newLocation.Y, Layout.Hand);
            genderStr = "their";
        }

        public Player(string newName, bool isMale, Point newLocation)
        {
            name = newName;
            hand = new CardStack(newLocation.X, newLocation.Y, Layout.Deck);
            tableCards = new CardStack(newLocation.X + TABLECARDS_OFFSET, newLocation.Y, Layout.Hand);
            if (isMale)
                genderStr = "his";
            else genderStr = "her";
        }

        public void Update(MouseState mouse, MouseState oldMouse)
        {
            if (GameController.debug)
                hand.Layout = Layout.Hand;
            else
                hand.Layout = Layout.Deck;
            hand.Update(mouse, oldMouse);
            tableCards.Update(mouse, oldMouse);

            //DEBUG
            if (life > 0 && mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released &&
                new Rectangle((int)Hand.TopLeft.X, (int)Hand.TopLeft.Y, (int)Card.cardSizeX + (int)(Hand.Location.X - Hand.TopLeft.X), (int)Card.cardSizeY + (int)(Hand.Location.Y - Hand.TopLeft.Y)).Contains(new Point(oldMouse.X, oldMouse.Y)))
                life--;

            if (life == 0) alive = false;
        }

        //Resets the player for a new game.
        public void Reset()
        {
            life = 4;
            alive = true;
            deathFunctionsFinished = false;
            finishedTurn = false;
            playedCard = false;
            hasDrawn = false;
        }


        //Gets the distance one player is from another.
        public int GetDistanceAway(Player[] players, Player otherPlayer)
        {
            int thisIndex = -1, otherIndex = -1;
            for (int i = 0; i < players.Length; i++) //Identify indexes of both players.
            {
                if (players[i] == this)
                    thisIndex = i;
                else if (players[i] == otherPlayer)
                    otherIndex = i;
            }

            if (thisIndex == -1 || otherIndex == -1)
                return -1;

            int distanceRight = 0, distanceLeft = 0, helper = thisIndex; //Identify the closest distance to the player.
            while (helper != otherIndex)
            {
                helper++;
                if (helper >= players.Length)
                    helper = 0;
                distanceRight++;
            }
            helper = thisIndex;
            while (helper != otherIndex)
            {
                helper--;
                if (helper < 0)
                    helper = players.Length - 1;
                distanceLeft++;
            }
            int distance = distanceLeft > distanceRight ? distanceRight : distanceLeft;

            if (otherPlayer.TableCards.Contains(CardType.Mustang))
                distance++;
            if (TableCards.Contains(CardType.Scope) && distance > 1)
                distance--;

            return distance;
        }

        //If the player has a gun (scope) that can reach the player, returns true;
        protected bool CanReachPlayer(Player[] players, int otherPlayerIndex)
        {
            int distance = GetDistanceAway(players, players[otherPlayerIndex]);

            switch (distance)
            {
                case 0:
                    { }
                    break;
                case 1:
                    return true;
                case 2:
                    if (tableCards.Contains(CardType.Schofield) || tableCards.Contains(CardType.Remington) || tableCards.Contains(CardType.RevCarabine) ||
                        hand.Contains(CardType.Winchester))
                        return true; break;
                case 3:
                    if (tableCards.Contains(CardType.Remington) || tableCards.Contains(CardType.RevCarabine) || tableCards.Contains(CardType.Winchester))
                        return true; break;
                case 4:
                    if (tableCards.Contains(CardType.RevCarabine) || tableCards.Contains(CardType.Winchester))
                        return true; break;
                case 5:
                    if (tableCards.Contains(CardType.Winchester))
                        return true; break;
                default:
                    return false;
            }
            return false;
        }

        //Creates an array with only reachable players.
        protected Player[] GetReachablePlayers(Player[] players)
        {
            Player[] reachable = new Player[players.Length];

            int num = 0;

            if (players.Length <= 1)
                return null;

            for (int i = 0; i < players.Length; i++)
                if (players[i] != this && CanReachPlayer(players, i))
                {
                    reachable[num] = players[i];
                    num++;
                }
            players = new Player[num];
            for (int i = 0; i < players.Length; i++)
                players[i] = reachable[i];

            if (players.Length < 1)
            {
                return null;
            }
            return players;
        }

        public virtual PlayCard Play(Player[] players, PlayType typeOfPlay)
        {
            //implement in subclasses
            return null;
        }

        public virtual PlayCard[] DiscardExtras(Player[] players)
        {
            //implement in subclasses
            return null;
        }
    }
}