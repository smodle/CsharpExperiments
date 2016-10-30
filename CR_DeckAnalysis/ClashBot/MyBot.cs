﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR_DeckAnalysis;
using Discord;
using Discord.Commands;
using System.IO;
using System.Text.RegularExpressions;
using ClashResources;

namespace ClashBot
{
    class MyBot
    {
        DiscordClient discord;
        CommandService commands;
        public List<SeasonSummary> DeckSummaries = new List<SeasonSummary>();
        public List<Deck> RDecks = DeckList.getRecommendedDecks();
        List<Tuple<string, string, string>> CardLinks = ClashResources.CardLinks.getCardLinks();


        public List<string> CardNames = new List<string>();//(new string[] { "mirror", "ice spirit", "skeletons", "fire spirits", "goblins", "zap", "spear goblins", "minions", "arrows", "bomber", "cannon", "archers", "knight", "tesla", "mortar", "minion horde", "barbarians", "royal giant", "tombstone", "mini p.e.k.k.a", "valkyrie", "musketeer", "fireball", "furnace", "hog rider", "wizard", "giant", "bomb tower", "inferno tower", "goblin hut", "elixir collector", "rocket", "barbarian hut", "three musketeers", "rage", "goblin barrel", "guards", "dark prince", "poison", "baby dragon", "skeleton army", "freeze", "prince", "witch", "balloon", "lightning", "bowler", "giant skeleton", "x-bow", "p.e.k.k.a", "golem", "log", "miner", "princess", "ice wizard", "lumberjack", "sparky", "lava hound", "mega minion", "inferno dragon" });

        public MyBot()
        {
            initializeClashData();
            initializeImageData();

            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });


            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            commands = discord.GetService<CommandService>();

            /*commands.CreateCommand("hello")
                .Do(async(e) =>
                {
                    await e.Channel.SendMessage("good afternoon, oden11.");
                }          
                );*/

            RegisterCardImgCommand();
            RegisterSeasonSummaryCommand();
            RegisterCardTrendCommand();
            RegisterDecksFromCardCommand();
            registerAllDeckCommands();



            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("MjM3NjQ1OTM4OTM4NzQwNzM3.Cua1IA.8Q5pvt9TmCy-IqaX5d2V5np0VU4", TokenType.Bot);
            }
                );
        }

        private void RegisterGreetCommand()
        {

            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("greet") //create command greet
            .Alias(new string[] { "gr", "hi" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Greets a person.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });
        }


        private void RegisterDecksFromCardCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("top100") //create command greet
            .Alias(new string[] { "top10", "top" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Decks in the top 100 players using given card.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {

                string cardName = e.GetArg("GreetedPerson");
                await e.Channel.SendFile("img/" + cardName + ".png");
                cardName = cardName.Replace('-', ' ');

                List<int> targetDecks = new List<int>();

                for (int i = 0; i < DeckSummaries[DeckSummaries.Count - 1].Top100Decks.Count; i++)
                {
                    if (DeckSummaries[DeckSummaries.Count - 1].Top100Decks[i].doesContainCard(cardName))
                    {
                        targetDecks.Add(i);
                    }
                }

                if (targetDecks.Count > 0)
                {
                    string deckString = "";
                    for (int i = 0; i < targetDecks.Count; i++)
                    {
                        deckString = DeckSummaries[DeckSummaries.Count - 1].Top100Decks[targetDecks[i]].toString_Cards();
                        await e.Channel.SendMessage("Deck Rank #" + targetDecks[i] + " (" + (i + 1) + " of " + targetDecks.Count + "): " + deckString);
                    }
                }

            });
        }





        private void RegisterCardImgCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("card") //create command greet
            .Alias(new string[] { "crd", "cards" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Information about a given Clash Royale Card.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                string rawInput = e.GetArg("GreetedPerson");
                string cardName = rawInput.Replace('-', ' ');

                if (CardNames.Contains(cardName))
                {
                    await e.Channel.SendFile("img/" + rawInput + ".png");

                    for (int i = 0; i < DeckSummaries[DeckSummaries.Count - 1].CardData.Count; i++)
                    {
                        if (DeckSummaries[DeckSummaries.Count - 1].CardData[i].Name == cardName)
                        {
                            double currentUsage = DeckSummaries[DeckSummaries.Count - 1].CardData[i].NumberPresent;
                            double previousUsage = DeckSummaries[DeckSummaries.Count - 2].CardData[i].NumberPresent;
                            double usageDelta = currentUsage - previousUsage;

                            if (currentUsage > 0)
                            {
                                //await e.Channel.SendMessage("Cost: " + DeckSummaries[DeckSummaries.Count - 1].CardData[i].Cost + ". Last season " + cardName + " was in " + currentUsage + "% of the top 100 decks, a change of " + usageDelta + "% from the previous season. Try '!top100 " + cardName + "' to see which top 100 decks used " + cardName + ".");
                                await e.Channel.SendMessage("Cost: " + DeckSummaries[DeckSummaries.Count - 1].CardData[i].Cost + ". Last season " + cardName + " was in " + currentUsage + "% of the top 100 decks, a change of " + usageDelta + "% from the previous season.");

                                for (int ix = 0; ix < DeckSummaries[DeckSummaries.Count - 1].Top100Decks.Count; ix++)
                                {
                                    if (DeckSummaries[DeckSummaries.Count - 1].Top100Decks[ix].doesContainCard(cardName))
                                    {
                                        string deckString = DeckSummaries[DeckSummaries.Count - 1].Top100Decks[ix].toString_Cards();
                                        await e.Channel.SendMessage("Top Ranked Deck Using " + cardName + " #" + ix + ": " + deckString);

                                        break;
                                    }
                                }
                                await e.Channel.SendMessage(" Try '!top100 " + cardName + "' to see the rest of the top 100 decks using " + cardName + ".");

                            }
                            else
                            {
                                await e.Channel.SendMessage("Cost: " + DeckSummaries[DeckSummaries.Count - 1].CardData[i].Cost + ". Last season " + cardName + " wasn't used in the top 100 decks!  Change of " + usageDelta + "% from the previous season.");
                            }
                            break;
                        }
                    }



                    //now lets get the links!!!
                    List<Tuple<string, string, string>> l = new List<Tuple<string, string, string>>();
                    for (int i = 0; i < CardLinks.Count; i++)
                    {
                        if (CardLinks[i].Item1 == cardName)
                        {
                            l.Add(CardLinks[i]);
                        }

                    }
                    if (l.Count > 0)
                    {
                        for (int i = 0; i < l.Count; i++)
                        {
                            await e.Channel.SendMessage(l[i].Item3 + " - " + l[i].Item2);

                        }
                    }

                }
                else
                {
                    await e.Channel.SendMessage("Sorry, " + e.User.Name + " , I'm unable to identify " + rawInput + ". oden11 is lazy so I can't handle spaces. Make sure all spaces are replaced with the '-' character. For example, 'hog rider' should be 'hog-rider'.");
                }
                // 
                //sends a message to channel with the given text
            });


            /*  commands.CreateCommand("archers")
                  .Do(async (e) =>
                  {
                      await e.Channel.SendFile("img/archers.png");
                  });*/
        }

        public static string ReplaceAllSpaces(string str)
        {
            string str1 = str.Replace(".", "");
            return Regex.Replace(str1, @"\s+", "-");
        }


        #region Decks

        private void registerAllDeckCommands()
        {
          //  RegisterDecksCommand();
         //   RegisterArenasCommand();
            RegisterIndividualDeckCommands();

        }

        private void RegisterIndividualDeckCommands()
        {


            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("decks") //create command greet
            .Alias(new string[] { "deck", "deckz" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Clash Royale deck information.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                int inputNum;
                bool oops = Int32.TryParse(e.GetArg("GreetedPerson"), out inputNum);
                int seasonNum = DeckSummaries.Count - 1;
                int indexNum = seasonNum - inputNum;

                if (!oops || indexNum < 0 || indexNum > 9)
                {
                    string s = e.GetArg("GreetedPerson");

                    int x = 0;
                    for (int j = 0; j < RDecks.Count; j++)
                    {
                        if (RDecks[j].Nickname.ToLower() == s.ToLower())
                            x = j;
                    }
                    await e.Channel.SendMessage("**Avg. Cost**:  " + RDecks[x].AvgCost() + ", **Card List**: " + RDecks[x].toString_Cards());//% Legendary: " + DeckSummaries[indexNum].Pct_Legendary + ", % Epic: " + DeckSummaries[indexNum].Pct_Epic + ", % Rare: " + DeckSummaries[indexNum].Pct_Rare + ", % Common: " + DeckSummaries[indexNum].Pct_Common);
                    await e.Channel.SendFile("C:/Users/odle.so.1/Downloads/cards/img/decks/" + s + ".png");
                }
                else
                {




                    await e.Channel.SendMessage("Avg. Deck Cost:  " + DeckSummaries[indexNum].AvgCost + ", % Legendary: " + DeckSummaries[indexNum].Pct_Legendary + ", % Epic: " + DeckSummaries[indexNum].Pct_Epic + ", % Rare: " + DeckSummaries[indexNum].Pct_Rare + ", % Common: " + DeckSummaries[indexNum].Pct_Common);
                }

                // await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });








        }
        private void RegisterDecksCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("decks") //create command greet
            .Alias(new string[] { "deck", "deckz" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Clash Royale deck information.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                int inputNum;
                bool oops = Int32.TryParse(e.GetArg("GreetedPerson"), out inputNum);
                int seasonNum = DeckSummaries.Count - 1;
                int indexNum = seasonNum - inputNum;

                if (!oops || indexNum < 0 || indexNum > 9)
                {
                    await e.Channel.SendMessage("Hi " + e.User.Name + ", to find decks from a certain arena type '!decks X' (replace X with the arena number.)");
                }
                else
                {




                    await e.Channel.SendMessage("Avg. Deck Cost:  " + DeckSummaries[indexNum].AvgCost + ", % Legendary: " + DeckSummaries[indexNum].Pct_Legendary + ", % Epic: " + DeckSummaries[indexNum].Pct_Epic + ", % Rare: " + DeckSummaries[indexNum].Pct_Rare + ", % Common: " + DeckSummaries[indexNum].Pct_Common);
                }

                // await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });


        }

        private void RegisterArenasCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("decks") //create command greet
            .Alias(new string[] { "deck", "deckz" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Clash Royale deck information.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                int inputNum;
                bool oops = Int32.TryParse(e.GetArg("GreetedPerson"), out inputNum);
                int seasonNum = DeckSummaries.Count - 1;
                int indexNum = seasonNum - inputNum;

                if (!oops || indexNum < 0)
                {
                    await e.Channel.SendMessage(e.User.Name + ", looks like your input was invalid. Enter '0' for most recent season, '1' for the one before that, etc.");
                }
                else
                {
                    await e.Channel.SendMessage("Avg. Deck Cost:  " + DeckSummaries[indexNum].AvgCost + ", % Legendary: " + DeckSummaries[indexNum].Pct_Legendary + ", % Epic: " + DeckSummaries[indexNum].Pct_Epic + ", % Rare: " + DeckSummaries[indexNum].Pct_Rare + ", % Common: " + DeckSummaries[indexNum].Pct_Common);
                }

                // await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });


        }

        #endregion



        private void RegisterSeasonSummaryCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("seasonsummary") //create command greet
            .Alias(new string[] { "season", "ss" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Information about a given Clash Royale season.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {
                int inputNum;
                bool oops = Int32.TryParse(e.GetArg("GreetedPerson"), out inputNum);
                int seasonNum = DeckSummaries.Count - 1;
                int indexNum = seasonNum - inputNum;

                if (!oops || indexNum < 0)
                {
                    await e.Channel.SendMessage("Sorry, " + e.User.Name + ", looks like your input was invalid. Enter '0' for most recent season, '1' for the one before that, etc.");
                }
                else
                {
                    await e.Channel.SendMessage("Avg. Deck Cost:  " + DeckSummaries[indexNum].AvgCost + ", % Legendary: " + DeckSummaries[indexNum].Pct_Legendary + ", % Epic: " + DeckSummaries[indexNum].Pct_Epic + ", % Rare: " + DeckSummaries[indexNum].Pct_Rare + ", % Common: " + DeckSummaries[indexNum].Pct_Common);
                }

                // await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });


            /*  commands.CreateCommand("archers")
                  .Do(async (e) =>
                  {
                      await e.Channel.SendFile("img/archers.png");
                  });*/
        }


        private void RegisterCardTrendCommand()
        {
            //Since we have setup our CommandChar to be '~', we will run this command by typing ~greet
            commands.CreateCommand("cardtrend") //create command greet
            .Alias(new string[] { "trend", "cardusage" }) //add 2 aliases, so it can be run with ~gr and ~hi
            .Description("Trend for a certain type of card in Clash Royale.") //add description, it will be shown when ~help is used
            .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
            .Do(async e =>
            {

                string userInput = e.GetArg("GreetedPerson");

                if (userInput == "rare")
                {
                    await e.Channel.SendMessage("Seasonal " + userInput + " Card Usage (%): " + getRarityTrendString(CardRarity.Rare));
                }
                else if (userInput == "common" || userInput == "com")
                {
                    await e.Channel.SendMessage("Seasonal " + userInput + " Card Usage (%): " + getRarityTrendString(CardRarity.Common));
                }
                else if (userInput == "epic")
                {
                    await e.Channel.SendMessage("Seasonal " + userInput + " Card Usage (%): " + getRarityTrendString(CardRarity.Epic));
                }
                else if (userInput == "legendary" || userInput == "leg")
                {
                    await e.Channel.SendMessage("Seasonal " + userInput + " Card Usage (%): " + getRarityTrendString(CardRarity.Legendary));
                }
                else
                {
                    await e.Channel.SendMessage("Sorry, @" + e.User.Name + ", I didn't understand that. Try entering a card classification such as 'rare' or 'epic'.");
                }

                // await e.Channel.SendMessage(e.User.Name + " greets " + e.GetArg("GreetedPerson"));
                //sends a message to channel with the given text
            });




            /*  commands.CreateCommand("archers")
                  .Do(async (e) =>
                  {
                      await e.Channel.SendFile("img/archers.png");
                  });*/
        }
        private string getRarityTrendString(CardRarity r)
        {
            string s = "";
            for (int i = 0; i < DeckSummaries.Count - 1; i++)
            {
                s += DeckSummaries[i].getRarity(r) + ", ";
            }
            s += DeckSummaries[DeckSummaries.Count - 1].getRarity(r);

            return s;
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }



        private void initializeImageData()
        {

            for (int i = 0; i < RDecks.Count; i++)
            {
                var fileList = new string[8];
                for (int j = 0; j < RDecks[i].Cards.Count; j++)
                {
                    string c = RDecks[i].Cards[j].Name.Replace(' ', '-');
                    c = c.Replace(".", "");
                    fileList[j] = "C:/Users/odle.so.1/Downloads/cards/img/" + c + ".png";

                    //   System.Drawing.Image image =  System.Drawing.Image.FromFile("C:/Users/odle.so.1/Downloads/cards/img/" + c + ".png");
                    //   System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("img/" + c + ".png");
                }

                var im = ImageManip.CombineBitmap(fileList);
                im.Save("C:/Users/odle.so.1/Downloads/cards/img/decks/" + RDecks[i].Nickname + ".png");
            }
        }
        private void initializeClashData()
        {
            List<Deck> importList1 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\1.txt"));
            List<Deck> importList3 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\3.txt"));
            List<Deck> importList4 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\4.txt"));
            List<Deck> importList6 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\6.txt"));
            List<Deck> importList7 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\7.txt"));
            List<Deck> importList8 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\8.txt"));
            List<Deck> importList9 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\9.txt"));
            List<Deck> importList10 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\10.txt"));
            List<Deck> importList11 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\11.txt"));
            List<Deck> importList12 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\12.txt"));
            List<Deck> importList13 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\13.txt"));
            List<Deck> importList14 = IO.DeckList_Import(Path.Combine(Environment.CurrentDirectory, @"RawData\14.txt"));

            DeckSummaries.Add(new SeasonSummary(importList1, 1));
            DeckSummaries.Add(new SeasonSummary(importList3, 3));
            DeckSummaries.Add(new SeasonSummary(importList4, 4));
            DeckSummaries.Add(new SeasonSummary(importList6, 6));
            DeckSummaries.Add(new SeasonSummary(importList7, 7));
            DeckSummaries.Add(new SeasonSummary(importList8, 8));
            DeckSummaries.Add(new SeasonSummary(importList9, 9));
            DeckSummaries.Add(new SeasonSummary(importList10, 10));
            DeckSummaries.Add(new SeasonSummary(importList11, 11));
            DeckSummaries.Add(new SeasonSummary(importList12, 12));
            DeckSummaries.Add(new SeasonSummary(importList13, 13));
            DeckSummaries.Add(new SeasonSummary(importList14, 14));

            CardNames = DeckSummaries[0].CardNames;

            for (int i = 0; i < DeckSummaries.Count; i++)
            {
                //  DeckSummaries_Visible.Add(DeckSummaries[i]);
            }

            var tmpList = DeckSummaries[0].getEmptyComprehensiveCardReport();
            for (int i = 0; i < tmpList.Count; i++)
            {
                // SelectedDecks_Visible_T2.Add(tmpList[i]);
            }


            //   populateCardComboData();

        }




    }
}