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
        TabletopDeck final = new TabletopDeck(cards, fixlands);
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

    public TabletopDeck(List<List<Card>> cards, bool autogen_lands)//, List<Card> Tokens, bool draft, Card commanderCard)
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

    public static TabletopDeck FullCreateDeck(List<List<Card>> cardsin, bool fixlands)
    {
        Card CommanderCard = null;
        List<string> deck = new List<string>();
        foreach (List<Card> list in cardsin)
        {
            foreach (Card c in list)
                deck.Add(c.Nickname);
            deck.Add("");
        }
        List<Dictionary<string, string>> identifiers = new List<Dictionary<string, string>>();
        Dictionary<string, object> data = new Dictionary<string, object>();
        List<Dictionary<string, object>> calls = new List<Dictionary<string, object>>();
        List<List<Card>> cards = new List<List<Card>>();
        List<Card> currentList = new List<Card>();

        //List<Card> tokens = new List<Card>();
        int cardI = 1;
        foreach (string cardLine in deck)
        {
            if (string.IsNullOrEmpty(cardLine))
            {
                if (currentList.Count > 0)
                    cards.Add(currentList);
                currentList = new List<Card>();
                continue;
            }
            try
            {
                int count = 1;
                string name = cardLine.Trim();
                if (cardLine.Contains(" ") && int.TryParse(cardLine.Split(' ')[0], out int derp))
                {
                    count = Convert.ToInt32(cardLine.Substring(0, cardLine.IndexOf(' ')));
                    name = cardLine.Substring(cardLine.IndexOf(' ') + 1);
                }
                //lands
                /* if (badnames.Contains(name.Trim().ToLower()))
                     continue;*/
                Dictionary<string, string> DataBindings = new Dictionary<string, string>();
                DataBindings.Add("name", name);
                if (!identifiers.Any(p => p.ContainsValue(name)))
                    identifiers.Add(DataBindings);
                /* if (!string.IsNullOrEmpty(deckCommander))
                     if (name == deckCommander)
                         continue;*/
                Card c = new Card()
                {

                    //CardID = cardI++ * 100,
                    count = count,
                    Nickname = name,
                };


                //c.CardID = cardI++ * 100;
                currentList.Add(c);

                if (identifiers.Count > 70)
                {
                    data.Add("identifiers", identifiers);
                    calls.Add(data);
                    data = new Dictionary<string, object>();
                    identifiers = new List<Dictionary<string, string>>();
                }
            }
            catch
            {
                if (currentList.Count > 0)
                    cards.Add(currentList);
                currentList = new List<Card>();
            }
        }
        if (currentList.Count > 0)
            cards.Add(currentList);
        if (identifiers.Count > 0)
        {
            data.Add("identifiers", identifiers);
            calls.Add(data);
        }
        List<Card> tokens = new List<Card>();//tokens!
                                             //tokens.Add(new Card()
                                             //{
                                             //    Nickname = "Human Soldier",
                                             //    imgurl = "https://img.scryfall.com/cards/large/front/1/e/1e76f0e3-9411-401d-ab38-9c3c64769483.jpg?1578936729",
                                             //    CardID = cardI++ * 100,
                                             //    count=1,
                                             //});
        List<string> tokenSearchIds = new List<string>();

        foreach (var d in calls)
        {
            string jsondata = JsonConvert.SerializeObject(d);
            var cli = new HttpClient();
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsondata);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var asdf = cli.PostAsync("https://api.scryfall.com/cards/collection", byteContent);
            string content = asdf.Result.Content.ToString();
            //HttpWebRequest request = System.Net.WebRequest.Create("https://api.scryfall.com/cards/collection") as HttpWebRequest;
            //request.Method = "POST";
            //request.ContentType = "application/json";
            //ASCIIEncoding encoding = new ASCIIEncoding();
            //byte[] byte1 = encoding.GetBytes(jsondata);
            //request.ContentLength = byte1.Length;
            //Stream newStream = request.GetRequestStream();
            //newStream.Write(byte1, 0, byte1.Length);
            List<string> notfound = new List<string>();
            //Dictionary<string, string> CardAndURL = new Dictionary<string, string>();
            //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            //{
            //    Stream receiveStream = response.GetResponseStream();
            //    StreamReader readStream = null;
            //    if (response.CharacterSet == null || response.CharacterSet == "")
            //        readStream = new StreamReader(receiveStream);
            //    else
            //        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            JObject jsonResponse = JObject.Parse(content);
            foreach (var selection in jsonResponse["not_found"])
                foreach (var card in selection)
                    notfound.Add(card.First.ToString());
            foreach (var card in jsonResponse["data"])
            {
                string url = "";
                if (card["card_faces"] != null && card["image_uris"] == null)
                {
                    for (int i = 0; i < card["card_faces"].Count(); i++)
                    {
                        var cardInfo = card["card_faces"][i];
                        if (i == 0)
                            url = cardInfo["image_uris"]["large"].ToString();
                        else
                        {
                            //needs to check if it has image uris

                            Card c = new Card()
                            {
                                Nickname = cardInfo["name"].ToString(),
                                count = 1,
                                imgurl = cardInfo["image_uris"]["large"].ToString()
                            };
                            tokens.Add(c);
                        }
                    }
                }
                else
                    url = card["image_uris"]["large"].ToString();
                string name = card["name"].ToString();

                string manacost = "0";

                int cmc = 0;
                JArray colorid = new JArray();

                try
                {
                    manacost = card["mana_cost"].ToString();
                    cmc = Convert.ToInt32(card["cmc"].ToString());
                    colorid = (JArray)card["color_identity"];
                }
                catch { }
                if (name.StartsWith("Kala"))
                {

                }
                name = name.ToLower().Trim();
                foreach (List<Card> stack in cards)
                {
                    foreach (Card setcard in stack.Where(p => name.Contains(p.fixed_name()) || name.Contains(p.fixed_split_name())))
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            setcard.imgurl = url;
                            setcard.cmc = cmc;
                            foreach (Card.CMCColor c in Enum.GetValues(typeof(Card.CMCColor)))
                            {
                                if (setcard.colors.ContainsKey(c))
                                    continue;
                                if (cmc > 0)
                                {
                                    //not a land
                                    int count = manacost.Count(p => p == c.ToString()[0]);
                                    if (count > 0)
                                        setcard.colors.Add(c, count);
                                }
                                else
                                {
                                    //is a land. get its colors!
                                    if (colorid.Any(p => p.ToString() == c.ToString()))
                                    {
                                        setcard.colors.Add(c, 0);
                                    }
                                }

                            }
                        }
                    }
                }
                //check for tokens
                if (card["all_parts"] != null)
                    foreach (var part in card["all_parts"])
                        if (part["component"].ToString() == "token")
                            tokenSearchIds.Add(part["id"].ToString());
            }
        }

        if (CommanderCard != null)
            tokens.Add(CommanderCard);
        //token search
        if (tokenSearchIds.Count > 1)
        {
            System.Threading.Thread.Sleep(100);
            List<Dictionary<string, string>> tokenidentifiers = new List<Dictionary<string, string>>();
            foreach (string t in tokenSearchIds)
            {
                Dictionary<string, string> tok = new Dictionary<string, string>();
                tok.Add("id", t);
                tokenidentifiers.Add(tok);
            }
            Dictionary<string, object> tokenD = new Dictionary<string, object>();
            tokenD.Add("identifiers", tokenidentifiers);
            string jsondata = JsonConvert.SerializeObject(tokenD);
            HttpWebRequest request = System.Net.WebRequest.Create("https://api.scryfall.com/cards/collection") as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(jsondata);
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            List<string> notfound = new List<string>();
            Dictionary<string, string> CardAndURL = new Dictionary<string, string>();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                if (response.CharacterSet == null || response.CharacterSet == "")
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                string content = readStream.ReadToEnd();
                JObject jsonResponse = JObject.Parse(content);
                foreach (var card in jsonResponse["data"])
                {
                    string url = card["image_uris"]["large"].ToString();
                    string name = card["name"].ToString();
                    if (!tokens.Any(p => p.Nickname == name))
                        tokens.Add(new Card()
                        {
                            imgurl = url,
                            Nickname = name,
                            //CardID = cardI++ * 100,
                            count = 1,

                        });

                }
            }

        }
        if (tokens.Count > 0)
            cards.Add(tokens);
        if (CommanderCard != null)
        {
            //CommanderCard.CardID = cardI++ * 100;
            List<Card> commander = new List<Card>();
            commander.Add(CommanderCard);
            cards.Add(commander);
        }


        TabletopDeck final = new TabletopDeck(cards, fixlands);
        return final;
        string output = JsonConvert.SerializeObject(final, Formatting.Indented);
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


