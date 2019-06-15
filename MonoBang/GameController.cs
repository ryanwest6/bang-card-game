using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;
using System;

//For Bang.
namespace ERS
{
    class GameController : Section
    {

        #region Fields

        const int DEFAULT_SPEED = 45;
        int speed = DEFAULT_SPEED;

        CardStack deck, discardPile;
        //All current players
        Player[] players;
        //Pointer to the player who is the sheriff this round
        Player sheriff;
        //The visual of the cards moving.
        TravelingCard[] visual;
        //How many visual cards there are.
        int visualNum = 0, oldVisualNum;

        ChatBox chat; //The chatbox at the bottom of the screen.

        //If the game has started and if the game is paused.
        bool gameStarted = false, paused = false, gameIsOver = false;
        //Shows debug text, like deck size.
        public static bool debug = false;
        //True if the deck has been reshuffled.
        bool deckShuffled = true, cardDealed = false;
        //Controls whether the players auto-advance, or advance turns by keypress.
        bool autoPlay = false, nextPlayerCanBegin = false;
        //For cases when there is one card left in the deck and 2 need to be drawn.
        int cardsDrawn = 0;

        int thinkingTimer = 0, whosTurn = 0;

        //space between games
        int betweenGamesTimer = 0;

        PlayCard lastPlay; //The last card in play.

        #endregion

        public GameController(Section p)
            : base(p)
        {
            chat = new ChatBox(15, BangGame.SCREEN_HEIGHT - 155);
            visual = new TravelingCard[10000];

            deck = new CardStack(BangGame.SCREEN_WIDTH - 105, 180, true);
            discardPile = new CardStack(BangGame.SCREEN_WIDTH - 105, 280);
            
            players = new Player[] { new CPU("Calamity Janet", false, new Point(200, 40)), new CPU("Willy the Kid", true, new Point(200, 120)), 
                new CPU("Black Jack", true, new Point(200, 200)), new CPU("Lucky Duke", true, new Point(200, 280)), new CPU("Rose Doolan", false, new Point(200, 360)),
                new CPU("Paul Regret", true, new Point(200, 440)) /*new CPU("El Gringo", true, new Point(200, 540))*/ };

            RestartGame();
        }

        //Updates states, both input and players. Also handles button presses and dead player functions.
        private void PreUpdate()
        {
            oldVisualNum = visualNum;
            UpdateStates();

            for (int i = 0; i < players.Length; i++)
                if (!players[i].IsAlive && !players[i].FinishedDeathFunctions && CanProceed())
                {
                    chat.Write(players[i].Name + " has been killed by " + players[whosTurn].Name + "!");
                    for (int j = 0; j < players[i].Hand.Size; )
                        AddVisual(new TravelingCard(players[i].Hand, j, discardPile, speed, CardState.FaceUp, j * 5));
                    for (int j = 0; j < players[i].TableCards.Size; )
                        AddVisual(new TravelingCard(players[i].TableCards, j, discardPile, speed, CardState.FaceUp, j * 5));
                    players[i].FinishedDeathFunctions = true;
                }
            //Where the deck actually gets reshuffled
            if (!deckShuffled && CanProceed())
            {
                deck.Shuffle();
                deckShuffled = true;
            }
            if (thinkingTimer > 0)
                thinkingTimer--;

            for (int i = 0; i < players.Length; i++)
                players[i].Update(mouse, oldMouse);
            deck.Update(mouse, oldMouse);
            discardPile.Update(mouse, oldMouse);
            GetInput();
        }
        
        //Handles keyboard input.
        private void GetInput()
        {
            if (keyboard.IsKeyDown(Keys.Right) || (mouse.X >= 160 && mouse.X < 175 && mouse.Y >= 2 && mouse.Y < 15 &&
                mouse.LeftButton == ButtonState.Pressed))
            {
                if (keyboard.IsKeyDown(Keys.RightShift))
                { Card.cardSizeX++; Card.cardSizeY += 1.4f; }
                else speed++;
            }
            if (keyboard.IsKeyDown(Keys.Left) || (mouse.X >= 140 && mouse.X < 155 && mouse.Y >= 2 && mouse.Y < 15 &&
                mouse.LeftButton == ButtonState.Pressed))
            {
                if (keyboard.IsKeyDown(Keys.RightShift))
                { Card.cardSizeX--; Card.cardSizeY -= 1.4f; }
                else if (speed > 1) speed--;
            }

            //when user presses A, switches playing mode between automatic and controlled by user
            if (keyboard.IsKeyDown(Keys.A) && oldKeyboard.IsKeyUp(Keys.A) || (mouse.X >= 424 && mouse.X < 435 && mouse.Y >= 2 && mouse.Y < 15 &&
                mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released))
                autoPlay = !autoPlay;

            if (!autoPlay && (keyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space)) || (mouse.X >= 409 && mouse.X < 422 && mouse.Y >= 2 &&
                mouse.Y < 15 && mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released))
                nextPlayerCanBegin = true;
            else
                nextPlayerCanBegin = false;
           

            if (keyboard.IsKeyDown(Keys.P) && oldKeyboard.IsKeyUp(Keys.P) || (mouse.X >= 441 && mouse.X < 454 && mouse.Y >= 2 && mouse.Y < 15 &&
                mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released))
                paused = !paused;

            if ((keyboard.IsKeyDown(Keys.F) && oldKeyboard.IsKeyUp(Keys.F)) || (mouse.X >= 200 && mouse.X < 266 && mouse.Y >= 2 && mouse.Y < 14 && 
                mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released))
                debug = !debug;
            if (!keyboard.IsKeyDown(Keys.K))
                discardPile.Layout = Layout.Deck;
            else discardPile.Layout = Layout.Hand;
        }

        #region Gameplay Methods

        public override Section Update()
        {
            PreUpdate();

            if (betweenGamesTimer > 0)
                betweenGamesTimer--;
            //if (betweenGamesTimer == 1)
            //    SendAllCardsToPile();

            if (!paused && betweenGamesTimer <= 0)
            {

                //Handles moving the cards across the screen
                ControlVisualEffects();

                //If not started, deal out cards to each player.
                if (!gameStarted && deck.Size == 52 && CanProceed())
                {
                    int i;
                    for (i = deck.Size - 1; i >= (52 - (players.Length * 4)); i--)
                        AddVisual(new TravelingCard(deck, i, players[i % players.Length].Hand, speed, 
                            debug ? CardState.FaceUp : CardState.FaceDown, (int)((52 - i) * (speed * .2))));

                    //The sheriff gets an additional first card.
                    AddVisual(new TravelingCard(deck, i, sheriff.Hand, speed, 
                        debug ? CardState.FaceUp : CardState.FaceDown, (int)((52 - i) * (speed * .2))));

                    cardDealed = true;
                }
                else if (!gameStarted && cardDealed && CanProceed())
                {
                    gameStarted = true; cardDealed = false; chat.Write("The game has begun!");
                }
                if (!gameIsOver)
                {
                    //Priority is highest to lowest [CanProceed()]
                    if (deck.Size == 0 && CanProceed()) //Moves the discard stack to the pile and shuffles when empty.
                        ReshuffleDeck();
                    if (gameStarted && CanProceed() && (autoPlay || nextPlayerCanBegin)) //Regular gameplay.
                        PlayTurns();
                    if (GetLivingPlayers().Length == 1 && CanProceed())
                    { //Restarts the game if one player wins.
                        chat.Write(GetLivingPlayers()[0].Name + " has won the game!!!");
                        betweenGamesTimer = 240;
                        SendAllCardsToPile();
                    }
                }
                //if gameIsOver, this restarts it
                else if (CanProceed())
                    RestartGame();
            }

            chat.Update(mouse, oldMouse);
            UpdateOldStates();
            return this;
        }

        //Assigns roles to each player, depending on number of players. Max players = 8
        private void AssignRoles()
        {
            //Reset roles
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Role = CharacterRole.None;
            }

            List<CharacterRole> roles = new List<CharacterRole>();

            roles.Add(CharacterRole.Sheriff);
            roles.Add(CharacterRole.Renegade);
            roles.Add(CharacterRole.Outlaw);
            if (players.Length > 3)
            roles.Add(CharacterRole.Outlaw);
            if (players.Length > 4)
                roles.Add(CharacterRole.Vice);
            if (players.Length > 5)
                roles.Add(CharacterRole.Outlaw);
            if (players.Length > 6)
                roles.Add(CharacterRole.Vice);
            if (players.Length > 7)
                roles.Add(CharacterRole.Renegade);

            int playersLeftToAssign = players.Length;

            Random rand = new Random();

            while (roles.Count > 0)
            {
                int nextRandPlayerIndex = rand.Next() % players.Length;

                if (players[nextRandPlayerIndex].Role == CharacterRole.None)
                {
                    players[nextRandPlayerIndex].Role = roles[0];
                    if (roles[0] == CharacterRole.Sheriff)
                    {
                        players[nextRandPlayerIndex].Life++;
                        chat.Write("This town is run by Sheriff " + players[nextRandPlayerIndex].Name + ".");
                        sheriff = players[nextRandPlayerIndex];
                        whosTurn = nextRandPlayerIndex;
                    }
                    roles.RemoveAt(0);
                }
            }
        }

        //Moves discarded cards to the deck and sets the flag to reshuffle it
        private void ReshuffleDeck()
        {
            if (lastPlay == null || lastPlay.Who == null || !lastPlay.Who.HasUsedBarrel) //Solves a glitch when there are no cards in the discard pile to read.
            {
                for (int i = 0; i < discardPile.Size; i++)
                    AddVisual(new TravelingCard(discardPile, i, deck, speed, CardState.FaceUp, i + 1));
                deckShuffled = false;
                chat.Write("Reshuffling deck...");
            }
        }

        //Resets all variables to begin another game
        private void RestartGame()
        {
            gameStarted = false;
            gameIsOver = false;
            chat.Clear();
            deck.Shuffle();
            for (int i = 0; i < players.Length; i++)
                players[i].Reset();
            chat.Write("Welcome to Bang!");
            chat.Write("Dealing cards...");
            AssignRoles();
        }

        //Resets things and moves all cards to the discard pile.
        private void SendAllCardsToPile()
        {
            gameIsOver = true;
            foreach (Player p in players)
            {
                for (int i = 0; i < p.Hand.Size + i; i++)
                    AddVisual(new TravelingCard(p.Hand, deck, speed, CardState.FaceDown));
                for (int i = 0; i < p.TableCards.Size + i; i++)
                    AddVisual(new TravelingCard(p.TableCards, deck, speed, CardState.FaceDown));
            }
            for (int i = 0; i < discardPile.Size + i; i++)
                AddVisual(new TravelingCard(discardPile, deck, speed, CardState.FaceDown));
        }

        //Plays a turn, including drawing cards, playing them, and effects.
        private void PlayTurns()
        {
            if (!players[whosTurn].HasDrawn) //Draw 2 cards.
                DrawCards();
            else if (!players[whosTurn].HasPlayedCard) //Option to play a card.
                PlayCard();
            else if (lastPlay != null) //If the current player did play a card, the card takes its effect (other players may play).
                HandlePlayCardEffects();
            else if (players[whosTurn].HasFinishedTurn) //Discard extra cards at the end of the player's turn.
                FinishAndChangeTurn();
        }

        //Draws cards for a player at the beginning of their turn.
        private void DrawCards()
        {
            if (deck.Size == 1)
            {
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, speed, CardState.FaceDown));
                cardsDrawn = 1;
                chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " first card...");
            }
            else if (deck.Size > 0)
            {
                if (cardsDrawn == 0)
                {
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed), CardState.FaceDown));
                    chat.Write(players[whosTurn].Name + " draws 2 cards to begin " + players[whosTurn].Gender + " turn. ");
                }
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed * 1.25), CardState.FaceDown));
                if (cardsDrawn == 1)
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " second card to begin " + players[whosTurn].Gender + " turn. ");
                players[whosTurn].HasDrawn = true;
                if (cardsDrawn == 0)
                    players[whosTurn].HasThought = false;
                cardsDrawn = 0;
            }
        }

        //Lets the player play a card(s) if they choose.
        private void PlayCard()
        {
                //hasthought = true here or something
                players[whosTurn].HasFinishedTurn = false;
                players[whosTurn].HasPlayedCard = true;
                lastPlay = players[whosTurn].Play(GetLivingPlayers(), PlayType.Normal); //When the player originally plays a card.
                if (lastPlay != null)
                {
                    //If the chosen card to play was for the table, then equip it.
                    if (lastPlay.CType == CardType.Schofield || lastPlay.CType == CardType.Winchester || lastPlay.CType == CardType.Mustang ||
                        lastPlay.CType == CardType.Remington || lastPlay.CType == CardType.RevCarabine || lastPlay.CType == CardType.Scope ||
                        lastPlay.CType == CardType.Barrel)
                        EquipCard();
                    //Otherwise, the card goes to the discards.
                    else
                        AddVisual(new TravelingCard(players[whosTurn].Hand, lastPlay.Index, discardPile, speed, CardState.FaceUp, 0));
                    WriteCardToChat(players[whosTurn], lastPlay.Who, lastPlay.CType);
                }
                else
                    players[whosTurn].HasFinishedTurn = true;
        }

        //Finishes the player's turn by discarding extras, then changes the turn.
        private void FinishAndChangeTurn()
        {
            PlayCard[] extras = players[whosTurn].DiscardExtras(GetLivingPlayers());
                if (extras != null && CanProceed())
                {
                    for (int i = 0; i < extras.Length; i++)
                    {
                        AddVisual(new TravelingCard(players[whosTurn].Hand, extras[i].Card, discardPile, speed, CardState.FaceUp, 0));
                    }
                    if (extras.Length == 1)
                        chat.Write(players[whosTurn].Name + " discards " + extras.Length + " card at the end of " + players[whosTurn].Gender + " turn.");
                    else chat.Write(players[whosTurn].Name + " discards " + extras.Length + " cards at the end of " + players[whosTurn].Gender + " turn.");

                }
                int oldTurn = whosTurn;
                players[whosTurn].HasFinishedTurn = false;
                players[whosTurn].HasPlayedCard = false;
                players[whosTurn].HasDrawn = false;
                do
                {
                    whosTurn++;
                    if (whosTurn >= players.Length)
                        whosTurn = 0;
                    if (oldTurn == whosTurn)
                        break;
                }
                while (!players[whosTurn].IsAlive);
        }

        //Handles effects of cards played by the current player (bang requires missed, etc.)
        private void HandlePlayCardEffects()
        {
            PlayCard[] secondary = new PlayCard[players.Length];
            bool effectFinished = true; //This must be set to false each time to prevent current effect (barrel) from finishing.
            switch (lastPlay.CType)
            {
                case CardType.Bang:
                    if (!UseBarrel(out effectFinished))
                        secondary[0] = lastPlay.Who.Play(null, PlayType.BangResponse);
                    if (secondary[0] != null)
                    {
                        AddVisual(new TravelingCard(lastPlay.Who.Hand, secondary[0].Index, discardPile, speed, CardState.FaceUp, 0));
                        chat.Write(lastPlay.Who.Name + " avoids " + players[whosTurn].Name + "'s bang card with a missed!");
                    }
                    else if (secondary[0] == null && !lastPlay.Who.HasUsedBarrel) WriteLifeLossToChat(lastPlay.Who, players[whosTurn]);
                    break;
                case CardType.Saloon:
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].IsAlive && players[i].Life < players[i].MaxLife)
                            players[i].Life++;
                    }
                    break;
                case CardType.Beer:
                    if (players[whosTurn].Life < players[whosTurn].MaxLife)
                        players[whosTurn].Life++;
                    break;
                case CardType.Indians:
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (i != whosTurn && players[i].IsAlive)
                        {
                            secondary[i] = players[i].Play(null, PlayType.IndiansResponse);
                            if (secondary[i] != null)
                            {
                                AddVisual(new TravelingCard(players[i].Hand, secondary[i].Index, discardPile, speed, CardState.FaceUp, 0));
                                chat.Write(players[i].Name + " avoids the Indians with a bang!");
                            }
                            else WriteLifeLossToChat(players[i], players[whosTurn]);
                        }
                    }
                    break;
                case CardType.Gatling:
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (i != whosTurn && players[i].IsAlive)
                        {
                            secondary[i] = players[i].Play(null, PlayType.BangResponse);
                            if (secondary[i] != null)
                            {
                                AddVisual(new TravelingCard(players[i].Hand, secondary[i].Index, discardPile, speed, CardState.FaceUp, 0));
                                chat.Write(players[i].Name + " avoids the gatling with a missed!");
                            }
                            else WriteLifeLossToChat(players[i], players[whosTurn]);
                        }
                    }
                    break;
                case CardType.Duel:

                    break;
                case CardType.CatBalou:
                    bool discarded = false;
                    for (int i = 0; i < lastPlay.Who.TableCards.Size; i++)
                        if (lastPlay.OtherCard == lastPlay.Who.TableCards[i])
                        {
                            AddVisual(new TravelingCard(lastPlay.Who.TableCards, lastPlay.OtherIndex, discardPile, speed, CardState.FaceUp, 0));
                            discarded = true;
                        }
                    if (!discarded) AddVisual(new TravelingCard(lastPlay.Who.Hand, lastPlay.OtherIndex, discardPile, speed, CardState.FaceUp, 0));
                    chat.Write(players[whosTurn].Name + " discards " + lastPlay.Who.Name + "'s " + lastPlay.OtherCard.GetFullCardTypeString() + 
                        "!");
                    break;
                case CardType.Panic:
                    discarded = false;
                    for (int i = 0; i < lastPlay.Who.TableCards.Size; i++)
                        if (lastPlay.OtherCard == lastPlay.Who.TableCards[i])
                        {
                            AddVisual(new TravelingCard(lastPlay.Who.TableCards, lastPlay.OtherIndex, players[whosTurn].Hand, speed, CardState.FaceUp, 0));
                            discarded = true;
                        }
                    if (!discarded) AddVisual(new TravelingCard(lastPlay.Who.Hand, lastPlay.OtherIndex, players[whosTurn].Hand, speed, CardState.FaceUp, 0));
                    chat.Write(players[whosTurn].Name + " steals " + lastPlay.Who.Name + "'s " + lastPlay.OtherCard.GetFullCardTypeString() + 
                        "!");
                    break;
                case CardType.Stagecoach: effectFinished = HandleStagecoach(); break;
                case CardType.WellsFargo: effectFinished = HandleWellsFargo(); break;
                default: break;
            }
            if (effectFinished) //Completes the operations of the current card.
            {
                players[whosTurn].HasPlayedCard = false;
                if (lastPlay.Who != null)
                    lastPlay.Who.HasUsedBarrel = false;
                lastPlay = null;
            }
        }

        //Handles the wells fargo card operations.
        private bool HandleWellsFargo()
        {
            bool effectFinished = true;
            if (deck.Size < 3)
            {
                if (deck.Size == 2)
                {
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed * .8), CardState.FaceDown));
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " first two cards...");
                    cardsDrawn++;
                }
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, speed, CardState.FaceDown));
                cardsDrawn++;
                if (cardsDrawn == 1)
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " first card...");
                effectFinished = false;
            }
            else if (deck.Size > 0)
            {
                if (cardsDrawn == 0)
                {
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed), CardState.FaceDown));
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed * 1.2), CardState.FaceDown));
                }
                else if (cardsDrawn == 1)
                {
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " last two cards.");
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed), CardState.FaceDown));
                }
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed * 1.4), CardState.FaceDown));
                if (cardsDrawn == 2)
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " third card.");
                players[whosTurn].HasDrawn = true;
                if (cardsDrawn == 0)
                    players[whosTurn].HasThought = false;
                cardsDrawn = 0;
            }
            return effectFinished;
        }

        //Handles the stagecoach card operations.
        private bool HandleStagecoach()
        {
            bool effectFinished = true;
            if (deck.Size == 1)
            {
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, speed, CardState.FaceDown));
                cardsDrawn = 1;
                chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " first card...");
                effectFinished = false;
            }
            else if (deck.Size > 0)
            {
                if (cardsDrawn == 0)
                    AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed), CardState.FaceDown));
                AddVisual(new TravelingCard(deck, players[whosTurn].Hand, (int)(speed * 1.25), CardState.FaceDown));
                if (cardsDrawn == 1)
                    chat.Write(players[whosTurn].Name + " draws " + players[whosTurn].Gender + " second card.");
                players[whosTurn].HasDrawn = true;
                if (cardsDrawn == 0)
                    players[whosTurn].HasThought = false;
                cardsDrawn = 0;
            }
            return effectFinished;
        }

        //Allows the use of a barrel to get a free missed card.
        private bool UseBarrel(out bool effectFinished)
        {
            effectFinished = true;
            if (lastPlay.Who.HasUsedBarrel)
            {
                effectFinished = true;
                if (discardPile[discardPile.Size - 1].Suit == Suit.Hearts)
                {
                    chat.Write("The card suit is hearts; " + lastPlay.Who.Name + " is safe!");
                    return effectFinished;
                }
                else
                    chat.Write("The card suit is not hearts, rendering the barrel ineffective.");
            }
            if (lastPlay.Who.TableCards.Contains(CardType.Barrel) && !lastPlay.Who.HasUsedBarrel)
            {
                AddVisual(new TravelingCard(deck, discardPile, speed, CardState.FaceUp));
                chat.Write(lastPlay.Who.Name + " uses " + lastPlay.Who.Gender + " barrel!");
                lastPlay.Who.HasUsedBarrel = true;
                effectFinished = false;
                return true;
            }
            return false;
        }

        //Controls equipping cards such as guns, barrels, etc.
        private void EquipCard()
        {
            int index = -1;
            switch (lastPlay.CType)
            {
                case CardType.Winchester:
                case CardType.RevCarabine:
                case CardType.Remington:
                case CardType.Schofield:
                    AddVisual(new TravelingCard(players[whosTurn].Hand, lastPlay.Index, players[whosTurn].TableCards, speed, CardState.FaceUp, 0));
                    if (players[whosTurn].TableCards.Contains(CardType.Schofield, out index) || players[whosTurn].TableCards.Contains(CardType.Remington, out index) ||
                        players[whosTurn].TableCards.Contains(CardType.RevCarabine, out index))
                    {
                        chat.Write(players[whosTurn].Name + " has upgraded " + players[whosTurn].Gender + " " + players[whosTurn].TableCards[index].GetFullCardTypeString() + 
                            " with a " + lastPlay.Card.GetFullCardTypeString() + "!");
                        AddVisual(new TravelingCard(players[whosTurn].TableCards, index, discardPile, speed, CardState.FaceUp, 0));
                    } 
                    else chat.Write(players[whosTurn].Name + " has equipped a " + lastPlay.Card.GetFullCardTypeString() + "!");
                    break;
                case CardType.Mustang:
                case CardType.Scope:
                case CardType.Barrel:
                    AddVisual(new TravelingCard(players[whosTurn].Hand, lastPlay.Index, players[whosTurn].TableCards, speed, CardState.FaceUp, 0));
                    break;
            }

             
           // AddVisual(new TravelingCard(players[whosTurn].Hand, lastPlay.Index, discardPile, speed, CardState.FaceUp, 0));
        }

        //Filters the players list to get only the living ones.
        private Player[] GetLivingPlayers()
        {
            Player[] p = new Player[players.Length];

            int pNum = 0;

            for (int i = 0; i < players.Length; i++)
                if (players[i] != null && players[i].IsAlive)
                {
                    p[pNum] = players[i];
                    pNum++;
                }

            Player[] realP = new Player[pNum];
            for (int i = 0; i < realP.Length; i++)
                realP[i] = p[i];

            return realP;
        }

        #endregion

        #region Visual/Timing Methods 

        //Writes if a player loses a life or dies.
        private void WriteLifeLossToChat(Player whoWasHurt, Player whoDunnit)
        {
            if (whoWasHurt.Life > 0) chat.Write(whoWasHurt.Name + " is hit and loses one life!");            
        }

        //Writes the card played and on who.
        private void WriteCardToChat(Player whoDunnit, Player target, CardType cType)
        {
            switch (cType)
            {
                case CardType.Bang:
                    chat.Write(whoDunnit.Name + " plays a bang card on " + target.Name + "!"); break;
                case CardType.Gatling:
                    chat.Write(whoDunnit.Name + " has played gatling!"); break;
                case CardType.Indians:
                    chat.Write(whoDunnit.Name + " has played Indians!"); break;
                case CardType.Beer:
                    chat.Write(whoDunnit.Name + " regains one life point with a beer!"); break;
                case CardType.Saloon:
                    chat.Write(whoDunnit.Name + " has played saloon, healing all players!"); break;
                case CardType.Mustang:
                    chat.Write(whoDunnit.Name + " equips a mustang!"); break;
                case CardType.Scope:
                    chat.Write(whoDunnit.Name + " equips a scope!"); break;
                case CardType.Barrel:
                    chat.Write(whoDunnit.Name + " equips a barrel!"); break;
                case CardType.CatBalou:
                    chat.Write(whoDunnit.Name + " plays a cat balou on " + target.Name + "!"); break;
                case CardType.Panic:
                    chat.Write(whoDunnit.Name + " plays a panic on " + target.Name + "!"); break;
                case CardType.Stagecoach:
                    chat.Write(whoDunnit.Name + " plays a stagecoach and gets 2 cards!"); break;
                case CardType.WellsFargo:
                    chat.Write(whoDunnit.Name + " plays wells fargo and gets 3 cards!"); break;
                default:
                    /*chat.Write("NULL"); */break;
            }
        }

        //Controls visual effects of cards moving and timing.
        private void ControlVisualEffects()
        {
            for (int i = 0; i < visualNum; i++)
            {
                visual[i].Update(speed);
                if (visual[i].Finished) 
                { 
                    visual[i] = null; 
                    visualNum--; 
                }
            }
            MainProgram.MoveNullsToBack(visual, visualNum);
        }

        //Adds a card to the visual cards array if it isn't null.
        private void AddVisual(TravelingCard card)
        {
            if (card != null)
            {
                visual[visualNum] = card;
                visualNum++;
            }
        }

        //If the previous visual card movement is complete and the game can start another.
        private bool CanProceed()
        {
            return (visualNum == 0 && oldVisualNum == 0);
        }

        public override void Draw()
        {
            MainProgram.spriteBatch.Begin();

            //draw table cards area for each player
            for (int i = 0; i < players.Length; i++)
            {
                MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle((int)players[i].TableCards.Location.X - 2, (int)players[i].TableCards.Location.Y - 2,
                    (int)Card.cardSizeX * 4 + 13, (int)Card.cardSizeY + 4), Color.Black);
                MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle((int)players[i].TableCards.Location.X - 1, (int)players[i].TableCards.Location.Y - 1,
                    (int)Card.cardSizeX * 4 + 11, (int)Card.cardSizeY + 2), Color.SandyBrown);
            }

            deck.DrawDeck(CardState.FaceDown, debug);
            discardPile.DrawDeck(CardState.FaceUp, debug);

            for (int i = 0; i < players.Length; i++)
            {
                string playerStr = (sheriff == players[i]) ? "Sheriff " + players[i].Name : players[i].Name;
                players[i].Hand.DrawDeck(debug ? CardState.FaceUp : CardState.FaceDown);
                players[i].TableCards.DrawDeck(CardState.FaceUp);
                MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, playerStr, new Vector2(players[i].Hand.Location.X - 140, players[i].Hand.Location.Y - 4), Color.White);
                int distAway = players[whosTurn].GetDistanceAway(players, players[i]);
                string distAwayStr = (distAway == -1) ? "" : distAway.ToString() + " away";
                if (players[i].IsAlive)
                    MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, distAwayStr, new Vector2(players[i].Hand.Location.X - 90, players[i].Hand.Location.Y + 20), Color.White);
                for (int j = 0; j < players[i].Life; j++)
                    MainProgram.spriteBatch.Draw(MainProgram.game.bullet, new Rectangle((int)(players[i].Hand.Location.X - 140), (int)(players[i].Hand.Location.Y + 12 + (j * 12)), 32, 10), Color.White);
                if (players[i].Life == 0)
                    MainProgram.spriteBatch.Draw(MainProgram.game.bullet, new Rectangle((int)(players[i].Hand.Location.X - 140), (int)(players[i].Hand.Location.Y + 12), 32, 10), Color.Black);
            }

            //who's turn arrow
            MainProgram.spriteBatch.Draw(MainProgram.game.arrow, new Rectangle((int)(players[whosTurn].Hand.Location.X - 185), (int)(players[whosTurn].Hand.Location.Y + 26), 32, 32), Color.Black);

            for (int i = 0; i < visualNum; i++)
                visual[i].Draw();

            chat.Draw();

            //top menu
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(0, 0, BangGame.SCREEN_WIDTH, 17), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(0, 0, BangGame.SCREEN_WIDTH, 16), Color.Brown);

            //who's turn string
            MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, players[whosTurn].Name + "'s turn", new Vector2(BangGame.SCREEN_WIDTH - 110, 2), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(198, 1, 70, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(199, 2, 68, 12), Color.Green);

            MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "DEBUG: " + debug.ToString(), new Vector2(200, 2), debug ? Color.Yellow : Color.White);
            MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "AUTO-PLAY: " + autoPlay.ToString(), new Vector2(290, 2), autoPlay ? Color.Yellow : Color.White);

            //play one action
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(408, 1, 14, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(409, 2, 12, 12), autoPlay ? Color.LightSalmon : Color.LightGreen);
            MainProgram.spriteBatch.Draw(MainProgram.game.arrow, new Rectangle(410, 3, 10, 10), Color.Black);

            //autoplay
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(423+1, 1, 14, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(424+1, 2, 12, 12), autoPlay ? Color.Yellow : Color.LightGreen);
            MainProgram.spriteBatch.Draw(MainProgram.game.arrow, new Rectangle(423+1, 3, 10, 10), Color.Black);
            MainProgram.spriteBatch.Draw(MainProgram.game.arrow, new Rectangle(427+1, 3, 10, 10), Color.Black);

            //pause
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(440, 1, 14, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(441, 2, 12, 12), paused ? Color.Yellow : Color.LightGreen);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(444, 4, 2, 8), Color.Black);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(448, 4, 2, 8), Color.Black);

            //less time 
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(140, 1, 14, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(141, 2, 12, 12), paused ? Color.Yellow : Color.LightGreen);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(143, 7, 8, 2), Color.Black);
            //more time
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(160-4, 1, 14, 14), Color.White);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(161-4, 2, 12, 12), paused ? Color.Yellow : Color.LightGreen);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(163-4, 7, 8, 2), Color.Black);
            MainProgram.spriteBatch.Draw(MainProgram.game.pixel, new Rectangle(166-4, 4, 2, 8), Color.Black);




            if (debug)
            {
                //deck.DisplayCards(610);
                MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "X = " + mouse.X + ", Y = " + mouse.Y, new Vector2(500, 2), Color.White);
                /*MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "VisualNum =   " + visualNum, new Vector2(200, 14), Color.White);
                MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "Total Cards = " + 
                (deck.Size + discardPile.Size + players[0].Hand.Size + players[1].Hand.Size + players[2].Hand.Size + players[3].Hand.Size), new Vector2(200, 26), Color.White);*/
            }
            if (deck.Size + discardPile.Size + players[0].Hand.Size + players[1].Hand.Size + players[2].Hand.Size + players[3].Hand.Size + visualNum < 52)
            { }


            //if (paused)
            //    MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "PAUSED", new Vector2(BangGame.SCREEN_WIDTH - 50, 14), Color.White);

            MainProgram.spriteBatch.DrawString(MainProgram.game.smallFont, "Time between actions: " + speed, new Vector2(2, 2), Color.White);
            MainProgram.spriteBatch.End();
        }

        #endregion
    }
}