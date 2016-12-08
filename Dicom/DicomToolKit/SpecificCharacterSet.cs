using System;
using System.Collections.Generic;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class CharacterSet
    {
        string code;
        System.Text.Encoding encoding;
        byte[] g0;
        byte[] g1;

        internal CharacterSet(string code, System.Text.Encoding encoding) 
            : this(code, encoding, null, null)
        {
        }

        internal CharacterSet(string code, System.Text.Encoding encoding, byte[] g0, byte[] g1)
        {
            this.code = code;
            this.encoding = encoding;
            this.g0 = g0;
            this.g1 = g1;
        }

        public String CodeString
        {
            get
            {
                return code;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return encoding;
            }
        }

        public byte[] G0
        {
            get
            {
                return g0;
            }
        }

        public byte[] G1
        {
            get
            {
                return g1;
            }
        }

        /*

JIS X 0201
         ISO-IR 13 katakana
         ISO-IR 14 7-bit romaji
JIS X 0208
         ISO-IR 87 kanji (ideographic), hiragana (phonetic), and katakana
JIS X 0212
         ISO-IR 159 kanji (ideographic)

              ISO 2022 IR 6\ISO 2022 IR 87\ISO 2022 IR 100\ISO 2022 IR 159\ISO 2022 IR 13
              ASCII\JIS X 0208\Latin-1\JIS X 0212\JIS X 0201


ISO-2022-JP
    ESC ( B     ASCII
    ESC ( J     JIS X 0201-1976 (ISO/IEC 646:JP) Roman set
    ESC $ @     JIS X 0208-1978
    ESC $ B     JIS X 0208-1983
ISO-2022-JP-1 The same as ISO-2022-JP with one additional escape sequence 
    ESC $ (     DJIS X 0212-1990
ISO-2022-JP-2 The same as ISO-2022-JP-1 with the following additional escape sequences 
    ESC $ A     GB 2312-1980
    ESC $ ( C   KS X 1001-1992 (2 bytes per character)
    ESC - A     ISO/IEC 8859-1 high part, Extended Latin 1 set
    ESC - F     ISO/IEC 8859-7 high part, Basic Greek set
ISO-2022-JP-3 The same as ISO-2022-JP with three additional escape sequences 
    ESC ( I     JIS X 0201-1976 Kana set
    ESC $ ( O   JIS X 0213-2000 Plane 1
    ESC $ ( P   JIS X 0213-2000 Plane 2
ISO-2022-JP-2004 The same as ISO-2022-JP-3 with one additional escape sequence 
    ESC $ ( Q   JIS X 0213-2004 Plane 1



ISO-2022-KR 
    ESC $ ) C   KS X 1001-1992 previously named KS C 5601-1987
ISO-2022-CN
    ESC $ ( A   GB 2312-1980
    ESC $ ( G   CNS 11643-1992 Plane 1
    ESC $ ( H   CNS 11643-1992 Plane 2
ISO-2022-CN-EXT The same as ISO-2022-CN
    ESC $ ( E   ISO-IR-165
    ESC $ ( I   CNS 11643-1992 Plane 3
    ESC $ ( J   CNS 11643-1992 Plane 4
    ESC $ ( K   CNS 11643-1992 Plane 5
    ESC $ ( L   CNS 11643-1992 Plane 6
    ESC $ ( M   CNS 11643-1992 Plane 7

    */
        public static CharacterSet Create(string code)
        {
            CharacterSet value = null;
            switch (code)
            {
                case "GB18030":             // Chinese
                    value = new CharacterSet(code, Encoding.GetEncoding("GB18030"));
                    break;
                //case "ISO_IR 13":         // Japanese
                //    value = new CharacterSet(code, Encoding.GetEncoding("iso-2022-jp"));
                //    break;
                //case "ISO 2022 IR 13":         // Japanese
                //    value = new CharacterSet(code, Encoding.GetEncoding("iso-2022-jp"), new byte[] {0x28, 0x4a}, new byte[] {0x29, 0x49});
                //    break;
                case "ISO 2022 IR 87":      // Japanese
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-2022-jp"), new byte[] { 0x24, 0x42 }, new byte[] { 0x24, 0x42 });
                    break;
                case "ISO_IR 100":          // Latin-1
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-1"));
                    break;
                case "ISO 2022 IR 100":     // Latin-1
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-1"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x41});
                    break;
                case "ISO_IR 101":          // Latin-2
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-2"));
                    break;
                case "ISO 2022 IR 101":     // Latin-2
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-2"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x42});
                    break;
                case "ISO_IR 109":          // Latin-3
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-3"));
                    break;
                case "ISO 2022 IR 109":     // Latin-3
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-3"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x43});
                    break;
                case "ISO_IR 110":          // Latin-4
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-4"));
                    break;
                case "ISO 2022 IR 110":     // Latin-4
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-4"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x44});
                    break;
                case "ISO_IR 126":          // Greek
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-7"));
                    break;
                case "ISO 2022 IR 126":     // Greek
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-7"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x46});
                    break;
                case "ISO_IR 127":          // Arabic
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-6"));
                    break;
                case "ISO 2022 IR 127":     // Arabic
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-6"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x47});
                    break;
                case "ISO_IR 138":          // Hebrew
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-8"));
                    break;
                case "ISO 2022 IR 138":     // Hebrew
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-8"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x48});
                    break;
                case "ISO_IR 144":          // Cyrillic
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-5"));
                    break;
                case "ISO 2022 IR 144":     // Cyrillic
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-5"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x4c});
                    break;
                case "ISO_IR 148":          // Latin-5
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-9"));
                    break;
                case "ISO 2022 IR 148":     // Latin-5
                    value = new CharacterSet(code, Encoding.GetEncoding("iso-8859-9"), new byte[] {0x28, 0x42}, new byte[] {0x2d, 0x4d});
                    break;
                case "ISO_IR 149":          // Korean
                    value = new CharacterSet(code, Encoding.GetEncoding("ks_c_5601-1987"));
                    break;
                case "ISO 2022 IR 149":     // Korean
                    value = new CharacterSet(code, Encoding.GetEncoding("EUC-KR"), null, new byte[] { 0x24, 0x29, 0x43 });
                    //value = new CharacterSet(code, Encoding.GetEncoding("ks_c_5601-1987"), null, new byte[] { 0x24, 0x29, 0x43 });
                    break;
                case "ISO 2022 IR 159":     // Japanese
                    value = new CharacterSet(code, Encoding.GetEncoding("EUC-JP"), null, null);
                    break;
                case "ISO_IR 166":          // Thai
                    value = new CharacterSet(code, Encoding.GetEncoding("windows-874"));
                    break;
                //case "ISO 2022 IR 166":     // Thai
                //    value = new CharacterSet(code, Encoding.GetEncoding("windows-874"), new byte[] { 0x28, 0x42 }, new byte[] { 0x2d, 0x54 });
                //    break;
                case "ISO_IR 192":          // UTF-8
                    value = new CharacterSet(code, Encoding.GetEncoding("UTF-8"));
                    break;
                case "ISO_IR 6":            //  ASCII
                    value = new CharacterSet(code, Encoding.GetEncoding("us-ascii"));
                    break;
                case "ISO 2022 IR 6":       // ASCII
                    value = new CharacterSet(code, Encoding.GetEncoding("us-ascii"), new byte[] {0x28, 0x42}, null);
                    break;
                case "":
                default:
                    value = new CharacterSet(code, new DefaultEncoding());
                    break;
            }
            return value;
        }

        public string Key
        {
            get
            {
                return GetKey(g1);
            }
        }

        public static string GetKey(byte[] bytes)
        {
            if (bytes == null)
                return "2842";
            string key = String.Empty;
            foreach (byte b in bytes)
            {
                key += String.Format("{0:x}", b);
            }
            return key;
        }

    }

    public class SpecificCharacterSet
    {
        List<CharacterSet> encodings;

        private static SpecificCharacterSet @default = new SpecificCharacterSet();
        public static SpecificCharacterSet Default
        {
            get
            {
                return @default;
            }
        }

        public SpecificCharacterSet()
        {
            parse("");
        }

        public SpecificCharacterSet(object value)
        {
            parse(value);
        }
        
        protected void parse(object value)
        {
            encodings = new List<CharacterSet>();

            if (!(value is string[]) && !(value is string) && value != null)
                throw new ArgumentException("Expecting a null, string, or string[].", "value");

            string[] sets = value as string[];
            if (value == null)
            {
                sets = new string[1];
                sets[0] = String.Empty;
            }
            else if (value is string)
            {
                sets = new string[1];
                sets[0] = value as String;
            }
            // treat "ISO 2022 IR 6\ISO 2022 IR 87" and "\ISO 2022 IR 87" as just "ISO 2022 IR 87"
            else if (sets.Length == 2 && (sets[0] == "ISO_IR 6" || sets[0] == "ISO 2022 IR 6" || sets[0] == String.Empty) && sets[1] == "ISO 2022 IR 87")
            {
                sets = new string[1];
                sets[0] = "ISO 2022 IR 87";
            }
            // treat "ISO 2022 IR 149" as "ISO 2022 IR 6\ISO 2022 IR 149"
            else if (sets.Length == 1 && (sets[0] == "ISO_IR 149" || sets[0] == "ISO 2022 IR 149"))
            {
                sets = new string[2];
                sets[0] = "ISO 2022 IR 6";
                sets[1] = "ISO 2022 IR 149";
            }
            else
            {
                // currently multiple character sets are not fully supported
                if (sets.Length > 1 && (sets[0] != "ISO_IR 6" && sets[0] != "ISO 2022 IR 6" && sets[0] != String.Empty))
                {
                    sets = new string[1];
                    sets[0] = "";
                }
            }

            if (sets.Length == 1)
            {
                encodings.Add(CharacterSet.Create(sets[0]));
            }
            else
            {
                foreach (string set in sets)
                {
                    // TODO there are certain rules here for what is legal when vm > 1
                    // 6.1.2.5.4 Levels of Implementation and Initial Designation, part 5
                    if (set == String.Empty)
                    {
                        encodings.Add(CharacterSet.Create("ISO 2022 IR 6"));
                    }
                    else
                    {
                        // coerce character sets to ISO 2022
                        CharacterSet temp = CharacterSet.Create(set.Replace("_IR", " 2022 IR"));
                        encodings.Add(temp);
                    }
                }
            }
        }

        public CharacterSet FindCharacterSet(string key)
        {
            foreach (CharacterSet set in encodings)
            {
                if (key == set.Key)
                    return set;
            }
            throw new Exception(String.Format("Key {0} not found.", key));
        }

        /// <summary>
        /// Returns a set of characters that signal a switching of active character set.
        /// </summary>
        /// <param name="vr"></param>
        /// <returns></returns>
        /// <remarks>The use of code extensions has certain requirements as to which character set is active.  Typically at the start of a new
        /// line or page of text, the first, or default, character set is active and an escape sequence needs to be invoked to switch context
        /// to another character set regardless what was active before the page break.</remarks>
        private List<byte> GetTokens(string vr)
        {
            List<byte> tokens = new List<byte>();
            if(vr == "PN")
            {
                // before the first use of the character set in the Data Element value
                // before the first use of the character set in the name component and name component group in
                tokens = new List<byte>(new byte[] { (byte)'\\', (byte)'^', (byte)'=' });
            }
            else if(vr == "SH")
            {
                // before the first use of the character set in the Data Element value
                // before the first use of the character set in the line
                // before the first use of the character set in the page
                tokens = new List<byte>(new byte[] { (byte)'\\', (byte)'\n', (byte)'\f' });
            }
            return tokens;
        }

        public string GetString(byte[] bytes, String vr)
        {
            if (encodings.Count == 1)
            {
                return encodings[0].Encoding.GetString(bytes);
            }
            else
            {
                StringBuilder text = new StringBuilder();

                Encoding encoding = encodings[0].Encoding;
                List<byte> tokens = GetTokens(vr);
                byte[] fragment;
                int n, start;
                for (n = 0, start = 0; n < bytes.Length; n++)
                {
                    byte b = bytes[n];
                    if (tokens.Contains(b) || b == 0x1b)
                    {
                        if (n - start > 0)
                        {
                            fragment = new byte[n - start];
                            Buffer.BlockCopy(bytes, start, fragment, 0, n - start);
                            text.Append(encoding.GetString(fragment));
                        }
                        if (b == 0x1b)
                        {
                            byte[] key;
                            if ((n + 3 < bytes.Length-1) && bytes[n + 1] == 0x24 && bytes[n + 2] == 0x29 && bytes[n + 3] == 0x43)
                            {
                                key = new byte[] { bytes[++n], bytes[++n], bytes[++n] };
                            }
                            else if (n + 2 < bytes.Length - 1)
                            {
                                key = new byte[] { bytes[++n], bytes[++n] };
                            }
                            else
                            {
                                n = start;
                                break;
                            }
                            string temp = CharacterSet.GetKey(key);
                            encoding = FindCharacterSet(temp).Encoding;
                        }
                        else
                        {
                            // TODO if we are using Japanese maybe '\' must be '? as per spec
                            text.Append((char)b);
                            encoding = encodings[0].Encoding;
                        }
                        start = n+1;
                    }
                }
                if (n - start > 0)
                {
                    fragment = new byte[n - start];
                    Buffer.BlockCopy(bytes, start, fragment, 0, n - start);
                    text.Append(encoding.GetString(fragment));
                }
                return text.ToString();
            }
        }

        public byte[] GetBytes(string text, String vr)
        {
            if (encodings.Count == 1)
            {
                return encodings[0].Encoding.GetBytes(text);
            }
            else
            {
                CharacterSet set = encodings[0];

                List<byte> result = new List<byte>();
                List<byte> tokens = GetTokens(vr);
                byte[] fragment = null;
                foreach(char c in text)
                {
                    if (tokens.Contains((byte)c))
                    {
                        result.Add((byte)c);
                        set = encodings[0];
                        continue;
                    }
                    fragment = set.Encoding.GetBytes(new String(c, 1));
                    if (fragment.Length == 1 && fragment[0] == (byte)'?')
                    {
                        set = FindCharacterSet(c);
                        result.Add(0x1b);
                        foreach (byte e in set.G1)
                        {
                            result.Add(e);
                        }
                        fragment = set.Encoding.GetBytes(new String(c, 1));
                    }
                    foreach(byte b in fragment)
                    {
                        result.Add(b);
                    }
                }
                return result.ToArray();
            }
        }

        private CharacterSet FindCharacterSet(char c)
        {
            foreach(CharacterSet set in encodings)
            {
                byte [] temp = set.Encoding.GetBytes(new String(c,1));
                if (temp[0] != (byte)'?')
                {
                    return set;
                }
            }
            return encodings[0];
        }

        public int GetByteCount(string text, String vr)
        {
            if (encodings.Count == 1)
            {
                return encodings[0].Encoding.GetByteCount(text);
            }
            else
            {
                return GetBytes(text, vr).Length;
            }
        }

    }

    public class DefaultEncoding : ASCIIEncoding
    {
        private char encode = '\x00d7'; // multiplication sign

        public override string GetString(byte[] bytes)
        {
            StringBuilder text = new StringBuilder();
            foreach (byte b in bytes)
            {
                if (b > 0x7f)           // high ascii
                {
                    text.Append(String.Format("\x00d7{0:x2}", b));
                }
                else if (b == 0x1b)     // escape
                {
                    text.Append("\x00d71b");
                }
                else
                {
                    text.Append((char)b);
                }
            }
            return text.ToString();
        }

        public override byte[] GetBytes(string text)
        {
            byte[] bytes = new byte[GetByteCount(text)];
            GetBytes(text.ToCharArray(), 0, text.Length, bytes, 0);
            return bytes;
        }

        public override int GetByteCount(string text)
        {
            return GetByteCount(text.ToCharArray(), 0, text.Length);
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            int unknowns = 0;
            int length = index + count;
            for(int n = index; n < length; n++)
            {
                if (chars[n] == encode)
                {
                    unknowns++;
                }
            }
            return count - unknowns * 2;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int count = charIndex + charCount;
            for (int n = charIndex; n < count; n++)
            {
                char c = chars[n];
                if (c == encode)
                {
                    bytes[byteIndex++] = (byte)System.Convert.ToInt16(String.Format("{0}{1}", chars[n + 1], chars[n + 2]), 16);
                    n = n + 2;
                }
                else
                {
                    bytes[byteIndex++] = (byte)c;
                }
            }
            return charCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount*4;
        }
    }
}
