using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace ERS
{
    enum PlayType { Normal, BangResponse, IndiansResponse }
    enum CharacterRole { Sheriff, Vice, Outlaw, Renegade, None }

    [DebuggerDisplay("{name}")]
    class CPU : Player
    {

        public CPU(string newName, Point newLocation)
            : base(newName, newLocation) { }

        public CPU(string newName, bool isMale, Point newLocation)
            : base(newName, isMale, newLocation) { }

        //Allows the player to play a card on a particular person, depending on friends/enemies.
        public override PlayCard Play(Player[] players, PlayType typeOfPlay)
        {
            switch (typeOfPlay)
            {
                case PlayType.Normal:
                    return PlayNormal(players);
                case PlayType.BangResponse:
                    return RespondToBangCard(players);
                case PlayType.IndiansResponse:
                    return RespondToIndians(players);
                default: 
                    return null;
            }
        }

        //Discards extra cards at the end of a turn (max cards == current life)
        public override PlayCard[] DiscardExtras(Player[] players)
        {
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

        //Plays a normal turn. This method determines the order of which cards are to be played.
        private PlayCard PlayNormal(Player[] players)
        {
            if (players == null || players.Length == 0)
                return null;
            PlayCard c; //Assistive variable for only using card retrieiving functions (UseTableCard()) once.
            int index;

            if (life < maxLife && players.Length > 2 && hand.Contains(CardType.Beer, out index)) //First heal.
            { return new PlayCard(index, null, hand[index]); }

            c = UseDiscardingCard(players);
            if (c != null)
                return c;

            if (hand.Contains(CardType.Stagecoach, out index)/* && hand.Size <= life + 1*/)
                return new PlayCard(index, null, hand[index]);
            if (hand.Contains(CardType.WellsFargo, out index))
                return new PlayCard(index, null, hand[index]);

            c = UseTableCard(players);
            if (c != null)
                return c;

            if (hand.Contains(CardType.Gatling, out index))
                return new PlayCard(index, null, hand[index]);
            if (hand.Contains(CardType.Indians, out index))
                return new PlayCard(index, null, hand[index]);

            c = UseTargetCard(players);
            if (c != null)
                return c;

            if (hand.Contains(CardType.Saloon, out index) && ShouldUseSaloon(players))
                return new PlayCard(index, null, hand[index]);

            hasPlayedBang = false; //This will be the end of their turn, resets for the next round.
            return null;
        }

        //Determines if it is profitable to use a saloon card.
        private bool ShouldUseSaloon(Player[] players)
        {
            if (life >= maxLife) //If full health, don't use saloon.
                return false;
            bool otherPlayersFullHealth = true;
            if (life < maxLife) //If the only players that benefit are friends, use it.
            {
                for (int i = 0; i < players.Length; i++)
                    if (players[i].Life != maxLife)
                    { otherPlayersFullHealth = false; break; }
                if (otherPlayersFullHealth)
                    return true;
            }
            for (int i = 0; i < players.Length; i++) //Use an attack card before healing in case kiling can take place first.
                if (players[i].Life == 1 && CanReachPlayer(players, i) && CanUseAttackCard())
                    return false;
            if (life == 1)
                return true;
            if (life == 2)
                return MainProgram.Chance(.5);
            return false;
            
        }
        
        //Returns if the hand contains a direct attack card and if they can use it that turn.
        private bool CanUseAttackCard()
        {
            return ((hand.Contains(CardType.Bang) && !hasPlayedBang) || hand.Contains(CardType.Gatling) || hand.Contains(CardType.Indians));
        }

        //Uses equippable cards like guns and mustangs.
        private PlayCard UseTableCard(Player[] players)
        {
            int index;

            if (hand.Contains(CardType.Scope, out index) && !tableCards.Contains(CardType.Scope))
                return new PlayCard(index, null, hand[index]);
            if (hand.Contains(CardType.Mustang, out index) && !tableCards.Contains(CardType.Mustang))
                return new PlayCard(index, null, hand[index]);
            if (hand.Contains(CardType.Barrel, out index) && !tableCards.Contains(CardType.Barrel))
                return new PlayCard(index, null, hand[index]);

            if (hand.Contains(CardType.Winchester, out index))
                if (!tableCards.Contains(CardType.Winchester))
                    return new PlayCard(index, null, hand[index]);
            if (hand.Contains(CardType.RevCarabine, out index))
            {
                if (!tableCards.Contains(CardType.Winchester) && !tableCards.Contains(CardType.RevCarabine))
                    return new PlayCard(index, null, hand[index]);
            }
            if (hand.Contains(CardType.Remington, out index))
            {
                if (!tableCards.Contains(CardType.Winchester) && !tableCards.Contains(CardType.RevCarabine) && !tableCards.Contains(CardType.Remington))
                    return new PlayCard(index, null, hand[index]);
            }
            if (hand.Contains(CardType.Schofield, out index))
            {
                if (!tableCards.Contains(CardType.Schofield) && !tableCards.Contains(CardType.RevCarabine) && !tableCards.Contains(CardType.Remington) && !tableCards.Contains(CardType.Winchester))
                    return new PlayCard(index, null, hand[index]);
            }
            return null;
        }

        //Uses cards requiring a target such as bang or duel.
        private PlayCard UseTargetCard(Player[] players)
        {
            if (!hand.Contains(CardType.Bang))
                return null;
            players = GetReachablePlayers(players);
            if (players == null) //If there are no reachable players, just return null.
                return null;
            int lowest = 4; 
            int[] lowIndexes = new int[players.Length];
            int num = 0;
            for (int i = 0; i < players.Length; i++) //Identifies the reachable players with lowest life.
                if (players[i].Life < lowest)
                    lowest = players[i].Life;
            for (int i = 0; i < players.Length; i++)
                if (players[i].Life == lowest)
                {
                    lowIndexes[num] = i;
                    num++;
                }

            int target;
            if (num > 0) //If there is a lowest life player(s), randomly choose from them.
                target = lowIndexes[MainProgram.random.Next(num)];
            else target = MainProgram.random.Next(players.Length); //Else choose from all players.

            for (int i = 0; i < hand.Size; i++) //Plays the card on the chosen player.
                if (hand[i].CType == CardType.Bang && !hasPlayedBang)
                {
                    hasPlayedBang = true;
                    return new PlayCard(i, players[target], hand[i]);
                }
            return null;
        }

        //Uses panic(1) or cat balou(2), selecting the best card of the best player to discard.
        private PlayCard UseDiscardingCard(Player[] players)
        {
            int index;
            double[] rating = new double[players.Length]; //The rating given to each player based on the cards to discard/steal; highest will be the target.
            if (hand.Contains(CardType.Panic))
                players = GetReachablePlayers(players);
            else if (!hand.Contains(CardType.CatBalou))
                return null;
            if (players == null) //if no reachable players
                return null;
            if (hand.Contains(CardType.Panic, out index) || hand.Contains(CardType.CatBalou, out index))
                for (int i = 0; i < players.Length; i++)
                    if (players[i] != this)
                    {
                        if (players[i].TableCards.Contains(CardType.Mustang))
                            rating[i] += GetRating(CardType.Mustang);
                        if (players[i].TableCards.Contains(CardType.Scope))
                            rating[i] += GetRating(CardType.Scope);
                        if (players[i].TableCards.Contains(CardType.Winchester))
                            rating[i] += GetRating(CardType.Winchester);
                        if (players[i].TableCards.Contains(CardType.RevCarabine))
                            rating[i] += GetRating(CardType.RevCarabine);
                        if (players[i].TableCards.Contains(CardType.Remington))
                            rating[i] += GetRating(CardType.Remington);
                        if (players[i].TableCards.Contains(CardType.Schofield))
                            rating[i] += GetRating(CardType.Schofield);
                        if (players[i].TableCards.Contains(CardType.Barrel))
                            rating[i] += GetRating(CardType.Barrel);
                    }

            int num = 0; //Get highest rating players(s) and randomly pick one.
            double highest = 0;
            int[] highestIndex = new int[players.Length];
            for (int i = 0; i < players.Length; i++)
                if (rating[i] >= highest && players[i] != this) highest = rating[i];
            for (int i = 0; i < players.Length; i++)
                if (rating[i] == highest && players[i] != this)
                {
                    highestIndex[num] = i;
                    num++;
                }
            num = MainProgram.random.Next(num);

            //Pick the best card of the chosen player.
            double bestRating = 0; 
            int helperIndex, bestIndex = -1;
            if (players[highestIndex[num]].TableCards.Contains(CardType.Mustang, out helperIndex))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Mustang); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.Scope, out helperIndex) && bestRating <= GetRating(CardType.Scope))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Scope); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.Winchester, out helperIndex) && bestRating <= GetRating(CardType.Winchester))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Winchester); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.RevCarabine, out helperIndex) && bestRating <= GetRating(CardType.RevCarabine))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.RevCarabine); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.Remington, out helperIndex) && bestRating <= GetRating(CardType.Remington))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Remington); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.Schofield, out helperIndex) && bestRating <= GetRating(CardType.Schofield))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Schofield); }
            if (players[highestIndex[num]].TableCards.Contains(CardType.Barrel, out helperIndex) && bestRating <= GetRating(CardType.Barrel))
            { bestIndex = helperIndex; bestRating = GetRating(CardType.Barrel); }
            if (bestIndex != -1)
                return new PlayCard(index, players[highestIndex[num]], hand[index], players[highestIndex[num]].TableCards[bestIndex], bestIndex);

            //If no players have any table cards, discards a random card from the chosen player's hand.
            int rand = MainProgram.random.Next(players[highestIndex[num]].Hand.Size);
            if (players[highestIndex[num]].Hand.Size - 1 > 0 /* && fifty percent chance */)
                return new PlayCard(index, players[highestIndex[num]], hand[index], players[highestIndex[num]].Hand[rand], rand);
            return null;
        }

        //Gets the rating or value of table cards.
        private double GetRating(CardType type)
        {
            switch (type)
            {
                case CardType.Winchester: return 4;
                case CardType.RevCarabine: return 3;
                case CardType.Remington: return 2;
                case CardType.Schofield: return 1;
                case CardType.Barrel: return 3;
                case CardType.Mustang: return 4;
                case CardType.Scope: return 3.1;
                default: return 0;
            }
        }

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
