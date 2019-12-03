using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.Speech;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
using System.IO;
using System.Media;
using System.Diagnostics;
using System.Configuration;
using System.Text.Encodings.Web;
using System.Threading;

namespace SpeechRecognition
{
    public class SpeechAsistant
    {
        private MainWindow window;
        private CultureInfo culture;
        private SpeechRecognitionEngine speech;
        private GrammarLibrary grammarLibrary;
        private bool inListening = true;
        private int delay = 10000;

        private Dictionary<(string, int), Process> processes;
        private int procID = 0;

        private Timer timer;

        public SpeechAsistant(MainWindow window)
        {
            this.window = window;

            processes = new Dictionary<(string, int), Process>();
            grammarLibrary = new GrammarLibrary(window);

            culture = new CultureInfo("ru-RU");
            speech = new SpeechRecognitionEngine(culture);

            this.ReloadGrammar();
            speech.SetInputToDefaultAudioDevice();
            speech.SpeechRecognized += OperationSpeechRecognized;
        }

        public void Reload() => this.ReloadGrammar();

        private void ReloadGrammar()
        {
            speech.UnloadAllGrammars();

            speech.LoadGrammarAsync(grammarLibrary.StartProgramGrammar());
            speech.LoadGrammarAsync(grammarLibrary.StopProgramGrammar());
            speech.LoadGrammarAsync(grammarLibrary.CancelingGrammar());
            speech.LoadGrammarAsync(grammarLibrary.ListenGrammar());
            
            inListening = true;
        }

        public void StartListening() => speech.RecognizeAsync(RecognizeMode.Multiple);
        public void StopListening() => speech.RecognizeAsyncStop();

        private void StartDelay()
        {
            if (timer != null)
                timer.Dispose();

            timer = new Timer((count) => {

                var audio = Properties.Resources.mistake;
                new SoundPlayer(audio).Play();

                inListening = true;

                timer.Dispose();
            }, null, delay, 0);
        }

        private void OperationSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < window.Confidence)
                return;

            foreach (var s in e.Result.Semantics)
            {
                var value = s.Value.Value as string;

                if (s.Key == "listen")
                {
                    if (!inListening) { break; }

                    var audio = Properties.Resources.listen;
                    new SoundPlayer(audio).Play();

                    inListening = false;
                }

                if (s.Key == "cancel")
                {
                    if (inListening) { break; }

                    var audio = Properties.Resources.mistake;
                    new SoundPlayer(audio).Play();

                    inListening = true;

                    if (timer != null)
                        timer.Dispose();
                }

                if (s.Key == "start")
                {
                    if (inListening) { break; }

                    try 
                    {
                        var process = Process.Start(new ProcessStartInfo()
                        { 
                            FileName = value,
                            UseShellExecute = true
                        });;

                        processes.Add((value, procID), process);
                        procID++;

                    } 
                    catch
                    {
                        var audio = Properties.Resources.error;
                        new SoundPlayer(audio).Play();
                    }

                    this.StartDelay();
                }

                if (s.Key == "close")
                {
                    if (inListening) { break; }

                    if (!value.Contains("https://"))
                    {
                        for (int i = 0; i < procID; i++)
                        {
                            try 
                            { 
                                processes[(value, i)].CloseMainWindow();
                                processes.Remove((value, i));
                            } 
                            catch (Exception exception)
                            {
                                if (exception.Message != 
                                    "The given key was not present in the dictionary.")
                                {
                                    var audio = Properties.Resources.error;
                                    new SoundPlayer(audio).Play();
                                }
                            }
                        }
                    }

                    this.StartDelay();
                }
            }
        }
    }
}
