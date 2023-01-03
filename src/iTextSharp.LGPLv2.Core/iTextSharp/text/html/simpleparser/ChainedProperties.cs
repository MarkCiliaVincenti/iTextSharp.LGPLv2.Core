using System.util;

namespace iTextSharp.text.html.simpleparser;

public class ChainedProperties
{
    public static int[] FontSizes = { 8, 10, 12, 14, 18, 24, 36 };
    public IList<TagAttributes> Chain = new List<TagAttributes>();

    public string this[string key]
    {
        get
        {
            for (var k = Chain.Count - 1; k >= 0; --k)
            {
                var p = Chain[k];
                var attrs = p.Attrs;
                if (attrs.ContainsKey(key))
                {
                    return attrs[key];
                }
            }

            return null;
        }
    }

    public void AddToChain(string key, INullValueDictionary<string, string> prop)
    {
        // adjust the font size
        prop.TryGetValue(HtmlTags.SIZE, out var value);
        if (value == null)
        {
            return;
        }

        if (value.EndsWith("pt"))
        {
            prop[ElementTags.SIZE] = value.Substring(0, value.Length - 2);
        }
        else
        {
            var s = 0;
            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                var old = this["basefontsize"];
                if (old == null)
                {
                    old = "12";
                }

                var f = float.Parse(old, NumberFormatInfo.InvariantInfo);
                var c = (int)f;
                for (var k = FontSizes.Length - 1; k >= 0; --k)
                {
                    if (c >= FontSizes[k])
                    {
                        s = k;
                        break;
                    }
                }

                var inc = int.Parse(value.StartsWith("+") ? value.Substring(1) : value,
                                    CultureInfo.InvariantCulture);
                s += inc;
            }
            else
            {
                try
                {
                    s = int.Parse(value, CultureInfo.InvariantCulture) - 1;
                }
                catch
                {
                    s = 0;
                }
            }

            if (s < 0)
            {
                s = 0;
            }
            else if (s >= FontSizes.Length)
            {
                s = FontSizes.Length - 1;
            }

            prop[ElementTags.SIZE] = FontSizes[s].ToString();
        }

        Chain.Add(new TagAttributes(key, prop));
    }

    public bool HasProperty(string key)
    {
        for (var k = Chain.Count - 1; k >= 0; --k)
        {
            var p = Chain[k];
            var attrs = p.Attrs;
            if (attrs.ContainsKey(key))
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveChain(string key)
    {
        for (var k = Chain.Count - 1; k >= 0; --k)
        {
            if (key.Equals(Chain[k].Tag))
            {
                Chain.RemoveAt(k);
                return;
            }
        }
    }

    /// <summary>
    ///     Class that stores the info about one tag in the chain.
    /// </summary>
    public sealed class TagAttributes
    {
        public TagAttributes(string tag, INullValueDictionary<string, string> attrs)
        {
            Tag = tag;
            Attrs = attrs;
        }

        public INullValueDictionary<string, string> Attrs { set; get; }
        public string Tag { set; get; }
    }
}