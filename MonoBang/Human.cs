using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ERS
{
    class Human : Player
    {
        public Human(string newName, Point newLocation)
            : base(newName, newLocation) { }

        public Human(string newName, bool isMale, Point newLocation)
            : base(newName, isMale, newLocation) { }

        //Allows the player to play a card on a particular person, depending on friends/enemies.
        public override PlayCard Play(Player[] players, PlayType typeOfPlay)
        {
            switch (typeOfPlay)
            {
                case PlayType.Normal:
                    return PlayNormal(players);
                
                //for bang and indians response, the human can't decide rn; it auto protects them.
                case PlayType.BangResponse:
                    return RespondToBangCard(players);
                case PlayType.IndiansResponse:
                    return RespondToIndians(players);
                default:
                    return null;
            }
        }

        //Core functionality method for the human player choosing which card to play.
        private PlayCard PlayNormal(Player[] players)
        {
            if (players == null || players.Length == 0)
                return null;

            //Next steps:

            /* Think of how the player can take several seconds (calling this method over and over)
            to take their turn. Also, need to have new UI method to:
            1. select a card and show it's selected
            2. tell the user if that card can be played or not (if it can be played on some players, 
                highlight those players only)
            3. if the card requires a second action, let the human choose it or cancel; if it doesn't, 
                automatically play the card (like a beer)

            This will require super class Player methods that let the user know if a given card can be
            played (you can't have duplicates in the tablecards, can play a bang once at right distance)
            */

            return null;
        }

        //Discards extra cards at the end of a turn (max cards == current life)
        public override PlayCard[] DiscardExtras(Player[] players)
        {
            //return null;
            //temporarily CPU method copy

            if (hand.Size <= life)
                return null;

            bool[] usedIndex = new bool[hand.Size];

            PlayCard[] p = new PlayCard[hand.Size - life];
            int[] indexes = new int[1];
            int numFilled = 0;
            if (players.Length <= 2 && hand.Contains(CardType.Beer, out indexes))
                for (int i = 0; i < indexes.Length && numFilled < p.Length; i++)
                { p[numFilled] = new PlayCard(indexes[i], null, hand[indexes[i]]); usedIndex[indexes[i]] = true; numFilled++; }
            if (tableCards.Contains(CardType.Schofield) && hand.Contains(CardType.Schofield, out indexes))
                for (int i = 0; i < indexes.Length && numFilled < p.Length; i++)
                { p[numFilled] = new PlayCard(indexes[i], null, hand[indexes[i]]); usedIndex[indexes[i]] = true; numFilled++; }
            if (hand.Contains(CardType.Bang, out indexes))
                for (int i = 0; i < indexes.Length && numFilled < p.Length; i++)
                { p[numFilled] = new PlayCard(indexes[i], null, hand[indexes[i]]); usedIndex[indexes[i]] = true; numFilled++; }
            if (hand.Contains(CardType.Missed, out indexes))
                for (int i = 0; i < indexes.Length && numFilled < p.Length; i++)
                { p[numFilled] = new PlayCard(indexes[i], null, hand[indexes[i]]); usedIndex[indexes[i]] = true; numFilled++; }

            int rand = 0; //Pick random cards afterwords, making sure to not pick the same card twice.
            while (numFilled < p.Length)
            {
                rand = MainProgram.random.Next(hand.Size);

                if (!usedIndex[rand])
                {
                    p[numFilled] = new PlayCard(rand, null, hand[rand]);
                    usedIndex[rand] = true;
                    numFilled++;
                }
            }

            return p;
        }


        //These are CPU methods, perhaps they should be replaced later?

        //If the player has a missed card, cancels out the bang. Otherwise lose one life.
        private PlayCard RespondToBangCard(Player[] players)
        {
            int index;

            if (hand.Contains(CardType.Missed, out index))
                return new PlayCard(index, null, hand[index]);
            if (tableCards.Contains(CardType.Barrel) && !hasUsedBarrel)
            {
                return null;
            }
            life--;
            return null;
        }

        //If the player has a bang card, cancels out the indians. Otherwise lose one life.
        private PlayCard RespondToIndians(Player[] players)
        {
            int index;
            if (hand.Contains(CardType.Bang, out index))
                return new PlayCard(index, null, hand[index]);
            life--;
            return null;
        }
    }
}