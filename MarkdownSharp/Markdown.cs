namespace MarkdownSharp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class Markdown
    {
        private const string _version = "1.13";
        private string _emptyElementSuffix;
        private bool _linkEmails;
        private bool _strictBoldItalic;
        private bool _autoNewlines;
        private bool _autoHyperlink;
        private bool _encodeProblemUrlCharacters;
        private const int _nestDepth = 6;
        private const int _tabWidth = 4;
        private const string _markerUL = "[*+-]";
        private const string _markerOL = @"\d+[.]";
        private static readonly Dictionary<string, string> _escapeTable = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _invertedEscapeTable = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _backslashEscapeTable = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _urls;
        private readonly Dictionary<string, string> _titles;
        private readonly Dictionary<string, string> _htmlBlocks;
        private int _listLevel;
        private static string AutoLinkPreventionMarker = "\x001aP";
        private static Regex _newlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static Regex _newlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);
        private static Regex _leadingWhitespace = new Regex("^[ ]*", RegexOptions.Compiled);
        private static Regex _htmlBlockHash = new Regex("\x001aH\\d+H", RegexOptions.Compiled);
        private static string _nestedBracketsPattern;
        private static string _nestedParensPattern;
        private static Regex _linkDef = new Regex($"
                        ^[ ]{{0,{3}}}\[([^\[\]]+)\]:  # id = $1
                          [ ]*
                          \n?                   # maybe *one* newline
                          [ ]*
                        <?(\S+?)>?              # url = $2
                          [ ]*
                          \n?                   # maybe one newline
                          [ ]*
                        (?:
                            (?<=\s)             # lookbehind for whitespace
                            ["(]
                            (.+?)               # title = $3
                            [")]
                            [ ]*
                        )?                      # title is optional
                        (?:\n+|\Z)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _blocksHtml = new Regex(GetBlockPattern(), RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
        private static Regex _htmlTokens = new Regex("\r\n            (<!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->)|        # match <!-- foo -->\r\n            (<\\?.*?\\?>)|                 # match <?foo?> " + RepeatString("\r\n            (<[A-Za-z\\/!$](?:[^<>]|", 6) + RepeatString(")*>)", 6) + " # match <tag> and </tag>", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        private static Regex _anchorRef = new Regex($"
            (                               # wrap whole match in $1
                \[
                    ({GetNestedBracketsPattern()})                   # link text = $2
                \]

                [ ]?                        # one optional space
                (?:\n[ ]*)?                 # one optional newline followed by spaces

                \[
                    (.*?)                   # id = $3
                \]
            )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _anchorInline = new Regex($"
                (                           # wrap whole match in $1
                    \[
                        ({GetNestedBracketsPattern()})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({GetNestedParensPattern()})               # href = $3
                        [ ]*
                        (                   # $4
                        (['"])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _anchorRefShortcut = new Regex("\r\n            (                               # wrap whole match in $1\r\n              \\[\r\n                 ([^\\[\\]]+)                 # link text = $2; can't contain [ or ]\r\n              \\]\r\n            )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _imagesRef = new Regex("\r\n                    (               # wrap whole match in $1\r\n                    !\\[\r\n                        (.*?)       # alt text = $2\r\n                    \\]\r\n\r\n                    [ ]?            # one optional space\r\n                    (?:\\n[ ]*)?     # one optional newline followed by spaces\r\n\r\n                    \\[\r\n                        (.*?)       # id = $3\r\n                    \\]\r\n\r\n                    )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _imagesInline = new Regex($"
              (                     # wrap whole match in $1
                !\[
                    (.*?)           # alt text = $2
                \]
                \s?                 # one optional whitespace character
                \(                  # literal paren
                    [ ]*
                    ({GetNestedParensPattern()})           # href = $3
                    [ ]*
                    (               # $4
                    (['"])       # quote char = $5
                    (.*?)           # title = $6
                    \5              # matching quote
                    [ ]*
                    )?              # title is optional
                \)
              )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _headerSetext = new Regex("\r\n                ^(.+?)\r\n                [ ]*\r\n                \\n\r\n                (=+|-+)     # $1 = string of ='s or -'s\r\n                [ ]*\r\n                \\n+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _headerAtx = new Regex("\r\n                ^(\\#{1,6})  # $1 = string of #'s\r\n                [ ]*\r\n                (.+?)       # $2 = Header text\r\n                [ ]*\r\n                \\#*         # optional closing #'s (not counted)\r\n                \\n+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _horizontalRules = new Regex("\r\n            ^[ ]{0,3}         # Leading space\r\n                ([-*_])       # $1: First marker\r\n                (?>           # Repeated marker group\r\n                    [ ]{0,2}  # Zero, one, or two spaces.\r\n                    \\1        # Marker character\r\n                ){2,}         # Group repeated at least twice\r\n                [ ]*          # Trailing spaces\r\n                $             # End of line.\r\n            ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static string _wholeList = string.Format("\r\n            (                               # $1 = whole list\r\n              (                             # $2\r\n                [ ]{{0,{1}}}\r\n                ({0})                       # $3 = first list item marker\r\n                [ ]+\r\n              )\r\n              (?s:.+?)\r\n              (                             # $4\r\n                  \\z\r\n                |\r\n                  \\n{{2,}}\r\n                  (?=\\S)\r\n                  (?!                       # Negative lookahead for another list item marker\r\n                    [ ]*\r\n                    {0}[ ]+\r\n                  )\r\n              )\r\n            )", $"(?:{"[*+-]"}|{@"\d+[.]"})", 3);
        private static Regex _listNested = new Regex("^" + _wholeList, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _listTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + _wholeList, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _codeBlock = new Regex(string.Format("\r\n                    (?:\\n\\n|\\A\\n?)\r\n                    (                        # $1 = the code block -- one or more lines, starting with a space\r\n                    (?:\r\n                        (?:[ ]{{{0}}})       # Lines must start with a tab-width of spaces\r\n                        .*\\n+\r\n                    )+\r\n                    )\r\n                    ((?=^[ ]{{0,{0}}}[^ \\t\\n])|\\Z) # Lookahead for non-space at line-start, or end of doc", 4), RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _codeSpan = new Regex("\r\n                    (?<![\\\\`])   # Character before opening ` can't be a backslash or backtick\r\n                    (`+)      # $1 = Opening run of `\r\n                    (?!`)     # and no more backticks -- match the full run\r\n                    (.+?)     # $2 = The code block\r\n                    (?<!`)\r\n                    \\1\r\n                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _bold = new Regex(@"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _strictBold = new Regex(@"(^|[\W_])(?:(?!\1)|(?=^))(\*|_)\2(?=\S)(.*?\S)\2\2(?!\2)(?=[\W_]|$)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _italic = new Regex(@"(\*|_) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _strictItalic = new Regex(@"(^|[\W_])(?:(?!\1)|(?=^))(\*|_)(?=\S)((?:(?!\2).)*?\S)\2(?!\2)(?=[\W_]|$)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex _blockquote = new Regex("\r\n            (                           # Wrap whole match in $1\r\n                (\r\n                ^[ ]*>[ ]?              # '>' at the start of a line\r\n                    .+\\n                # rest of the first line\r\n                (.+\\n)*                 # subsequent consecutive lines\r\n                \\n*                     # blanks\r\n                )+\r\n            )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline);
        private const string _charInsideUrl = "[-A-Z0-9+&@#/%?=~_|\\[\\]\\(\\)!:,\\.;\x001a]";
        private const string _charEndingUrl = @"[-A-Z0-9+&@#/%=~_|\[\])]";
        private static Regex _autolinkBare = new Regex("(<|=\")?\\b(https?|ftp)(://[-A-Z0-9+&@#/%?=~_|\\[\\]\\(\\)!:,\\.;\x001a]*[-A-Z0-9+&@#/%=~_|\\[\\])])(?=$|\\W)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _endCharRegex = new Regex(@"[-A-Z0-9+&@#/%=~_|\[\])]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _outDent = new Regex("^[ ]{1," + 4 + "}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex _codeEncoder = new Regex(@"&|<|>|\\|\*|_|\{|\}|\[|\]", RegexOptions.Compiled);
        private static Regex _amps = new Regex("&(?!((#[0-9]+)|(#[xX][a-fA-F0-9]+)|([a-zA-Z][a-zA-Z0-9]*));)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static Regex _angles = new Regex(@"<(?![A-Za-z/?\$!])", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static Regex _backslashEscapes;
        private static Regex _unescapes = new Regex("\x001aE\\d+E", RegexOptions.Compiled);
        private static readonly char[] _problemUrlChars = "\"'*()[]$:".ToCharArray();

        static Markdown()
        {
            string str = "";
            foreach (char ch in @"\`*_{}[]()>#+-.!/")
            {
                string s = ch.ToString();
                string hashKey = GetHashKey(s, false);
                _escapeTable.Add(s, hashKey);
                _invertedEscapeTable.Add(hashKey, s);
                _backslashEscapeTable.Add(@"\" + s, hashKey);
                str = str + Regex.Escape(@"\" + s) + "|";
            }
            _backslashEscapes = new Regex(str.Substring(0, str.Length - 1), RegexOptions.Compiled);
        }

        public Markdown()
        {
            this._emptyElementSuffix = " />";
            this._linkEmails = true;
            this._urls = new Dictionary<string, string>();
            this._titles = new Dictionary<string, string>();
            this._htmlBlocks = new Dictionary<string, string>();
        }

        public Markdown(MarkdownOptions options)
        {
            this._emptyElementSuffix = " />";
            this._linkEmails = true;
            this._urls = new Dictionary<string, string>();
            this._titles = new Dictionary<string, string>();
            this._htmlBlocks = new Dictionary<string, string>();
            this._autoHyperlink = options.AutoHyperlink;
            this._autoNewlines = options.AutoNewlines;
            this._emptyElementSuffix = options.EmptyElementSuffix;
            this._encodeProblemUrlCharacters = options.EncodeProblemUrlCharacters;
            this._linkEmails = options.LinkEmails;
            this._strictBoldItalic = options.StrictBoldItalic;
        }

        private string AnchorInlineEvaluator(Match match)
        {
            string str = this.SaveFromAutoLinking(match.Groups[2].Value);
            string url = match.Groups[3].Value;
            string str3 = match.Groups[6].Value;
            url = this.EncodeProblemUrlChars(url);
            url = this.EscapeBoldItalic(url);
            if (url.StartsWith("<") && url.EndsWith(">"))
            {
                url = url.Substring(1, url.Length - 2);
            }
            string str4 = $"<a href="{url}"";
            if (!string.IsNullOrEmpty(str3))
            {
                str3 = AttributeEncode(str3);
                str3 = this.EscapeBoldItalic(str3);
                str4 = str4 + $" title="{str3}"";
            }
            return (str4 + $">{str}</a>");
        }

        private string AnchorRefEvaluator(Match match)
        {
            string str4;
            string str = match.Groups[1].Value;
            string str2 = this.SaveFromAutoLinking(match.Groups[2].Value);
            string key = match.Groups[3].Value.ToLowerInvariant();
            if (key == "")
            {
                key = str2.ToLowerInvariant();
            }
            if (!this._urls.ContainsKey(key))
            {
                str4 = str;
            }
            else
            {
                string url = this._urls[key];
                url = this.EncodeProblemUrlChars(url);
                url = this.EscapeBoldItalic(url);
                str4 = "<a href=\"" + url + "\"";
                if (this._titles.ContainsKey(key))
                {
                    string s = AttributeEncode(this._titles[key]);
                    s = AttributeEncode(this.EscapeBoldItalic(s));
                    str4 = str4 + " title=\"" + s + "\"";
                }
                str4 = str4 + ">" + str2 + "</a>";
            }
            return str4;
        }

        private string AnchorRefShortcutEvaluator(Match match)
        {
            string str4;
            string str = match.Groups[1].Value;
            string str2 = this.SaveFromAutoLinking(match.Groups[2].Value);
            string key = Regex.Replace(str2.ToLowerInvariant(), @"[ ]*\n[ ]*", " ");
            if (!this._urls.ContainsKey(key))
            {
                str4 = str;
            }
            else
            {
                string url = this._urls[key];
                url = this.EncodeProblemUrlChars(url);
                url = this.EscapeBoldItalic(url);
                str4 = "<a href=\"" + url + "\"";
                if (this._titles.ContainsKey(key))
                {
                    string s = AttributeEncode(this._titles[key]);
                    s = this.EscapeBoldItalic(s);
                    str4 = str4 + " title=\"" + s + "\"";
                }
                str4 = str4 + ">" + str2 + "</a>";
            }
            return str4;
        }

        private static string AttributeEncode(string s) => 
            s.Replace(">", "&gt;").Replace("<", "&lt;").Replace("\"", "&quot;");

        private string AtxHeaderEvaluator(Match match)
        {
            string text = match.Groups[2].Value;
            int length = match.Groups[1].Value.Length;
            return string.Format("<h{1}>{0}</h{1}>\n\n", this.RunSpanGamut(text), length);
        }

        private string BlockQuoteEvaluator(Match match)
        {
            string text = Regex.Replace(Regex.Replace(match.Groups[1].Value, "^[ ]*>[ ]?", "", RegexOptions.Multiline), "^[ ]+$", "", RegexOptions.Multiline);
            text = Regex.Replace(Regex.Replace(this.RunBlockGamut(text, true), "^", "  ", RegexOptions.Multiline), @"(\s*<pre>.+?</pre>)", new MatchEvaluator(this.BlockQuoteEvaluator2), RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            text = $"<blockquote>
{text}
</blockquote>";
            string hashKey = GetHashKey(text, true);
            this._htmlBlocks[hashKey] = text;
            return ("\n\n" + hashKey + "\n\n");
        }

        private string BlockQuoteEvaluator2(Match match) => 
            Regex.Replace(match.Groups[1].Value, "^  ", "", RegexOptions.Multiline);

        private void Cleanup()
        {
            this.Setup();
        }

        private string CodeBlockEvaluator(Match match)
        {
            string block = match.Groups[1].Value;
            block = this.EncodeCode(this.Outdent(block));
            block = _newlinesLeadingTrailing.Replace(block, "");
            return ("\n\n<pre><code>" + block + "\n</code></pre>\n\n");
        }

        private string CodeSpanEvaluator(Match match)
        {
            string code = Regex.Replace(Regex.Replace(match.Groups[2].Value, "^[ ]*", ""), "[ ]*$", "");
            code = this.EncodeCode(code);
            code = this.SaveFromAutoLinking(code);
            return ("<code>" + code + "</code>");
        }

        private string DoAnchors(string text)
        {
            text = _anchorRef.Replace(text, new MatchEvaluator(this.AnchorRefEvaluator));
            text = _anchorInline.Replace(text, new MatchEvaluator(this.AnchorInlineEvaluator));
            text = _anchorRefShortcut.Replace(text, new MatchEvaluator(this.AnchorRefShortcutEvaluator));
            return text;
        }

        private string DoAutoLinks(string text)
        {
            if (this._autoHyperlink)
            {
                text = _autolinkBare.Replace(text, new MatchEvaluator(Markdown.handleTrailingParens));
            }
            text = Regex.Replace(text, "<((https?|ftp):[^'\">\\s]+)>", new MatchEvaluator(this.HyperlinkEvaluator));
            if (this._linkEmails)
            {
                string pattern = "<\r\n                      (?:mailto:)?\r\n                      (\r\n                        [-.\\w]+\r\n                        \\@\r\n                        [-a-z0-9]+(\\.[-a-z0-9]+)*\\.[a-z]+\r\n                      )\r\n                      >";
                text = Regex.Replace(text, pattern, new MatchEvaluator(this.EmailEvaluator), RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            }
            return text;
        }

        private string DoBlockQuotes(string text) => 
            _blockquote.Replace(text, new MatchEvaluator(this.BlockQuoteEvaluator));

        private string DoCodeBlocks(string text)
        {
            text = _codeBlock.Replace(text, new MatchEvaluator(this.CodeBlockEvaluator));
            return text;
        }

        private string DoCodeSpans(string text) => 
            _codeSpan.Replace(text, new MatchEvaluator(this.CodeSpanEvaluator));

        private string DoHardBreaks(string text)
        {
            text = !this._autoNewlines ? Regex.Replace(text, @" {2,}\n", $"<br{this._emptyElementSuffix}
") : Regex.Replace(text, @"\n", $"<br{this._emptyElementSuffix}
");
            return text;
        }

        private string DoHeaders(string text)
        {
            text = _headerSetext.Replace(text, new MatchEvaluator(this.SetextHeaderEvaluator));
            text = _headerAtx.Replace(text, new MatchEvaluator(this.AtxHeaderEvaluator));
            return text;
        }

        private string DoHorizontalRules(string text) => 
            _horizontalRules.Replace(text, "<hr" + this._emptyElementSuffix + "\n");

        private string DoImages(string text)
        {
            text = _imagesRef.Replace(text, new MatchEvaluator(this.ImageReferenceEvaluator));
            text = _imagesInline.Replace(text, new MatchEvaluator(this.ImageInlineEvaluator));
            return text;
        }

        private string DoItalicsAndBold(string text)
        {
            if (this._strictBoldItalic)
            {
                text = _strictBold.Replace(text, "$1<strong>$3</strong>");
                text = _strictItalic.Replace(text, "$1<em>$3</em>");
            }
            else
            {
                text = _bold.Replace(text, "<strong>$2</strong>");
                text = _italic.Replace(text, "<em>$2</em>");
            }
            return text;
        }

        private string DoLists(string text, bool isInsideParagraphlessListItem = false)
        {
            text = (this._listLevel <= 0) ? _listTopLevel.Replace(text, this.GetListEvaluator(false)) : _listNested.Replace(text, this.GetListEvaluator(isInsideParagraphlessListItem));
            return text;
        }

        private string EmailEvaluator(Match match)
        {
            string addr = this.Unescape(match.Groups[1].Value);
            addr = "mailto:" + addr;
            addr = this.EncodeEmailAddress(addr);
            return Regex.Replace(string.Format("<a href=\"{0}\">{0}</a>", addr), "\">.+?:", "\">");
        }

        private string EncodeAmpsAndAngles(string s)
        {
            s = _amps.Replace(s, "&amp;");
            s = _angles.Replace(s, "&lt;");
            return s;
        }

        private string EncodeCode(string code) => 
            _codeEncoder.Replace(code, new MatchEvaluator(this.EncodeCodeEvaluator));

        private string EncodeCodeEvaluator(Match match)
        {
            string str = match.Value;
            return ((str == "&") ? "&amp;" : ((str == "<") ? "&lt;" : ((str == ">") ? "&gt;" : _escapeTable[match.Value])));
        }

        private string EncodeEmailAddress(string addr)
        {
            StringBuilder builder = new StringBuilder(addr.Length * 5);
            Random random = new Random();
            foreach (char ch in addr)
            {
                int num = random.Next(1, 100);
                if (((num > 90) || (ch == ':')) && (ch != '@'))
                {
                    builder.Append(ch);
                }
                else if (num < 0x2d)
                {
                    builder.AppendFormat("&#x{0:x};", (int) ch);
                }
                else
                {
                    builder.AppendFormat("&#{0};", (int) ch);
                }
            }
            return builder.ToString();
        }

        private string EncodeProblemUrlChars(string url)
        {
            if (!this._encodeProblemUrlCharacters)
            {
                return url;
            }
            StringBuilder builder = new StringBuilder(url.Length);
            for (int i = 0; i < url.Length; i++)
            {
                char ch = url[i];
                bool flag = Array.IndexOf<char>(_problemUrlChars, ch) != -1;
                if (flag && ((ch == ':') && (i < (url.Length - 1))))
                {
                    flag = (url[i + 1] != '/') && ((url[i + 1] < '0') || (url[i + 1] > '9'));
                }
                if (flag)
                {
                    builder.Append("%" + $"{((byte) ch):x}");
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        private string EscapeBackslashes(string s) => 
            _backslashEscapes.Replace(s, new MatchEvaluator(this.EscapeBackslashesEvaluator));

        private string EscapeBackslashesEvaluator(Match match) => 
            _backslashEscapeTable[match.Value];

        private string EscapeBoldItalic(string s)
        {
            s = s.Replace("*", _escapeTable["*"]);
            s = s.Replace("_", _escapeTable["_"]);
            return s;
        }

        private string EscapeImageAltText(string s)
        {
            s = this.EscapeBoldItalic(s);
            s = Regex.Replace(s, @"[\[\]()]", m => _escapeTable[m.ToString()]);
            return s;
        }

        private string EscapeSpecialCharsWithinTagAttributes(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length);
            foreach (Token local1 in this.TokenizeHTML(text))
            {
                string input = local1.Value;
                if (local1.Type == TokenType.Tag)
                {
                    input = input.Replace(@"\", _escapeTable[@"\"]);
                    if (this._autoHyperlink && input.StartsWith("<!"))
                    {
                        input = input.Replace("/", _escapeTable["/"]);
                    }
                    input = Regex.Replace(input, "(?<=.)</?code>(?=.)", _escapeTable["`"]);
                    input = this.EscapeBoldItalic(input);
                }
                builder.Append(input);
            }
            return builder.ToString();
        }

        private string FormParagraphs(string text, bool unhash = true)
        {
            string[] strArray = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text, ""));
            for (int i = 0; i < strArray.Length; i++)
            {
                if (!strArray[i].StartsWith("\x001aH"))
                {
                    strArray[i] = _leadingWhitespace.Replace(this.RunSpanGamut(strArray[i]), "<p>") + "</p>";
                }
                else if (unhash)
                {
                    int num2 = 50;
                    bool keepGoing = true;
                    while (keepGoing && (num2 > 0))
                    {
                        keepGoing = false;
                        strArray[i] = _htmlBlockHash.Replace(strArray[i], delegate (Match match) {
                            keepGoing = true;
                            return this._htmlBlocks[match.Value];
                        });
                        num2--;
                    }
                }
            }
            return string.Join("\n\n", strArray);
        }

        private static string GetBlockPattern()
        {
            string newValue = "ins|del";
            string str2 = "p|div|h[1-6]|blockquote|pre|table|dl|ol|ul|address|script|noscript|form|fieldset|iframe|math";
            string str3 = "\r\n            (?>﻿  ﻿  ﻿  ﻿              # optional tag attributes\r\n              \\s﻿  ﻿  ﻿              # starts with whitespace\r\n              (?>\r\n                [^>\"/]+﻿              # text outside quotes\r\n              |\r\n                /+(?!>)﻿  ﻿              # slash not followed by >\r\n              |\r\n                \"[^\"]*\"﻿  ﻿          # text inside double quotes (tolerate >)\r\n              |\r\n                '[^']*'﻿                  # text inside single quotes (tolerate >)\r\n              )*\r\n            )?﻿\r\n            ";
            string str4 = RepeatString("\r\n                (?>\r\n                  [^<]+﻿  ﻿  ﻿          # content without tag\r\n                |\r\n                  <\\2﻿  ﻿  ﻿          # nested opening tag\r\n                    " + str3 + "       # attributes\r\n                  (?>\r\n                      />\r\n                  |\r\n                      >", 6) + ".*?" + RepeatString("\r\n                      </\\2\\s*>﻿          # closing nested tag\r\n                  )\r\n                  |﻿  ﻿  ﻿  ﻿\r\n                  <(?!/\\2\\s*>           # other tags with a different name\r\n                  )\r\n                )*", 6);
            string str5 = str4.Replace(@"\2", @"\3");
            return "\r\n            (?>\r\n                  (?>\r\n                    (?<=\\n)     # Starting at the beginning of a line\r\n                    |           # or\r\n                    \\A\\n?       # the beginning of the doc\r\n                  )\r\n                  (             # save in $1\r\n\r\n                    # Match from `\\n<tag>` to `</tag>\\n`, handling nested tags\r\n                    # in between.\r\n\r\n                        <($block_tags_b_re)   # start tag = $2\r\n                        $attr>                # attributes followed by > and \\n\r\n                        $content              # content, support nesting\r\n                        </\\2>                 # the matching end tag\r\n                        [ ]*                  # trailing spaces\r\n                        (?=\\n+|\\Z)            # followed by a newline or end of document\r\n\r\n                  | # Special version for tags of group a.\r\n\r\n                        <($block_tags_a_re)   # start tag = $3\r\n                        $attr>[ ]*\\n          # attributes followed by >\r\n                        $content2             # content, support nesting\r\n                        </\\3>                 # the matching end tag\r\n                        [ ]*                  # trailing spaces\r\n                        (?=\\n+|\\Z)            # followed by a newline or end of document\r\n\r\n                  | # Special case just for <hr />. It was easier to make a special\r\n                    # case than to make the other regex more complicated.\r\n\r\n                        [ ]{0,$less_than_tab}\r\n                        <hr\r\n                        $attr                 # attributes\r\n                        /?>                   # the matching end tag\r\n                        [ ]*\r\n                        (?=\\n{2,}|\\Z)         # followed by a blank line or end of document\r\n\r\n                  | # Special case for standalone HTML comments:\r\n\r\n                      (?<=\\n\\n|\\A)            # preceded by a blank line or start of document\r\n                      [ ]{0,$less_than_tab}\r\n                      (?s:\r\n                        <!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->\r\n                      )\r\n                      [ ]*\r\n                      (?=\\n{2,}|\\Z)            # followed by a blank line or end of document\r\n\r\n                  | # PHP and ASP-style processor instructions (<? and <%)\r\n\r\n                      [ ]{0,$less_than_tab}\r\n                      (?s:\r\n                        <([?%])                # $4\r\n                        .*?\r\n                        \\4>\r\n                      )\r\n                      [ ]*\r\n                      (?=\\n{2,}|\\Z)            # followed by a blank line or end of document\r\n\r\n                  )\r\n            )".Replace("$less_than_tab", 3.ToString()).Replace("$block_tags_b_re", str2).Replace("$block_tags_a_re", newValue).Replace("$attr", str3).Replace("$content2", str5).Replace("$content", str4);
        }

        private static string GetHashKey(string s, bool isHtmlBlock)
        {
            char ch = isHtmlBlock ? 'H' : 'E';
            return ("\x001a" + ch.ToString() + Math.Abs(s.GetHashCode()).ToString() + ch.ToString());
        }

        private MatchEvaluator GetListEvaluator(bool isInsideParagraphlessListItem = false) => 
            delegate (Match match) {
                string list = match.Groups[1].Value;
                string str2 = Regex.IsMatch(match.Groups[3].Value, "[*+-]") ? "ul" : "ol";
                return string.Format("<{0}>\n{1}</{0}>\n", str2, this.ProcessListItems(list, (str2 == "ul") ? "[*+-]" : @"\d+[.]", isInsideParagraphlessListItem));
            };

        private static string GetNestedBracketsPattern()
        {
            if (_nestedBracketsPattern == null)
            {
                _nestedBracketsPattern = RepeatString("\r\n                    (?>              # Atomic matching\r\n                       [^\\[\\]]+      # Anything other than brackets\r\n                     |\r\n                       \\[\r\n                           ", 6) + RepeatString(" \\]\r\n                    )*", 6);
            }
            return _nestedBracketsPattern;
        }

        private static string GetNestedParensPattern()
        {
            if (_nestedParensPattern == null)
            {
                _nestedParensPattern = RepeatString("\r\n                    (?>              # Atomic matching\r\n                       [^()\\s]+      # Anything other than parens or whitespace\r\n                     |\r\n                       \\(\r\n                           ", 6) + RepeatString(" \\)\r\n                    )*", 6);
            }
            return _nestedParensPattern;
        }

        private static string handleTrailingParens(Match match)
        {
            if (match.Groups[1].Success)
            {
                return match.Value;
            }
            string str = match.Groups[2].Value;
            string input = match.Groups[3].Value;
            if (!input.EndsWith(")"))
            {
                return ("<" + str + input + ">");
            }
            int num = 0;
            using (IEnumerator enumerator = Regex.Matches(input, "[()]").GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num = (((Match) enumerator.Current).Value != "(") ? (num - 1) : ((num > 0) ? (num + 1) : 1);
                }
            }
            string tail = "";
            if (num < 0)
            {
                input = Regex.Replace(input, @"\){1," + -num + "}$", delegate (Match m) {
                    tail = m.Value;
                    return "";
                });
            }
            if (tail.Length > 0)
            {
                char ch = input[input.Length - 1];
                if (!_endCharRegex.IsMatch(ch.ToString()))
                {
                    tail = ch.ToString() + tail;
                    input = input.Substring(0, input.Length - 1);
                }
            }
            string[] textArray1 = new string[] { "<", str, input, ">", tail };
            return string.Concat(textArray1);
        }

        private string HashHTMLBlocks(string text) => 
            _blocksHtml.Replace(text, new MatchEvaluator(this.HtmlEvaluator));

        private string HtmlEvaluator(Match match)
        {
            string s = match.Groups[1].Value;
            string hashKey = GetHashKey(s, true);
            this._htmlBlocks[hashKey] = s;
            return ("\n\n" + hashKey + "\n\n");
        }

        private string HyperlinkEvaluator(Match match)
        {
            string str = match.Groups[1].Value;
            return $"<a href="{this.EscapeBoldItalic(this.EncodeProblemUrlChars(str))}">{str}</a>";
        }

        private string ImageInlineEvaluator(Match match)
        {
            string altText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;
            if (url.StartsWith("<") && url.EndsWith(">"))
            {
                url = url.Substring(1, url.Length - 2);
            }
            return this.ImageTag(url, altText, title);
        }

        private string ImageReferenceEvaluator(Match match)
        {
            string str = match.Groups[1].Value;
            string altText = match.Groups[2].Value;
            string key = match.Groups[3].Value.ToLowerInvariant();
            if (key == "")
            {
                key = altText.ToLowerInvariant();
            }
            if (!this._urls.ContainsKey(key))
            {
                return str;
            }
            string url = this._urls[key];
            string title = null;
            if (this._titles.ContainsKey(key))
            {
                title = this._titles[key];
            }
            return this.ImageTag(url, altText, title);
        }

        private string ImageTag(string url, string altText, string title)
        {
            altText = this.EscapeImageAltText(AttributeEncode(altText));
            url = this.EncodeProblemUrlChars(url);
            url = this.EscapeBoldItalic(url);
            string str = $"<img src="{url}" alt="{altText}"";
            if (!string.IsNullOrEmpty(title))
            {
                title = AttributeEncode(this.EscapeBoldItalic(title));
                str = str + $" title="{title}"";
            }
            return (str + this._emptyElementSuffix);
        }

        private string LinkEvaluator(Match match)
        {
            string str = match.Groups[1].Value.ToLowerInvariant();
            this._urls[str] = this.EncodeAmpsAndAngles(match.Groups[2].Value);
            if ((match.Groups[3] != null) && (match.Groups[3].Length > 0))
            {
                this._titles[str] = match.Groups[3].Value.Replace("\"", "&quot;");
            }
            return "";
        }

        private string Normalize(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length);
            StringBuilder builder2 = new StringBuilder();
            bool flag = false;
            int num = 0;
            goto TR_0016;
        TR_0003:
            num++;
        TR_0016:
            while (true)
            {
                if (num >= text.Length)
                {
                    if (flag)
                    {
                        builder.Append(builder2);
                    }
                    builder.Append('\n');
                    return builder.Append("\n\n").ToString();
                }
                char ch = text[num];
                switch (ch)
                {
                    case '\t':
                    {
                        int num2 = 4 - (builder2.Length % 4);
                        for (int i = 0; i < num2; i++)
                        {
                            builder2.Append(' ');
                        }
                        goto TR_0003;
                    }
                    case '\n':
                        if (flag)
                        {
                            builder.Append(builder2);
                        }
                        builder.Append('\n');
                        builder2.Length = 0;
                        flag = false;
                        goto TR_0003;

                    case '\v':
                    case '\f':
                        break;

                    case '\r':
                        if ((num < (text.Length - 1)) && (text[num + 1] != '\n'))
                        {
                            if (flag)
                            {
                                builder.Append(builder2);
                            }
                            builder.Append('\n');
                            builder2.Length = 0;
                            flag = false;
                        }
                        goto TR_0003;

                    default:
                        if (ch != '\x001a')
                        {
                            break;
                        }
                        goto TR_0003;
                }
                if (!flag && (text[num] != ' '))
                {
                    flag = true;
                }
                builder2.Append(text[num]);
                break;
            }
            goto TR_0003;
        }

        private string Outdent(string block) => 
            _outDent.Replace(block, "");

        private string ProcessListItems(string list, string marker, bool isInsideParagraphlessListItem = false)
        {
            this._listLevel++;
            list = Regex.Replace(list, @"\n{2,}\z", "\n");
            string pattern = string.Format("(^[ ]*)                    # leading whitespace = $1\r\n                ({0}) [ ]+                 # list marker = $2\r\n                ((?s:.+?)                  # list item text = $3\r\n                (\\n+))\r\n                (?= (\\z | \\1 ({0}) [ ]+))", marker);
            bool lastItemHadADoubleNewline = false;
            list = Regex.Replace(list, pattern, new MatchEvaluator(delegate (Match match) {
                string block = match.Groups[3].Value;
                bool flag = block.EndsWith("\n\n");
                if ((flag || block.Contains("\n\n")) | lastItemHadADoubleNewline)
                {
                    block = this.RunBlockGamut(this.Outdent(block) + "\n", false);
                }
                else
                {
                    char[] trimChars = new char[] { '\n' };
                    block = this.DoLists(this.Outdent(block), true).TrimEnd(trimChars);
                    if (!isInsideParagraphlessListItem)
                    {
                        block = this.RunSpanGamut(block);
                    }
                }
                lastItemHadADoubleNewline = flag;
                return $"<li>{block}</li>
";
            }.Invoke), RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            this._listLevel--;
            return list;
        }

        private static string RepeatString(string text, int count)
        {
            StringBuilder builder = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
            {
                builder.Append(text);
            }
            return builder.ToString();
        }

        private string RunBlockGamut(string text, bool unhash = true)
        {
            text = this.DoHeaders(text);
            text = this.DoHorizontalRules(text);
            text = this.DoLists(text, false);
            text = this.DoCodeBlocks(text);
            text = this.DoBlockQuotes(text);
            text = this.HashHTMLBlocks(text);
            text = this.FormParagraphs(text, unhash);
            return text;
        }

        private string RunSpanGamut(string text)
        {
            text = this.DoCodeSpans(text);
            text = this.EscapeSpecialCharsWithinTagAttributes(text);
            text = this.EscapeBackslashes(text);
            text = this.DoImages(text);
            text = this.DoAnchors(text);
            text = this.DoAutoLinks(text);
            text = text.Replace(AutoLinkPreventionMarker, "://");
            text = this.EncodeAmpsAndAngles(text);
            text = this.DoItalicsAndBold(text);
            text = this.DoHardBreaks(text);
            return text;
        }

        private string SaveFromAutoLinking(string s) => 
            s.Replace("://", AutoLinkPreventionMarker);

        private string SetextHeaderEvaluator(Match match)
        {
            string text = match.Groups[1].Value;
            int num = match.Groups[2].Value.StartsWith("=") ? 1 : 2;
            return string.Format("<h{1}>{0}</h{1}>\n\n", this.RunSpanGamut(text), num);
        }

        private void Setup()
        {
            this._urls.Clear();
            this._titles.Clear();
            this._htmlBlocks.Clear();
            this._listLevel = 0;
        }

        private string StripLinkDefinitions(string text) => 
            _linkDef.Replace(text, new MatchEvaluator(this.LinkEvaluator));

        private List<Token> TokenizeHTML(string text)
        {
            int startIndex = 0;
            int index = 0;
            List<Token> list = new List<Token>();
            foreach (Match match in _htmlTokens.Matches(text))
            {
                index = match.Index;
                if (startIndex < index)
                {
                    list.Add(new Token(TokenType.Text, text.Substring(startIndex, index - startIndex)));
                }
                list.Add(new Token(TokenType.Tag, match.Value));
                startIndex = index + match.Length;
            }
            if (startIndex < text.Length)
            {
                list.Add(new Token(TokenType.Text, text.Substring(startIndex, text.Length - startIndex)));
            }
            return list;
        }

        public string Transform(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            this.Setup();
            text = this.Normalize(text);
            text = this.HashHTMLBlocks(text);
            text = this.StripLinkDefinitions(text);
            text = this.RunBlockGamut(text, true);
            text = this.Unescape(text);
            this.Cleanup();
            return (text + "\n");
        }

        private string Unescape(string s) => 
            _unescapes.Replace(s, new MatchEvaluator(this.UnescapeEvaluator));

        private string UnescapeEvaluator(Match match) => 
            _invertedEscapeTable[match.Value];

        public string EmptyElementSuffix
        {
            get => 
                this._emptyElementSuffix;
            set => 
                (this._emptyElementSuffix = value);
        }

        public bool LinkEmails
        {
            get => 
                this._linkEmails;
            set => 
                (this._linkEmails = value);
        }

        public bool StrictBoldItalic
        {
            get => 
                this._strictBoldItalic;
            set => 
                (this._strictBoldItalic = value);
        }

        public bool AutoNewLines
        {
            get => 
                this._autoNewlines;
            set => 
                (this._autoNewlines = value);
        }

        public bool AutoHyperlink
        {
            get => 
                this._autoHyperlink;
            set => 
                (this._autoHyperlink = value);
        }

        public bool EncodeProblemUrlCharacters
        {
            get => 
                this._encodeProblemUrlCharacters;
            set => 
                (this._encodeProblemUrlCharacters = value);
        }

        public string Version =>
            "1.13";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Markdown.<>c <>9 = new Markdown.<>c();
            public static MatchEvaluator <>9__79_0;

            internal string <EscapeImageAltText>b__79_0(Match m) => 
                Markdown._escapeTable[m.ToString()];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Token
        {
            public Markdown.TokenType Type;
            public string Value;
            public Token(Markdown.TokenType type, string value)
            {
                this.Type = type;
                this.Value = value;
            }
        }

        private enum TokenType
        {
            Text,
            Tag
        }
    }
}

