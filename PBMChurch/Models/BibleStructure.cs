namespace PBMChurch.Models
{
    public class BibleBook
    {
        public string Name { get; set; }
        public int Chapters { get; set; }
        public int StartChapter { get; set; }
        public int EndChapter { get; set; }
    }

    public static class BibleStructure
    {
        public static List<BibleBook> Books = new List<BibleBook>
        {
            new BibleBook { Name = "Genesis", Chapters = 50, StartChapter = 1, EndChapter = 50 },
            new BibleBook { Name = "Exodus", Chapters = 40, StartChapter = 51, EndChapter = 90 },
            new BibleBook { Name = "Leviticus", Chapters = 27, StartChapter = 91, EndChapter = 117 },
            new BibleBook { Name = "Numbers", Chapters = 36, StartChapter = 118, EndChapter = 153 },
            new BibleBook { Name = "Deuteronomy", Chapters = 34, StartChapter = 154, EndChapter = 187 },
            new BibleBook { Name = "Joshua", Chapters = 24, StartChapter = 188, EndChapter = 211 },
            new BibleBook { Name = "Judges", Chapters = 21, StartChapter = 212, EndChapter = 232 },
            new BibleBook { Name = "Ruth", Chapters = 4, StartChapter = 233, EndChapter = 236 },
            new BibleBook { Name = "1 Samuel", Chapters = 31, StartChapter = 237, EndChapter = 267 },
            new BibleBook { Name = "2 Samuel", Chapters = 24, StartChapter = 268, EndChapter = 291 },
            new BibleBook { Name = "1 Kings", Chapters = 22, StartChapter = 292, EndChapter = 313 },
            new BibleBook { Name = "2 Kings", Chapters = 25, StartChapter = 314, EndChapter = 338 },
            new BibleBook { Name = "1 Chronicles", Chapters = 29, StartChapter = 339, EndChapter = 367 },
            new BibleBook { Name = "2 Chronicles", Chapters = 36, StartChapter = 368, EndChapter = 403 },
            new BibleBook { Name = "Ezra", Chapters = 10, StartChapter = 404, EndChapter = 413 },
            new BibleBook { Name = "Nehemiah", Chapters = 13, StartChapter = 414, EndChapter = 426 },
            new BibleBook { Name = "Esther", Chapters = 10, StartChapter = 427, EndChapter = 436 },
            new BibleBook { Name = "Job", Chapters = 42, StartChapter = 437, EndChapter = 478 },
            new BibleBook { Name = "Psalms", Chapters = 150, StartChapter = 479, EndChapter = 628 },
            new BibleBook { Name = "Proverbs", Chapters = 31, StartChapter = 629, EndChapter = 659 },
            new BibleBook { Name = "Ecclesiastes", Chapters = 12, StartChapter = 660, EndChapter = 671 },
            new BibleBook { Name = "Song of Songs", Chapters = 8, StartChapter = 672, EndChapter = 679 },
            new BibleBook { Name = "Isaiah", Chapters = 66, StartChapter = 680, EndChapter = 745 },
            new BibleBook { Name = "Jeremiah", Chapters = 52, StartChapter = 746, EndChapter = 797 },
            new BibleBook { Name = "Lamentations", Chapters = 5, StartChapter = 798, EndChapter = 802 },
            new BibleBook { Name = "Ezekiel", Chapters = 48, StartChapter = 803, EndChapter = 850 },
            new BibleBook { Name = "Daniel", Chapters = 12, StartChapter = 851, EndChapter = 862 },
            new BibleBook { Name = "Hosea", Chapters = 14, StartChapter = 863, EndChapter = 876 },
            new BibleBook { Name = "Joel", Chapters = 3, StartChapter = 877, EndChapter = 879 },
            new BibleBook { Name = "Amos", Chapters = 9, StartChapter = 880, EndChapter = 888 },
            new BibleBook { Name = "Obadiah", Chapters = 1, StartChapter = 889, EndChapter = 889 },
            new BibleBook { Name = "Jonah", Chapters = 4, StartChapter = 890, EndChapter = 893 },
            new BibleBook { Name = "Micah", Chapters = 7, StartChapter = 894, EndChapter = 900 },
            new BibleBook { Name = "Nahum", Chapters = 3, StartChapter = 901, EndChapter = 903 },
            new BibleBook { Name = "Habakkuk", Chapters = 3, StartChapter = 904, EndChapter = 906 },
            new BibleBook { Name = "Zephaniah", Chapters = 3, StartChapter = 907, EndChapter = 909 },
            new BibleBook { Name = "Haggai", Chapters = 2, StartChapter = 910, EndChapter = 911 },
            new BibleBook { Name = "Zechariah", Chapters = 14, StartChapter = 912, EndChapter = 925 },
            new BibleBook { Name = "Malachi", Chapters = 4, StartChapter = 926, EndChapter = 929 },
            new BibleBook { Name = "Matthew", Chapters = 28, StartChapter = 930, EndChapter = 957 },
            new BibleBook { Name = "Mark", Chapters = 16, StartChapter = 958, EndChapter = 973 },
            new BibleBook { Name = "Luke", Chapters = 24, StartChapter = 974, EndChapter = 997 },
            new BibleBook { Name = "John", Chapters = 21, StartChapter = 998, EndChapter = 1018 },
            new BibleBook { Name = "Acts", Chapters = 28, StartChapter = 1019, EndChapter = 1046 },
            new BibleBook { Name = "Romans", Chapters = 16, StartChapter = 1047, EndChapter = 1062 },
            new BibleBook { Name = "1 Corinthians", Chapters = 16, StartChapter = 1063, EndChapter = 1078 },
            new BibleBook { Name = "2 Corinthians", Chapters = 13, StartChapter = 1079, EndChapter = 1091 },
            new BibleBook { Name = "Galatians", Chapters = 6, StartChapter = 1092, EndChapter = 1097 },
            new BibleBook { Name = "Ephesians", Chapters = 6, StartChapter = 1098, EndChapter = 1103 },
            new BibleBook { Name = "Philippians", Chapters = 4, StartChapter = 1104, EndChapter = 1107 },
            new BibleBook { Name = "Colossians", Chapters = 4, StartChapter = 1108, EndChapter = 1111 },
            new BibleBook { Name = "1 Thessalonians", Chapters = 5, StartChapter = 1112, EndChapter = 1116 },
            new BibleBook { Name = "2 Thessalonians", Chapters = 3, StartChapter = 1117, EndChapter = 1119 },
            new BibleBook { Name = "1 Timothy", Chapters = 6, StartChapter = 1120, EndChapter = 1125 },
            new BibleBook { Name = "2 Timothy", Chapters = 4, StartChapter = 1126, EndChapter = 1129 },
            new BibleBook { Name = "Titus", Chapters = 3, StartChapter = 1130, EndChapter = 1132 },
            new BibleBook { Name = "Philemon", Chapters = 1, StartChapter = 1133, EndChapter = 1133 },
            new BibleBook { Name = "Hebrews", Chapters = 13, StartChapter = 1134, EndChapter = 1146 },
            new BibleBook { Name = "James", Chapters = 5, StartChapter = 1147, EndChapter = 1151 },
            new BibleBook { Name = "1 Peter", Chapters = 5, StartChapter = 1152, EndChapter = 1156 },
            new BibleBook { Name = "2 Peter", Chapters = 3, StartChapter = 1157, EndChapter = 1159 },
            new BibleBook { Name = "1 John", Chapters = 5, StartChapter = 1160, EndChapter = 1164 },
            new BibleBook { Name = "2 John", Chapters = 1, StartChapter = 1165, EndChapter = 1165 },
            new BibleBook { Name = "3 John", Chapters = 1, StartChapter = 1166, EndChapter = 1166 },
            new BibleBook { Name = "Jude", Chapters = 1, StartChapter = 1167, EndChapter = 1167 },
            new BibleBook { Name = "Revelation", Chapters = 22, StartChapter = 1168, EndChapter = 1189 }
        };

        public static string GetReadingRange(int startChapter, int endChapter)
        {
            var ranges = new List<string>();
            var currentChapter = startChapter;

            while (currentChapter <= endChapter)
            {
                var book = Books.FirstOrDefault(b => currentChapter >= b.StartChapter && currentChapter <= b.EndChapter);
                if (book != null)
                {
                    var bookStartChapter = currentChapter - book.StartChapter + 1;
                    var remainingInBook = book.EndChapter - currentChapter + 1;
                    var chaptersToRead = Math.Min(remainingInBook, endChapter - currentChapter + 1);
                    var bookEndChapter = bookStartChapter + chaptersToRead - 1;

                    if (bookStartChapter == bookEndChapter)
                    {
                        ranges.Add($"{book.Name} {bookStartChapter}");
                    }
                    else
                    {
                        ranges.Add($"{book.Name} {bookStartChapter}-{bookEndChapter}");
                    }

                    currentChapter += chaptersToRead;
                }
                else
                {
                    currentChapter++;
                }
            }

            return string.Join(", ", ranges);
        }
    }
}