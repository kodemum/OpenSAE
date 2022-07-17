using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public enum SymbolArtSoundEffect
    {
        [XmlEnum("0")]
        None = 0,
        [XmlEnum("1")]
        Default = 1,
        [XmlEnum("2")]
        Joy = 2,
        [XmlEnum("3")]
        Anger = 3,
        [XmlEnum("4")]
        Sorrow = 4,
        [XmlEnum("5")]
        Unease = 5,
        [XmlEnum("6")]
        Surprise = 6,
        [XmlEnum("7")]
        Doubt = 7,
        [XmlEnum("8")]
        Help = 8,
        [XmlEnum("9")]
        Whistle = 9,
        [XmlEnum("10")]
        Embarrassed = 10,
        [XmlEnum("11")]
        NailedIt = 11
    }
}
