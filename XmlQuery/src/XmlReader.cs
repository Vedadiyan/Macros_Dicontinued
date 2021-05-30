using System;
using System.Collections.Generic;
using System.Text;

namespace Macros.XmlQuery
{
    public class XmlReader
    {
        private readonly string xmlString;
        private int skip;
        private int index;
        private List<XmlReader> readers;
        private List<string> tags;
        private List<Dictionary<string, string>> attributes;
        private List<string> values;
        public int Count => Tags.Count;

        public List<string> Tags { get => tags; set => tags = value; }

        public XmlReader(string xmlString, int skip = 0)
        {
            this.xmlString = xmlString;
            this.skip = skip;
            readers = new List<XmlReader>();
            Tags = new List<string>();
            attributes = new List<Dictionary<string, string>>();
            values = new List<string>();
        }
        public T Query<T>(string path)
        {
            string[] segments = path.Split('.');
            string value = null;
            XmlReader current = this;
            for (int i = 0, index = 0; i < segments.Length; i++)
            {
                int _index = 0;
                string _segment = null; ;
                if (segments[i].EndsWith("]"))
                {
                    var reference = segments[i].Split('[');
                    _segment = reference[0];
                    _index = int.Parse(reference[1].TrimEnd(']'));
                    if (_index < current.readers.Count)
                    {
                        if (current.readers[_index].Tags.Count == 0)
                        {
                            value = current.values[_index];
                            break;
                        }
                        else
                        {
                            current = current.readers[_index];
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    _segment = segments[i];
                    string find = null;
                    for (int x = 0; x < current.Tags.Count; x++)
                    {
                        var _tag = current.Tags[x];
                        if (_tag.TrimStart('<').TrimEnd('>').Split()[0].Equals(_segment, StringComparison.OrdinalIgnoreCase))
                        {
                            find = _tag;
                            break;
                        }
                    }
                    if (find != null && (index = current.Tags.IndexOf(find)) != -1)
                    {
                        if (current.readers[index].Tags.Count == 0)
                        {
                            value = current.values[index];
                            break;
                        }
                        else
                        {
                            current = current.readers[index];
                        }
                    }
                }
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
        public XmlReader[] MoveTo(string path)
        {
            if(path.Contains('[') || path.Contains(']')) {
                throw new Exception("Move function does not support array item lookup");
            }
            string[] segments = path.Split('.');
            XmlReader current = this;
            for (int i = 0, index = 0; i < segments.Length; i++)
            {
                string _segment = null;
                _segment = segments[i];
                string find = null;
                for (int x = 0; x < current.Tags.Count; x++)
                {
                    var _tag = current.Tags[x];
                    if (_tag.TrimStart('<').TrimEnd('>').Split()[0].Equals(_segment, StringComparison.OrdinalIgnoreCase))
                    {
                        find = _tag;
                        break;
                    }
                }
                if (find != null && (index = current.Tags.IndexOf(find)) != -1)
                {
                    current = current.readers[index];
                }
            }
            if (current != this)
            {
                return current.readers.ToArray();
            }
            else
            {
                return null;
            }
        }
        public string GetValue(string tag, int index)
        {
            for (int i = 0, x = 0; i < Tags.Count; i++)
            {
                if (Tags[i].Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    if (x == index)
                    {
                        return (string)values[i];
                    }
                    x++;
                }
            }
            return null;
        }
        public T GetValue<T>(string tag, int index)
        {
            return (T)Convert.ChangeType(GetValue(tag, index), typeof(T));
        }
        public T GetValue<T>(string tag)
        {
            return GetValue<T>(tag, 0);
        }
        public void Parse()
        {
            StringBuilder startTag = new StringBuilder();
            StringBuilder endTag = new StringBuilder();
            StringBuilder value = null;
            bool startTagSet = false;
            bool endTagSet = false;
            for (int i = skip; i < xmlString.Length; i++)
            {
                switch (xmlString[i])
                {
                    case '<':
                        if (startTagSet || endTagSet)
                        {
                            if (startTagSet && !endTagSet)
                            {
                                XmlReader reader = new XmlReader(xmlString, i);
                                reader.Parse();
                                var formattedTag = formatTag(startTag.ToString());
                                Tags.Add(formattedTag.Tag);
                                attributes.Add(formattedTag.attributes);
                                startTag.Clear();
                                readers.Add(reader);
                                startTagSet = false;
                                values.Add(value?.ToString());
                                value?.Clear();
                                if (reader.index != 0)
                                {
                                    i = reader.index;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else if (startTagSet && endTagSet)
                            {
                                var formattedTag = formatTag(startTag.ToString());
                                Tags.Add(formattedTag.Tag);
                                attributes.Add(formattedTag.attributes);
                                values.Add(value?.ToString());
                                startTagSet = false;
                                endTagSet = false;
                            }
                            else
                            {
                                endTagSet = false;
                                endTag.Clear();
                                return;
                            }
                        }
                        else
                        {
                            if (xmlString[i + 1] == '/')
                            {
                                endTag.Clear();
                                do { endTag.Append(xmlString[i]); }
                                while (++i < xmlString.Length && xmlString[i - 1] != '>');
                                endTagSet = true;
                                index = i;
                            }
                            else
                            {
                                do { startTag.Append(xmlString[i]); }
                                while (++i < xmlString.Length && xmlString[i - 1] != '>');
                                startTagSet = true;

                            }
                        }
                        while (++i < xmlString.Length && (isWhiteSpace(xmlString[i - 1]))) ;
                        i -= 2;
                        value?.Clear();
                        break;
                    default:
                        if (value == null)
                        {
                            value = new StringBuilder();
                        }
                        var c = xmlString[i];
                        value.Append(xmlString[i]);
                        break;
                }
            }
        }
        private (string Tag, Dictionary<string, string> attributes) formatTag(string tag)
        {
            Dictionary<string, string> attributes = null;
            var segments = tag.TrimStart('<').TrimEnd('>').TrimEnd('/').Split(new char[] { ' ' }, 2);
            if (segments.Length > 1)
            {
                attributes = new Dictionary<string, string>();
                var values = segments[1].Split(new string[] { "=", "\"" }, StringSplitOptions.None);
                for (int i = 0; i < values.Length - 3; i += 3)
                {
                    attributes.Add(values[i].TrimStart().TrimEnd(), values[i + 2]);
                }
            }
            return (segments[0], attributes);
        }
        private bool isWhiteSpace(char c)
        {
            return c == ' ' || c == '\r' || c == '\n' || c == '\t';
        }
        public XmlReader this[string tag, int index]
        {
            get
            {
                for (int i = 0, x = 0; i < Tags.Count; i++)
                {
                    if (Tags[i].Equals(tag, StringComparison.OrdinalIgnoreCase))
                    {
                        if (x == index)
                        {
                            return readers[i];
                        }
                        x++;
                    }
                }
                return null;
            }
        }
    }

}