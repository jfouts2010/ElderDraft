using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ElderDraft
{
    public class LandFixer
    {
        Dictionary<Card.CMCColor, int> totalcosts = new Dictionary<Card.CMCColor, int>();
        Dictionary<Card.CMCColor, int> initiallands = new Dictionary<Card.CMCColor, int>();
        List<Card> deck;
        Random r = new Random();
        int decksize;
        public LandFixer(List<Card> in_deck)
        {
            //get a starting point!
            //Dictionary<Card.CMCColor, int> totalcosts = new Dictionary<Card.CMCColor, int>();
            this.deck = in_deck;
            decksize = deck.Count < 35 ? 40 : (deck.Count < 45 ? 60 : 100);//commanderable
            foreach (Card.CMCColor c in Enum.GetValues(typeof(Card.CMCColor)))
            {
                totalcosts.Add(c, deck.Sum(p => p.colors.ContainsKey(c) ? p.colors[c] : 0));
            }
            int totallandsneeded = decksize - deck.Count;

            foreach (Card.CMCColor c in Enum.GetValues(typeof(Card.CMCColor)))
            {
                if (totalcosts[c] > 0)
                    initiallands.Add(c, totalcosts[c] > 0 ? Math.Max(2, totalcosts[c] * totallandsneeded / totalcosts.Values.Sum()) : 0);
            }
            if (initiallands.Values.Sum() > totallandsneeded)
            {
                initiallands[initiallands.OrderByDescending(p => p.Value).FirstOrDefault().Key] -= (initiallands.Values.Sum() - totallandsneeded);
            }
            if (initiallands.Values.Sum() < totallandsneeded)
            {
                initiallands[initiallands.OrderBy(p => p.Value).FirstOrDefault().Key] += (totallandsneeded - initiallands.Values.Sum());
            }


            //we have a sim Point!

            //Run sim... 
            //fun part

        }

        public List<Card> findBestLands()
        {
            //for testing
            bool edh = deck.Count > 60;
            List<Card.CMCColor> colors = initiallands.Where(p => p.Value > 0).Select(p => p.Key).ToList();
            if (edh)
            {
                List<EDHLand> toaddlands = CommanderLandFixer.GetLandsV1(colors, ref deck);
                List<Card> idkwhythisexists = new List<Card>();
                idkwhythisexists = fillEDHLands(deck, toaddlands);
                
                foreach (Card c in idkwhythisexists)
                {
                    
                }
                double score = ScoreLandsDeck(idkwhythisexists);
               
                return idkwhythisexists;
            }

            int landsneeded = decksize - deck.Sum(p => p.count);
            List<Dictionary<Card.CMCColor, int>> landcombos = GetAllColorCombinations(colors, landsneeded);

            landcombos = landcombos.Where(p => GetLandsDifference(initiallands, p) < .5m * landsneeded).ToList();
            Dictionary<Card.CMCColor, int> bestcombo = new Dictionary<Card.CMCColor, int>();
            double bestscore = double.MinValue;
            foreach (Dictionary<Card.CMCColor, int> landset in landcombos)
            {
                List<Card> testDeck = filldecklands(deck, landset);
                double deckscore = ScoreLandsDeck(testDeck);

                if (deckscore > bestscore)
                {
                    bestcombo = landset;
                    bestscore = deckscore;
                }

            }
            //we want to return cards. 
            List<Card> emptyforlands = new List<Card>();
            emptyforlands = filldecklands(deck, bestcombo);//check if this works for draft
            return emptyforlands;
        }

        public int GetLandsDifference(Dictionary<Card.CMCColor, int> baseline, Dictionary<Card.CMCColor, int> d2)
        {
            if (baseline.Keys.Any(x => !d2.ContainsKey(x)))
                return 999;
            int diff = 0;
            foreach (Card.CMCColor c in baseline.Keys)
            {
                diff += Math.Abs(baseline[c] - d2[c]);
            }
            return diff;
        }

        public List<Card> filldecklands(List<Card> deck, Dictionary<Card.CMCColor, int> lands)
        {
            List<Card> ret = new List<Card>();
            ret.AddRange(deck);
            foreach (KeyValuePair<Card.CMCColor, int> kvp in lands)
            {
                Card landtoadd = new Card()
                {
                    cmc = 0,
                    //is_land = true,
                    Name = "Card",
                    count = kvp.Value,
                };
                switch (kvp.Key)
                {

                    case Card.CMCColor.B:
                        {
                            landtoadd.Nickname = "Swamp";
                            landtoadd.colors.Add(Card.CMCColor.B, 0);
                            landtoadd.count = kvp.Value;
                            break;
                        }
                    case Card.CMCColor.G:
                        {
                            landtoadd.Nickname = "Forest";
                            landtoadd.colors.Add(Card.CMCColor.G, 0);
                            landtoadd.count = kvp.Value;
                            break;
                        }
                    case Card.CMCColor.R:
                        {
                            landtoadd.Nickname = "Mountain";
                            landtoadd.colors.Add(Card.CMCColor.R, 0);
                            landtoadd.count = kvp.Value;
                            break;
                        }
                    case Card.CMCColor.W:
                        {
                            landtoadd.Nickname = "Plains";
                            landtoadd.colors.Add(Card.CMCColor.W, 0);
                            landtoadd.count = kvp.Value;
                            break;
                        }
                    case Card.CMCColor.U:
                        {
                            landtoadd.Nickname = "Island";
                            landtoadd.colors.Add(Card.CMCColor.U, 0);
                            landtoadd.count = kvp.Value;
                            break;
                        }
                }
                ret.Add(landtoadd);
            }
            return ret;
        }

        public List<Card> fillEDHLands(List<Card> deck, List<EDHLand> lands)
        {
            List<string> lookupnames = new List<string>();
            foreach (EDHLand l in lands)
            {
                if (l.priority == -1)//basic
                {
                    Card c = new Card()
                    {
                        Nickname = l.name,
                        count = l.countadded,
                    };
                    foreach (Card.CMCColor color in l.availcolors)
                        c.colors.Add(color, 0);
                    deck.Add(c);
                }
                else
                {
                    Card c = new Card()
                    {
                        Nickname = l.name,
                        count = 1,
                    };
                    foreach (Card.CMCColor color in l.availcolors)
                        c.colors.Add(color, 0);
                    lookupnames.Add(l.name);
                    deck.Add(c);
                }
            }
            List<Dictionary<string, string>> identifiers = new List<Dictionary<string, string>>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (string s in lookupnames)
            {
                Dictionary<string, string> DataBindings = new Dictionary<string, string>();
                DataBindings.Add("name", s);
                if (!identifiers.Any(p => p.ContainsValue(s)))
                    identifiers.Add(DataBindings);
            }
            data.Add("identifiers", identifiers);
            string jsondata = JsonConvert.SerializeObject(data);
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

                                //old token part?
                            }
                        }
                    }
                    else
                        url = card["image_uris"]["large"].ToString();
                    string name = card["name"].ToString();
                    name = name.ToLower().Trim();
                    foreach (Card setcard in deck.Where(p => name.Contains(p.fixed_name()) || name.Contains(p.fixed_split_name())))
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            setcard.imgurl = url;
                        }
                    }
                }
            }
            return deck;
        }

        public List<Dictionary<Card.CMCColor, int>> GetAllColorCombinations(List<Card.CMCColor> colors, int remaininglands)
        {
            if (colors.Count == 0)
                return new List<Dictionary<Card.CMCColor, int>>();
            if (remaininglands == 0)
                return new List<Dictionary<Card.CMCColor, int>>();
            List<Dictionary<Card.CMCColor, int>> ret = new List<Dictionary<Card.CMCColor, int>>();
            Card.CMCColor highestPrio = colors.OrderByDescending(p => totalcosts[p]).FirstOrDefault();
            {
                for (int i = remaininglands; i >= 0; i--)
                {
                    if (i == remaininglands)
                    {
                        Dictionary<Card.CMCColor, int> tdict = new Dictionary<Card.CMCColor, int>();
                        tdict.Add(highestPrio, i);
                        ret.Add(tdict);
                    }
                    else
                    {
                        foreach (Dictionary<Card.CMCColor, int> child in GetAllColorCombinations(colors.Where(p => p != highestPrio).ToList(), remaininglands - i))
                        {
                            child.Add(highestPrio, i);
                            ret.Add(child);
                        }
                    }
                }
            }
            return ret;

        }

        public double ScoreLandsDeck(List<Card> deck)
        {
            //Fill deck with actual 40 cards. 
            List<Card> expandedDeck = ExpandDeck(deck);
            double cumulativescore = 0;
            int cmcmax = deck.Select(p => p.cmc).Max();
            for (int i = 0; i < 1000; i++)
            {
                List<Card> landsAvail = new List<Card>();
                List<Card> currentHand = new List<Card>();
                List<Card> library = randomizeDeck(expandedDeck);
                currentHand.AddRange(library.Take(6));
                library = library.Skip(6).ToList();
                //draw 6 cause always draw first turn for now.

                for (int turn = 1; turn < cmcmax * 1.5 && landsAvail.Count <= cmcmax; turn++)
                {
                    currentHand.Add(library.FirstOrDefault());
                    library = library.Skip(1).ToList();
                    if (currentHand.Any(p => p.cmc == 0))
                    {
                        //pick one... smartly? jessuss
                        //look at total mana costs of hands
                        //look at cards avail at currentmana+1 only!
                        List<Card> playablecards = currentHand.Where(p => p.cmc != 0 && p.cmc <= landsAvail.Count + 1).ToList();
                        Dictionary<Card.CMCColor, int> viablecosts = GetTotalCosts(currentHand.Where(p => p.cmc != 0).ToList());
                        //how many of these can we fix 
                        Dictionary<Card.CMCColor, int> maxManaInColors = GetAvailMana(landsAvail, currentHand.Where(p => p.cmc == 0).ToList());
                        bool unsetcolor = true;
                        Card.CMCColor actuallyplayingcolor = viablecosts.OrderByDescending(p => p.Value).FirstOrDefault().Key;
                        foreach (Card c in playablecards.OrderByDescending(p => p.colors.Sum(x => x.Value)))//.Where(p => p.cmc>0 && p.cmc <= landsAvail.Count))
                        {
                            if (CanPlayThisCard(c, landsAvail, currentHand))
                            {
                                if (unsetcolor)
                                {
                                    actuallyplayingcolor = c.colors.OrderByDescending(p => p.Value).FirstOrDefault().Key;//play that cards heaviest color?
                                    unsetcolor = false;
                                }

                                currentHand.Remove(c);
                            }
                        }


                        //we can play a land now for the sake of not having to +1 it. we already checked colors
                        //try to play viable double lands first. 

                        Card land = currentHand.Where(p => p.cmc == 0).OrderByDescending(p => p.colors.Keys.Contains(actuallyplayingcolor)).OrderByDescending(p => p.colors.Count).FirstOrDefault();
                        currentHand.Remove(land);
                        landsAvail.Add(land);
                        //anything left in viable costs is a color that we have the cmc for that we missed!
                        //still playable cards. 
                        playablecards = currentHand.Where(p => p.cmc != 0 && p.cmc <= landsAvail.Count).ToList();
                        foreach (Card c in playablecards)
                        {
                            //-1 per card that fits, per cmc below current curve it is
                            if (c.Nickname == "Dream Trawler")
                            {

                            }
                            cumulativescore--;// -= landsAvail.Count+1 - c.cmc;
                        }
                    }
                }
            }
            return cumulativescore;
        }

        public List<Card> ExpandDeck(List<Card> library)
        {
            List<Card> ret = new List<Card>();
            foreach (Card c in library)
            {
                for (int i = 0; i < c.count; i++)
                {
                    ret.Add(c);
                }
            }
            return ret;
        }

        public List<Card> randomizeDeck(List<Card> library)
        {
            List<Card> ret = new List<Card>();
            List<Card> librarycopy = library.Where(p => true).ToList();
            while (librarycopy.Count > 0)
            {
                int randomindex = r.Next(0, librarycopy.Count);
                ret.Add(librarycopy[randomindex]);
                librarycopy.RemoveAt(randomindex);
            }
            return ret;
        }

        public Dictionary<Card.CMCColor, int> GetTotalCosts(List<Card> hand)
        {
            Dictionary<Card.CMCColor, int> tempcosts = new Dictionary<Card.CMCColor, int>();
            if (hand.Count == 0)
                return tempcosts;
            foreach (Card.CMCColor c in Enum.GetValues(typeof(Card.CMCColor)))
            {
                tempcosts.Add(c, hand.Max(p => p.colors.ContainsKey(c) ? p.colors[c] : 0));
            }
            return tempcosts;
        }

        public static Dictionary<Card.CMCColor, int> GetTotalPipCount(List<Card> deck)
        {
            Dictionary<Card.CMCColor, int> costs = new Dictionary<Card.CMCColor, int>();
            foreach (Card c in deck.Where(p => p.cmc > 0))
            {
                foreach (Card.CMCColor color in c.colors.Keys)
                {
                    if (!costs.ContainsKey(color))
                        costs.Add(color, 0);
                    costs[color] += c.colors[color];
                }
            }
            return costs;
        }

        public Dictionary<Card.CMCColor, int> GetAvailMana(List<Card> currentLands, List<Card> inhandLands)
        {
            Dictionary<Card.CMCColor, int> availmana = new Dictionary<Card.CMCColor, int>();
            foreach (Card.CMCColor c in Enum.GetValues(typeof(Card.CMCColor)))
            {
                availmana.Add(c, currentLands.Count(p => p.colors.ContainsKey(c)));//works for double lands. still count lands for total avail. 
                if (inhandLands.Any(p => p.colors.ContainsKey(c)))
                    availmana[c]++;
            }
            return availmana;
        }
        //dont ask this for cards above cmc...
        public bool CanPlayThisCard(Card c, List<Card> availlands, List<Card> hand)
        {
            //wtf do we do with hybrid types. 
            Dictionary<Card.CMCColor, int> maxManaInColors = GetAvailMana(availlands, hand.Where(p => p.cmc == 0).ToList());
            foreach (Card.CMCColor color in c.colors.Keys)
            {
                if (maxManaInColors[color] < c.colors[color])
                    return false;
            }
            return true;
        }
    }


    public class CommanderLandFixer
    {


        public static List<EDHLand> GetLandsV1(List<Card.CMCColor> inputcolors, ref List<Card> currentcards)
        {
            List<EDHLand> AllLands = new List<EDHLand>();
            string landfilelocation = "Lands.txt";
            string[] lines = System.IO.File.ReadAllLines(landfilelocation);

            string group = "";
            int max = 1000;
            bool skiptonextendblock = false;
            int colorrequirement = 0;
            foreach (string s in lines)
            {
                if (string.IsNullOrEmpty(s) || (s.StartsWith("{")))
                    continue;
                if (s.StartsWith("}"))
                {
                    skiptonextendblock = false;
                    colorrequirement = 0;
                    group = "";
                    max = 1000;
                    continue;
                }
                if (skiptonextendblock)
                {
                    continue;
                }
                if (s.StartsWith("if"))
                {
                    string number = s.Split(' ')[1];
                    colorrequirement = Convert.ToInt32(number);
                    continue;
                }
                if (s.StartsWith("group"))
                {
                    string groupname = s.Split(' ')[1];
                    group = groupname;
                    continue;
                }
                if (s.StartsWith("max"))
                {
                    string number = s.Split(' ')[1];
                    max = Convert.ToInt32(number);
                    continue;
                }
                if (s.StartsWith("/"))
                    continue;

                //its a land!
                string[] line = s.Split(' ');
                int prio = Convert.ToInt32(line[0]);
                if (prio > 1000)
                    continue;
                string colors = line.Last();
                string name = string.Join(" ", line.Skip(1).Take(line.Count() - 2));
                List<Card.CMCColor> landcolors = new List<Card.CMCColor>();

                EDHLand land = new EDHLand()
                {
                    name = name,
                    priority = prio,
                    requiredColorCount = colorrequirement,
                    identity = new List<Card.CMCColor>(),
                    availcolors = new List<Card.CMCColor>(),
                    max = max,
                    group = group,
                };
                foreach (string color in colors.Split('/', '{'))
                {
                    string fixcolor = color.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", "");
                    if (string.IsNullOrEmpty(fixcolor))
                        continue;
                    if (fixcolor == "A")
                    {
                        land.availcolors.Add(Card.CMCColor.B);
                        land.availcolors.Add(Card.CMCColor.G);
                        land.availcolors.Add(Card.CMCColor.R);
                        land.availcolors.Add(Card.CMCColor.W);
                        land.availcolors.Add(Card.CMCColor.U);
                    }
                    else
                    {
                        Card.CMCColor c = (Card.CMCColor)Enum.Parse(typeof(Card.CMCColor), fixcolor);
                        land.availcolors.Add(c);
                        land.identity.Add(c);
                    }
                }
                AllLands.Add(land);
            }
            //Remove existing lands that overlap?
            foreach (EDHLand land in AllLands)
            {
                if (land.priority < 1000)
                {
                    currentcards = currentcards.Where(p => p.Nickname.ToLower().Trim() != land.name.ToLower().Trim()).ToList();
                }
            }
            currentcards = currentcards.Where(p => p.Nickname != "Forest" && p.Nickname != "Plains" && p.Nickname != "Island" && p.Nickname != "Swamp" && p.Nickname != "Mountain").ToList();
            List<EDHLand> retlands = new List<EDHLand>();
            decimal landsneeded = 99 - currentcards.Sum(p => p.count);
            //Max lands we can add!
            //must have 10 basics. 
            //we like extra forests?
            int eachbasic = 0;
            int basicstotalwewant = inputcolors.Contains(Card.CMCColor.G) ? 15 : 10;
            switch (inputcolors.Count)
            {
                case 5:
                case 4:
                case 3:
                    {
                        eachbasic = (int)((basicstotalwewant) / inputcolors.Count);
                        break;
                    }
                case 2:
                    {
                        eachbasic = 10;
                        break;
                    }
                case 1:
                    {
                        eachbasic = 20;
                        break;
                    }
            }
            foreach (Card.CMCColor c in inputcolors)
            {
                if (c == Card.CMCColor.G)
                {
                    retlands.Add(new EDHLand()
                    {
                        countadded = (int)(eachbasic * 1.2m),
                        name = "Forest",
                        priority = -1,
                        identity = new List<Card.CMCColor>() { c },
                        availcolors = new List<Card.CMCColor>() { c },
                    });

                }
                else
                    retlands.Add(new EDHLand()
                    {
                        name = GetBasicNameByColor(c),
                        countadded = eachbasic,
                        priority = -1,
                        identity = new List<Card.CMCColor>() { c },
                        availcolors = new List<Card.CMCColor>() { c },
                    });
            }
            Dictionary<string, int> groups = new Dictionary<string, int>();
            Dictionary<Card.CMCColor, int> pips = LandFixer.GetTotalPipCount(currentcards);
            List<Card.CMCColor> pipprios = new List<Card.CMCColor>();
            double average = pips.Average(p => p.Value);
            foreach (Card.CMCColor c in pips.Keys)
            {
                if (pips[c] > average * 1.3)
                {
                    pipprios.Add(c);
                }
            }
            if (pipprios.Count > 0)
            {
                foreach (Card.CMCColor c in pipprios)
                {
                    foreach (EDHLand l in AllLands.Where(p => p.availcolors.Contains(c) && p.priority > 0))
                        l.priority -= 10;
                }
            }
            foreach (EDHLand land in AllLands.OrderBy(p => p.priority).ThenByDescending(p => ScoreColorsAgainstPips(pips, p.availcolors)))
            {

                if (land.priority == 0)
                {
                    retlands.Add(land);
                    continue;
                }
                if (land.priority > 100)
                    continue;
                if (inputcolors.Count() < land.requiredColorCount)
                    continue;
                if (!string.IsNullOrEmpty(land.group))
                {
                    if (groups.ContainsKey(land.group))
                        if (groups[land.group] >= land.max)
                            continue;
                }

                if (retlands.Sum(P => P.countadded) >= landsneeded)
                    break;

                //if doesnt match color identity.
                if (land.identity.Any(p => !inputcolors.Contains(p)))
                    continue;

                retlands.Add(land);
                if (!string.IsNullOrEmpty(land.group))
                {
                    if (!groups.ContainsKey(land.group))
                        groups.Add(land.group, 0);
                    groups[land.group]++;
                }
            }
            int sum = retlands.Sum(p => p.countadded);
            if (retlands.Sum(p => p.countadded) < landsneeded)
            {
                //fill?
                int incrementer = 0;
                while (retlands.Sum(p => p.countadded) < landsneeded)
                {
                    Card.CMCColor c = inputcolors[incrementer++ % inputcolors.Count];
                    retlands.FirstOrDefault(p => p.name == GetBasicNameByColor(c)).countadded++;
                }
            }
            return retlands;
        }

        public static int ScoreColorsAgainstPips(Dictionary<Card.CMCColor, int> pips, List<Card.CMCColor> colors)
        {
            int ret = 0;
            foreach (Card.CMCColor c in colors)
            {
                if (pips.ContainsKey(c))
                    ret += pips[c];
            }
            return ret;
        }

        public static string GetBasicNameByColor(Card.CMCColor c)
        {
            switch (c)
            {

                case Card.CMCColor.B:
                    {
                        return "Swamp";

                    }
                case Card.CMCColor.G:
                    {
                        return "Forest";

                    }
                case Card.CMCColor.R:
                    {
                        return "Mountain";

                    }
                case Card.CMCColor.W:
                    {
                        return "Plains";
                    }
                case Card.CMCColor.U:
                    {
                        return "Island";

                    }
            }
            return "URMOM";
        }
    }

    public class EDHLand
    {
        public List<Card.CMCColor> identity { get; set; } = new List<Card.CMCColor>();
        public List<Card.CMCColor> availcolors { get; set; } = new List<Card.CMCColor>();
        public string name { get; set; }
        public int priority { get; set; }
        public int requiredColorCount { get; set; } = 0;
        public int countadded { get; set; } = 1;
        public string group { get; set; }
        public int max { get; set; }
        public override string ToString()
        {
            return name + " - " + string.Join(" ", identity);
        }
    }
}
