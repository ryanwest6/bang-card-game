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

    class PlayCard
    {
        private int index, otherIndex; //The index of the card to return.
        private Player who; //Who the card is being issued on, if a single player.
        private Card card; //The actual card being played.
        private Card otherCard; //The card being played ON.

        public int Index { get { return index; } }
        public int OtherIndex { get { return otherIndex; } }
        public Player Who { get { return who; } }
        public CardType CType { get { return card.CType; } }
        public Card Card { get { return card; } }
        public Card OtherCard { get { return otherCard; } }

        //Sent by players to the gamescreen, 'whichPlayer' is only used for cards directed towards a particular player.
        public PlayCard(int newIndex, Player whichPlayer, Card whichCard)
        {
            index = newIndex;
            who = whichPlayer;
            card = whichCard;
        }

        //Sent by players to the gamescreen, 'whichPlayer' is only used for cards directed towards a particular player.
        public PlayCard(int newIndex, Player whichPlayer, Card whichCard, Card whichOtherCard, int newOtherIndex)
        {
            index = newIndex;
            who = whichPlayer;
            card = whichCard;
            otherCard = whichOtherCard;
            otherIndex = newOtherIndex;
        }
    }
}
