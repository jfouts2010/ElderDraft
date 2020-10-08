using ElderDraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class Identifier
{
    public string name { get; set; }
}

public class Call
{
    public List<Identifier> identifiers { get; set; }
}
public class TabletopDeck
{
    
    public static string Create(List<List<Card>> cards, bool fixlands)
    {
        TabletopDeck final = new TabletopDeck(cards, false, fixlands);
        string output = JsonConvert.SerializeObject(final, Formatting.Indented);
        return output;
        /*SaveFileDialog save = new SaveFileDialog();

        save.FileName = (string.IsNullOrEmpty(deckCommander) ? "" : deckCommander + " ") + "Deck-" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".json";

        save.Filter = "Json File | *.json";
        string x = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        save.InitialDirectory = x + "\\My Games\\Tabletop Simulator\\Saves\\Saved Objects";
        if (save.ShowDialog() == DialogResult.OK)
        {
            StreamWriter writer = new StreamWriter(save.OpenFile());
            writer.Write(output);
            writer.Dispose();
            writer.Close();
        }*/
    }
   
    public TabletopDeck(List<List<Card>> cards, bool draft, bool autogen_lands)//, List<Card> Tokens, bool draft, Card commanderCard)
    {
        List<objectstate> tempobjects = new List<objectstate>();
        int sideboardCount = 0;
        int xx = 1;
        int currentCardId = 1;
        Dictionary<string, Card> lands = GeneratePrettyLands();
        if (autogen_lands)
        {
            LandFixer sim = new LandFixer(cards[0]);
            List<Card> additonalLands = sim.findBestLands();
            cards[0] = (additonalLands);
        }
        foreach (List<Card> stack in cards)
        {
            tempobjects.Add(new objectstate());
            if (stack.Count == 1)
            {
                if (stack[0].count == 1)
                    stack[0].count = 2;//fix for single card stacks. 
            }
            foreach (Card c in stack)
            {
                c.CardID = currentCardId++ * 100;
                if (lands.Keys.FirstOrDefault(p => p.ToLower() == c.Nickname.ToLower()) != null)
                {
                    //replace land img with pretty one. 
                    c.imgurl = lands.FirstOrDefault(p => p.Key.ToLower() == c.Nickname.ToLower()).Value.imgurl;
                }
                for (int i = 0; i < c.count; i++)
                {
                    tempobjects[sideboardCount].DeckIDs.Add(c.CardID);
                    tempobjects[sideboardCount].ContainedObjects.Add(c);
                }
                tempobjects[sideboardCount].CustomDeck.Add(xx++.ToString(), new CardImage(c));
            }
            tempobjects[sideboardCount].Transform.posX = sideboardCount * -4;
            sideboardCount++;
        }
        if (draft && !autogen_lands)
        {
            tempobjects.Add(new objectstate());
            foreach (Card c in lands.Values)
            {
                c.CardID = currentCardId++ * 100;
                for (int i = 0; i < c.count; i++)
                {
                    tempobjects[sideboardCount].DeckIDs.Add(c.CardID);
                    tempobjects[sideboardCount].ContainedObjects.Add(c);
                }

                tempobjects[sideboardCount].CustomDeck.Add(xx++.ToString(), new CardImage(c));
            }
            tempobjects[sideboardCount].Transform.posX = sideboardCount * -4;//move off of main stack
            sideboardCount++;
        }
        this.ObjectStates = tempobjects.ToArray();

        //List<CardImage> tempimages = new List<CardImage>();

        //foreach (Card token in Tokens)
        //{
        //    this.ObjectStates[sideboardCount].DeckIDs.Add(token.CardID);
        //    this.ObjectStates[sideboardCount].ContainedObjects.Add(token);
        //    this.ObjectStates[sideboardCount].CustomDeck.Add(xx++.ToString(), new CardImage(token));
        //}
        //if (commanderCard != null)
        //{
        //    commanderCard.CardID = Tokens.Select(p => p.CardID).Max() + bonusCards++ * 100;
        //    this.ObjectStates[sideboardCount].DeckIDs.Add(commanderCard.CardID);
        //    this.ObjectStates[sideboardCount].ContainedObjects.Add(commanderCard);
        //    this.ObjectStates[sideboardCount].CustomDeck.Add(xx++.ToString(), new CardImage(commanderCard));
        //}
        //this.ObjectStates[sideboardCount].Transform.posX = sideboardCount * -4;//move off of main stack
        //sideboardCount++;
        //if (draft)
        //{



        //}


    }

    public objectstate[] ObjectStates { get; set; }

    public Dictionary<string, Card> GeneratePrettyLands()
    {
        Dictionary<string, Card> lands = new Dictionary<string, Card>();
        lands.Add("Forest", new Card()
        {
            Nickname = "Forest",
            //CardID = 100*100+100,
            count = 1,
            imgurl = "https://img.scryfall.com/cards/large/front/3/2/32af9f41-89e2-4e7a-9fec-fffe79cae077.jpg",
        });
        lands.Add("Swamp", new Card()
        {
            Nickname = "Swamp",
            //CardID = 100 * 100 + 200,
            count = 1,
            imgurl = "https://img.scryfall.com/cards/large/front/0/2/02cb5cfd-018e-4c5e-bef1-166262aa5f1d.jpg",
        });
        lands.Add("Mountain", new Card()
        {
            Nickname = "Mountain",
            //CardID = 100 * 100 + 300,
            count = 1,
            imgurl = "https://img.scryfall.com/cards/large/front/5/3/53fb7b99-9e47-46a6-9c8a-88e28b5197f1.jpg",
        });
        lands.Add("Plains", new Card()
        {
            Nickname = "Plains",
            //CardID = 100 * 100 + 400,
            count = 1,
            imgurl = "https://img.scryfall.com/cards/large/front/a/9/a9891b7b-fc52-470c-9f74-292ae665f378.jpg",
        });
        lands.Add("Island", new Card()
        {
            Nickname = "Island",
            //CardID = 100 * 100 + 500,
            count = 1,
            imgurl = "https://img.scryfall.com/cards/large/front/a/c/acf7b664-3e75-4018-81f6-2a14ab59f258.jpg",
        });
        return lands;
    }
}

public class objectstate
{

    public objectstate()
    {
        Transform = new transform()
        {
            posX = -0.0m,
            posY = 1.0m,
            posZ = -0.0m,

        };
        ContainedObjects = new List<Card>();
        DeckIDs = new List<int>();
        CustomDeck = new Dictionary<string, CardImage>();
        Name = "DeckCustom";
    }

    public transform Transform { get; set; }
    public string Name { get; set; }
    public List<Card> ContainedObjects { get; set; }
    public List<int> DeckIDs { get; set; }
    public Dictionary<string, CardImage> CustomDeck { get; set; }

}
