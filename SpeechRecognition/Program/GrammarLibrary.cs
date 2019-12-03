using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechRecognition
{
    class GrammarLibrary
    {
        MainWindow window;
        CultureInfo culture = new CultureInfo("ru-RU");

        public GrammarLibrary(MainWindow window)
        {
            this.window = window;
        }

        public Grammar CancelingGrammar()
        {
            Choices choises = new Choices();

            choises.Add($"отмена {window.NickName}");
            choises.Add($"{window.NickName} отмена");

            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = culture;
            grammarBuilder.Append(new SemanticResultKey("cancel", choises));

            return new Grammar(grammarBuilder);
        }

        public Grammar ListenGrammar()
        {
            Choices choises = new Choices();

            choises.Add($"слушай {window.NickName}");
            choises.Add($"{window.NickName} слушай");

            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = culture;
            grammarBuilder.Append(new SemanticResultKey("listen", choises));

            return new Grammar(grammarBuilder);
        }

        private Choices DirectoriesChoices()
        {
            Choices choices = new Choices();
            choices.Add(new SemanticResultValue("блокнот", "notepad"));

            var dictonary = window.GetDictonary() 
                as ObservableCollection<NickNamePairs>;

            foreach (var item in dictonary)
            {
                try
                {
                    choices.Add(
                        new SemanticResultValue(item.NickName, item.Path));
                }
                catch { }
            }

            return choices;
        }

        public Grammar StartProgramGrammar()
        {
            string[] variants =
            {
                "запусти", "открой", "найди"
            };

            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = culture;
            grammarBuilder.Append(new Choices(variants));
            grammarBuilder.Append(
                new SemanticResultKey("start", DirectoriesChoices()));

            return new Grammar(grammarBuilder);
        }

        public Grammar StopProgramGrammar()
        {
            string[] variants =
            {
                "закрой", "останови"
            };

            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = culture;
            grammarBuilder.Append(new Choices(variants));
            grammarBuilder.Append(
                new SemanticResultKey("close", DirectoriesChoices()));

            return new Grammar(grammarBuilder);
        }

        #region [Outdated] : НЕ НУЖНО
        /*
        private Choices RussianWordsChoices()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var files = assembly.GetManifestResourceNames()
                .Where(str => str.EndsWith(".txt"));

            Choices choices = new Choices();

            List<string> wordsList = new List<string>();

            foreach (var item in files)
            {
                using (Stream stream = assembly.GetManifestResourceStream(item))
                using (StreamReader reader = new StreamReader(stream, Encoding.Default))
                {
                    wordsList.AddRange(LoadWords(reader.ReadToEnd()));
                }
            }

            choices.Add(wordsList.ToArray());

            return choices;
        }

        static IEnumerable<string> LoadWords(string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                    where !string.IsNullOrEmpty(m.Value)
                    select TrimSuffix(m.Value);

            return words;
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }

        public Grammar FullGrammar()
        {
            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = culture;
            grammarBuilder.Append(RussianWordsChoices());

            return new Grammar(grammarBuilder);
        }
        */
        #endregion
    }
}
