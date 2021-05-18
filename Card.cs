using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Card
{
    public Card()
    {
        Name = "Card";
        Transform = new transform();
        colors = new Dictionary<CMCColor, int>();
    }

    public override string ToString()
    {
        return count + " " + Nickname;// + " [" + string.Join(",", colors.Select(p => p.Key)) + "] ";
    }

    public string Name { get; set; }
    public string fixed_name()
    {
        return Nickname.ToLower().Trim();
    }
    public string fixed_split_name()
    {
        return Nickname.ToLower().Trim().Split('/')[0].Trim();
    }
    public string Nickname { get; set; }
    public transform Transform { get; set; }
    [JsonIgnore]
    public int count { get; set; }
    [JsonIgnore]
    public string imgurl { get; set; }
    [JsonIgnore]
    public string lowresimgurl { get; set; }
    [JsonIgnore]
    public Dictionary<CMCColor, int> colors { get; set; }
    [JsonIgnore]
    public int cmc { get; set; }
    public Dictionary<CMCColor,bool > colorIdentity { get; set; }
    public int rank { get; set; }
    public int CardID { get; set; }
    public string rarity { get; set; }
    public decimal price { get; set; }
    public Rarity rarity_enum { get; set; }

    public enum Rarity
    {
        common=1,
        uncommon=2,
        rare=3,
        mythic=4,
    }

    public string type { get; set; }
    public string BackURL { get; set; }
    public enum CMCColor
    {
        C = 0,
        B = 1,
        U = 2,
        R = 3,
        W = 4,
        G = 5,
    }

}

public class transform
{
    public transform()
    {
        posX = 0.0m;
        posY = 0.0m;
        posZ = 0.0m;
        rotX = 0;
        rotY = 180;
        rotZ = 180;
        scaleX = 1;
        scaleY = 1;
        scaleZ = 1;
    }

    public decimal posX { get; set; }
    public decimal posY { get; set; }
    public decimal posZ { get; set; }
    public int rotX { get; set; }
    public int rotY { get; set; }
    public int rotZ { get; set; }
    public int scaleX { get; set; }
    public int scaleY { get; set; }
    public int scaleZ { get; set; }

}

public class CardImage
{
    public CardImage(Card c)
    {
        FaceURL = c.imgurl;
        NumWidth = 1;
        NumHeight = 1;
        BackIsHidden = true;
        BackURL = "https://i.imgur.com/HQP9ISc.png";// "https://i.imgur.com/aN7dr5Z.png";// "https://www.frogtown.me/images/gatherer/CardBack.jpg";
        if (string.IsNullOrEmpty(FaceURL))
            FaceURL = "https://i.imgur.com/lwg3PiO.png?REPLACEME";
        if (!string.IsNullOrEmpty(c.BackURL))
            BackURL = c.BackURL;
    }

    public int NumWidth { get; set; }
    public int NumHeight { get; set; }
    public string FaceURL { get; set; }
    public string BackURL { get; set; }
    public bool BackIsHidden { get; set; }
}



