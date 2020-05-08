namespace Squirrel.Json
{
    using Squirrel.Json.Reflection;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    [GeneratedCode("simple-json", "1.0.0")]
    internal static class SimpleJson
    {
        private const int TOKEN_NONE = 0;
        private const int TOKEN_CURLY_OPEN = 1;
        private const int TOKEN_CURLY_CLOSE = 2;
        private const int TOKEN_SQUARED_OPEN = 3;
        private const int TOKEN_SQUARED_CLOSE = 4;
        private const int TOKEN_COLON = 5;
        private const int TOKEN_COMMA = 6;
        private const int TOKEN_STRING = 7;
        private const int TOKEN_NUMBER = 8;
        private const int TOKEN_TRUE = 9;
        private const int TOKEN_FALSE = 10;
        private const int TOKEN_NULL = 11;
        private const int BUILDER_CAPACITY = 0x7d0;
        private static readonly char[] EscapeTable = new char[0x5d];
        private static readonly char[] EscapeCharacters = new char[] { '"', '\\', '\b', '\f', '\n', '\r', '\t' };
        private static readonly string EscapeCharactersString = new string(EscapeCharacters);
        private static IJsonSerializerStrategy _currentJsonSerializerStrategy;
        private static Squirrel.Json.PocoJsonSerializerStrategy _pocoJsonSerializerStrategy;
        private static Squirrel.Json.DataContractJsonSerializerStrategy _dataContractJsonSerializerStrategy;

        static SimpleJson()
        {
            EscapeTable[0x22] = '"';
            EscapeTable[0x5c] = '\\';
            EscapeTable[8] = 'b';
            EscapeTable[12] = 'f';
            EscapeTable[10] = 'n';
            EscapeTable[13] = 'r';
            EscapeTable[9] = 't';
        }

        private static string ConvertFromUtf32(int utf32)
        {
            if ((utf32 < 0) || (utf32 > 0x10ffff))
            {
                throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
            }
            if ((0xd800 <= utf32) && (utf32 <= 0xdfff))
            {
                throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
            }
            if (utf32 < 0x10000)
            {
                return new string((char) utf32, 1);
            }
            utf32 -= 0x10000;
            char[] chArray1 = new char[] { (char) ((utf32 >> 10) + 0xd800), (char) ((utf32 % 0x400) + 0xdc00) };
            return new string(chArray1);
        }

        public static object DeserializeObject(string json)
        {
            object obj2;
            if (!TryDeserializeObject(json, out obj2))
            {
                throw new SerializationException("Invalid JSON string");
            }
            return obj2;
        }

        public static T DeserializeObject<T>(string json) => 
            ((T) DeserializeObject(json, typeof(T), null));

        public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy) => 
            ((T) DeserializeObject(json, typeof(T), jsonSerializerStrategy));

        public static object DeserializeObject(string json, Type type) => 
            DeserializeObject(json, type, null);

        public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
        {
            object obj2 = DeserializeObject(json);
            return (((type == null) || ((obj2 != null) && ReflectionUtils.IsAssignableFrom(obj2.GetType(), type))) ? obj2 : (jsonSerializerStrategy ?? CurrentJsonSerializerStrategy).DeserializeObject(obj2, type));
        }

        private static void EatWhitespace(char[] json, ref int index)
        {
            while ((index < json.Length) && (" \t\n\r\b\f".IndexOf(json[index]) != -1))
            {
                index++;
            }
        }

        public static string EscapeToJavascriptString(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return jsonString;
            }
            StringBuilder builder = new StringBuilder();
            int num = 0;
            while (num < jsonString.Length)
            {
                char ch = jsonString[num++];
                if (ch != '\\')
                {
                    builder.Append(ch);
                    continue;
                }
                if ((jsonString.Length - num) >= 2)
                {
                    char ch2 = jsonString[num];
                    if (ch2 == '\\')
                    {
                        builder.Append('\\');
                        num++;
                        continue;
                    }
                    if (ch2 == '"')
                    {
                        builder.Append("\"");
                        num++;
                        continue;
                    }
                    if (ch2 == 't')
                    {
                        builder.Append('\t');
                        num++;
                        continue;
                    }
                    if (ch2 == 'b')
                    {
                        builder.Append('\b');
                        num++;
                        continue;
                    }
                    if (ch2 == 'n')
                    {
                        builder.Append('\n');
                        num++;
                        continue;
                    }
                    if (ch2 == 'r')
                    {
                        builder.Append('\r');
                        num++;
                    }
                }
            }
            return builder.ToString();
        }

        private static int GetLastIndexOfNumber(char[] json, int index)
        {
            int num = index;
            while ((num < json.Length) && ("0123456789+-.eE".IndexOf(json[num]) != -1))
            {
                num++;
            }
            return (num - 1);
        }

        private static bool IsNumeric(object value) => 
            (!(value is sbyte) ? (!(value is byte) ? (!(value is short) ? (!(value is ushort) ? (!(value is int) ? (!(value is uint) ? (!(value is long) ? (!(value is ulong) ? (!(value is float) ? (!(value is double) ? (value is decimal) : true) : true) : true) : true) : true) : true) : true) : true) : true) : true);

        private static int LookAhead(char[] json, int index)
        {
            int num = index;
            return NextToken(json, ref num);
        }

        private static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);
            if (index == json.Length)
            {
                return 0;
            }
            char ch = json[index];
            index++;
            if (ch > '[')
            {
                if (ch == ']')
                {
                    return 4;
                }
                if (ch == '{')
                {
                    return 1;
                }
                if (ch == '}')
                {
                    return 2;
                }
            }
            else
            {
                switch (ch)
                {
                    case '"':
                        return 7;

                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '\'':
                    case '(':
                    case ')':
                    case '*':
                    case '+':
                    case '.':
                    case '/':
                        break;

                    case ',':
                        return 6;

                    case '-':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        return 8;

                    case ':':
                        return 5;

                    default:
                        if (ch != '[')
                        {
                            break;
                        }
                        return 3;
                }
            }
            index--;
            int num = json.Length - index;
            if ((num >= 5) && ((json[index] == 'f') && ((json[index + 1] == 'a') && ((json[index + 2] == 'l') && ((json[index + 3] == 's') && (json[index + 4] == 'e'))))))
            {
                index += 5;
                return 10;
            }
            if ((num >= 4) && ((json[index] == 't') && ((json[index + 1] == 'r') && ((json[index + 2] == 'u') && (json[index + 3] == 'e')))))
            {
                index += 4;
                return 9;
            }
            if ((num < 4) || ((json[index] != 'n') || ((json[index + 1] != 'u') || ((json[index + 2] != 'l') || (json[index + 3] != 'l')))))
            {
                return 0;
            }
            index += 4;
            return 11;
        }

        private static JsonArray ParseArray(char[] json, ref int index, ref bool success)
        {
            JsonArray array = new JsonArray();
            NextToken(json, ref index);
            bool flag = false;
            while (true)
            {
                if (!flag)
                {
                    int num = LookAhead(json, index);
                    if (num == 0)
                    {
                        success = false;
                        return null;
                    }
                    if (num == 6)
                    {
                        NextToken(json, ref index);
                        continue;
                    }
                    if (num != 4)
                    {
                        object item = ParseValue(json, ref index, ref success);
                        if (!success)
                        {
                            return null;
                        }
                        array.Add(item);
                        continue;
                    }
                    NextToken(json, ref index);
                }
                return array;
            }
        }

        private static object ParseNumber(char[] json, ref int index, ref bool success)
        {
            object obj2;
            EatWhitespace(json, ref index);
            int lastIndexOfNumber = GetLastIndexOfNumber(json, index);
            int length = (lastIndexOfNumber - index) + 1;
            string str = new string(json, index, length);
            if ((str.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1) && (str.IndexOf("e", StringComparison.OrdinalIgnoreCase) == -1))
            {
                long num4;
                success = long.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out num4);
                obj2 = num4;
            }
            else
            {
                double num3;
                success = double.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out num3);
                obj2 = num3;
            }
            index = lastIndexOfNumber + 1;
            return obj2;
        }

        private static IDictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
        {
            IDictionary<string, object> dictionary = new JsonObject();
            NextToken(json, ref index);
            bool flag = false;
            while (!flag)
            {
                int num = LookAhead(json, index);
                if (num == 0)
                {
                    success = false;
                    return null;
                }
                if (num == 6)
                {
                    NextToken(json, ref index);
                    continue;
                }
                if (num == 2)
                {
                    NextToken(json, ref index);
                    return dictionary;
                }
                string str = ParseString(json, ref index, ref success);
                if (!success)
                {
                    success = false;
                    return null;
                }
                if (NextToken(json, ref index) != 5)
                {
                    success = false;
                    return null;
                }
                object obj2 = ParseValue(json, ref index, ref success);
                if (!success)
                {
                    success = false;
                    return null;
                }
                dictionary[str] = obj2;
            }
            return dictionary;
        }

        private static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder builder = new StringBuilder(0x7d0);
            EatWhitespace(json, ref index);
            int num = index;
            index = num + 1;
            char ch = json[num];
            bool flag = false;
            while (true)
            {
                if (!flag && (index != json.Length))
                {
                    num = index;
                    index = num + 1;
                    ch = json[num];
                    if (ch == '"')
                    {
                        flag = true;
                    }
                    else
                    {
                        if (ch != '\\')
                        {
                            builder.Append(ch);
                            continue;
                        }
                        if (index != json.Length)
                        {
                            num = index;
                            index = num + 1;
                            ch = json[num];
                            if (ch == '"')
                            {
                                builder.Append('"');
                                continue;
                            }
                            if (ch == '\\')
                            {
                                builder.Append('\\');
                                continue;
                            }
                            if (ch == '/')
                            {
                                builder.Append('/');
                                continue;
                            }
                            if (ch == 'b')
                            {
                                builder.Append('\b');
                                continue;
                            }
                            if (ch == 'f')
                            {
                                builder.Append('\f');
                                continue;
                            }
                            if (ch == 'n')
                            {
                                builder.Append('\n');
                                continue;
                            }
                            if (ch == 'r')
                            {
                                builder.Append('\r');
                                continue;
                            }
                            if (ch == 't')
                            {
                                builder.Append('\t');
                                continue;
                            }
                            if (ch != 'u')
                            {
                                continue;
                            }
                            if ((json.Length - index) >= 4)
                            {
                                uint num2;
                                bool flag2;
                                uint num3;
                                success = flag2 = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2);
                                if (!flag2)
                                {
                                    return "";
                                }
                                if ((0xd800 > num2) || (num2 > 0xdbff))
                                {
                                    builder.Append(ConvertFromUtf32((int) num2));
                                    index += 4;
                                    continue;
                                }
                                index += 4;
                                if (((json.Length - index) < 6) || ((new string(json, index, 2) != @"\u") || (!uint.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || ((0xdc00 > num3) || (num3 > 0xdfff)))))
                                {
                                    success = false;
                                    return "";
                                }
                                builder.Append((char) num2);
                                builder.Append((char) num3);
                                index += 6;
                                continue;
                            }
                        }
                    }
                }
                if (flag)
                {
                    return builder.ToString();
                }
                success = false;
                return null;
            }
        }

        private static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case 1:
                    return ParseObject(json, ref index, ref success);

                case 3:
                    return ParseArray(json, ref index, ref success);

                case 7:
                    return ParseString(json, ref index, ref success);

                case 8:
                    return ParseNumber(json, ref index, ref success);

                case 9:
                    NextToken(json, ref index);
                    return true;

                case 10:
                    NextToken(json, ref index);
                    return false;

                case 11:
                    NextToken(json, ref index);
                    return null;
            }
            success = false;
            return null;
        }

        private static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder)
        {
            bool flag2;
            builder.Append("[");
            bool flag = true;
            using (IEnumerator enumerator = anArray.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (!flag)
                        {
                            builder.Append(",");
                        }
                        if (!SerializeValue(jsonSerializerStrategy, current, builder))
                        {
                            flag2 = false;
                            break;
                        }
                        flag = false;
                        continue;
                    }
                    builder.Append("]");
                    return true;
                }
            }
            return flag2;
        }

        private static bool SerializeNumber(object number, StringBuilder builder)
        {
            switch (number)
            {
                case (long _):
                    builder.Append(((long) number).ToString(CultureInfo.InvariantCulture));
                    break;

                case (ulong _):
                    builder.Append(((ulong) number).ToString(CultureInfo.InvariantCulture));
                    break;

                case (int _):
                    builder.Append(((int) number).ToString(CultureInfo.InvariantCulture));
                    break;

                case (uint _):
                    builder.Append(((uint) number).ToString(CultureInfo.InvariantCulture));
                    break;

                case (decimal _):
                    builder.Append(((decimal) number).ToString(CultureInfo.InvariantCulture));
                    break;

                case (float _):
                    builder.Append(((float) number).ToString(CultureInfo.InvariantCulture));
                    break;

                default:
                    builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
                    break;
            }
            return true;
        }

        public static string SerializeObject(object json) => 
            SerializeObject(json, CurrentJsonSerializerStrategy);

        public static string SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy)
        {
            StringBuilder builder = new StringBuilder(0x7d0);
            return (SerializeValue(jsonSerializerStrategy, json, builder) ? builder.ToString() : null);
        }

        private static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
        {
            builder.Append("{");
            IEnumerator enumerator = keys.GetEnumerator();
            IEnumerator enumerator2 = values.GetEnumerator();
            for (bool flag = true; enumerator.MoveNext() && enumerator2.MoveNext(); flag = false)
            {
                object current = enumerator2.Current;
                if (!flag)
                {
                    builder.Append(",");
                }
                string aString = enumerator.Current as string;
                if (aString != null)
                {
                    SerializeString(aString, builder);
                }
                else if (!SerializeValue(jsonSerializerStrategy, current, builder))
                {
                    return false;
                }
                builder.Append(":");
                if (!SerializeValue(jsonSerializerStrategy, current, builder))
                {
                    return false;
                }
            }
            builder.Append("}");
            return true;
        }

        private static bool SerializeString(string aString, StringBuilder builder)
        {
            if (aString.IndexOfAny(EscapeCharacters) == -1)
            {
                builder.Append('"');
                builder.Append(aString);
                builder.Append('"');
                return true;
            }
            builder.Append('"');
            int charCount = 0;
            char[] chArray = aString.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                char index = chArray[i];
                if ((index >= EscapeTable.Length) || (EscapeTable[index] == '\0'))
                {
                    charCount++;
                }
                else
                {
                    if (charCount > 0)
                    {
                        builder.Append(chArray, i - charCount, charCount);
                        charCount = 0;
                    }
                    builder.Append('\\');
                    builder.Append(EscapeTable[index]);
                }
            }
            if (charCount > 0)
            {
                builder.Append(chArray, chArray.Length - charCount, charCount);
            }
            builder.Append('"');
            return true;
        }

        private static bool SerializeValue(IJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
        {
            bool flag = true;
            string aString = value as string;
            if (aString != null)
            {
                flag = SerializeString(aString, builder);
            }
            else
            {
                IDictionary<string, object> dictionary = value as IDictionary<string, object>;
                if (dictionary != null)
                {
                    flag = SerializeObject(jsonSerializerStrategy, dictionary.Keys, dictionary.Values, builder);
                }
                else
                {
                    IDictionary<string, string> dictionary2 = value as IDictionary<string, string>;
                    if (dictionary2 != null)
                    {
                        flag = SerializeObject(jsonSerializerStrategy, dictionary2.Keys, dictionary2.Values, builder);
                    }
                    else
                    {
                        IEnumerable anArray = value as IEnumerable;
                        if (anArray != null)
                        {
                            flag = SerializeArray(jsonSerializerStrategy, anArray, builder);
                        }
                        else if (IsNumeric(value))
                        {
                            flag = SerializeNumber(value, builder);
                        }
                        else if (value as bool)
                        {
                            builder.Append(((bool) value) ? "true" : "false");
                        }
                        else if (value == null)
                        {
                            builder.Append("null");
                        }
                        else
                        {
                            object obj2;
                            flag = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj2);
                            if (flag)
                            {
                                SerializeValue(jsonSerializerStrategy, obj2, builder);
                            }
                        }
                    }
                }
            }
            return flag;
        }

        public static bool TryDeserializeObject(string json, out object obj)
        {
            bool success = true;
            if (json == null)
            {
                obj = null;
            }
            else
            {
                char[] chArray = json.ToCharArray();
                int index = 0;
                obj = ParseValue(chArray, ref index, ref success);
            }
            return success;
        }

        public static IJsonSerializerStrategy CurrentJsonSerializerStrategy
        {
            get => 
                (_currentJsonSerializerStrategy ?? (_currentJsonSerializerStrategy = DataContractJsonSerializerStrategy));
            set => 
                (_currentJsonSerializerStrategy = value);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Squirrel.Json.PocoJsonSerializerStrategy PocoJsonSerializerStrategy =>
            (_pocoJsonSerializerStrategy ?? (_pocoJsonSerializerStrategy = new Squirrel.Json.PocoJsonSerializerStrategy()));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Squirrel.Json.DataContractJsonSerializerStrategy DataContractJsonSerializerStrategy =>
            (_dataContractJsonSerializerStrategy ?? (_dataContractJsonSerializerStrategy = new Squirrel.Json.DataContractJsonSerializerStrategy()));
    }
}

