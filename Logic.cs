using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Speech.Recognition;
using SoundpadConnector;
using SoundpadConnector.XML;
using System.IO;
using MoreLinq;
using System.Threading;

//TODO Add sound stop command
//TODO Change button
//TODO Add play last

namespace SoundpadVoiceControl
{
    class Logic
    {
        Soundpad Soundpad;
        MainForm mainform = null;
        List<Sound> soundList;

        private IKeyboardMouseEvents m_GlobalHook;

        readonly MouseButtons startProcessingButton = MouseButtons.XButton1;

        SpeechRecognitionEngine recognizer;
        Grammar pq;
        Grammar bind;
        Grammar unbind;
        Grammar number;
        Grammar exitcancel;
        Grammar restart;

        Sound[] boundSounds = new Sound[50];

        bool processing = false;
        TaskCompletionSource<bool> tcs = null;

        string boundSoundsFilePath = @"C:\Users\Noah\Desktop\boundsounds.txt";

        async public void init()
        {
            await Task.Run(() =>
            {
                while (mainform == null)
                    mainform = (MainForm)Application.OpenForms["MainForm"];

                Console.WriteLine("Starting");

                Soundpad = new Soundpad();
                Soundpad.ConnectAsync();
                soundList = Soundpad.GetSoundlist().Result.Value.Sounds;

                if (File.Exists(boundSoundsFilePath))
                {
                    using (StreamReader sr = File.OpenText(boundSoundsFilePath))
                    {
                        string s;
                        int i = 0;
                        while ((s = sr.ReadLine()) != null)
                        {
                            if (s == "" || EqualityComparer<Sound>.Default.Equals(soundList.Find(x => x.Url == s), default))
                            {
                                i++;
                                continue;
                            }
                            boundSounds[i] = soundList.Find(x => x.Url == s);
                            i++;
                        }
                    }
                }
                ListBoundSounds();

                Choices numbers = new Choices();
                for (int i = 0; i < 50; i++) numbers.Add(i.ToString());

                Choices sounds = new Choices();
                List<string> tempSounds = new List<string>();
                soundList.ForEach(x =>
                {
                    List<string> soundWords = new List<string>();
                    x.Title.Split(' ').ForEach(y =>
                    {
                        string tempSound = "";
                        soundWords.Add(y);
                        soundWords.ForEach(z => tempSound += z + " ");
                        tempSounds.Add(tempSound.Trim());
                    });
                });
                tempSounds.Distinct().ToList().ForEach(x => sounds.Add(x));

                Choices numbersAndSounds = new Choices();
                numbersAndSounds.Add(numbers);
                numbersAndSounds.Add(sounds);

                GrammarBuilder pqB = new GrammarBuilder(new Choices(new string[] { "play", "queue" }));
                pqB.Append(numbersAndSounds);
                pq = new Grammar(pqB);

                GrammarBuilder bindB = new GrammarBuilder("bind");
                bindB.Append(sounds);
                bindB.Append(numbers);
                bind = new Grammar(bindB);

                GrammarBuilder unbindB = new GrammarBuilder("unbind");
                unbindB.Append(numbers);
                unbind = new Grammar(unbindB);

                GrammarBuilder numberB = new GrammarBuilder(numbers);
                number = new Grammar(numberB);

                GrammarBuilder ecB = new GrammarBuilder(new Choices(new string[] { "exit", "cancel" }));
                exitcancel = new Grammar(ecB);

                GrammarBuilder restartB = new GrammarBuilder("restart");
                restart = new Grammar(restartB);

                recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
                recognizer.SetInputToDefaultAudioDevice();
                recognizer.LoadGrammar(exitcancel);

                m_GlobalHook = Hook.GlobalEvents();

                m_GlobalHook.MouseDown += GotMouseDown;
                m_GlobalHook.MouseUp += GotMouseUp;

                Application.Run(new ApplicationContext());
            });
        }

        void GotMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == startProcessingButton && processing == false)
            {
                processing = true;
                Task.Run(() => BeginProcessing());
            }
        }

        void GotMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == startProcessingButton)
            {
                tcs?.TrySetResult(true);
            }
        }

        void BeginProcessing()
        {
            recognizer.UnloadAllGrammars();
            recognizer.LoadGrammar(exitcancel);
            recognizer.LoadGrammar(pq);
            recognizer.LoadGrammar(bind);
            recognizer.LoadGrammar(unbind);
            recognizer.LoadGrammar(number);
            mainform.SetMainLabel(
                "Command Options:\n" +
                "<Number>\n" +
                "Play <Sound | Number>\n" +
                "Queue <Sound | Number>\n" +
                "Bind <Sound> <Number>\n" +
                "Unbind <Number>\n" +
                "Cancel\n" +
                "Exit");
            RecognitionResult reresult = GetRecognize(0);
            List<string> voiceResults = GetVoiceResults(reresult);
            if (voiceResults == null) return;
            string command = reresult.Alternates.First().Words.First().Text.ToLower();
            for (int i = 0; i < voiceResults.Count; i++)
            {
                voiceResults[i] = voiceResults[i].Replace(command, "").Trim();
            }
            List<Sound> soundResults = new List<Sound>();
            if (int.TryParse(command, out int numCommand))
            {
                if (boundSounds[numCommand] == null)
                {
                    Console.WriteLine("Index " + numCommand + " is not bound");
                    mainform.SetErrorLabel("Index " + numCommand + " is not bound\nTrying again");
                    BeginProcessing();
                }
                else
                {
                    Console.WriteLine("Playing");
                    mainform.SetMainLabel("Playing bound sound number " + numCommand + "\nSound title: " + boundSounds[numCommand].Title);
                    Soundpad.PlaySound(boundSounds[numCommand].Index).Wait();
                    FinishProcessing("Played " + boundSounds[numCommand].Title);
                }
            }
            else if (command == "play" || command == "queue")
            {
                if (int.TryParse(voiceResults[0], out int playNum))
                {
                    if (boundSounds[playNum] == null)
                    {
                        Console.WriteLine("Index " + playNum + " is not bound");
                        mainform.SetErrorLabel("Index " + playNum + " is not bound\nTrying again");
                        BeginProcessing();
                    }
                    else
                    {
                        if (command == "play")
                        {
                            Console.WriteLine("Playing");
                            mainform.SetMainLabel("Playing bound sound number " + playNum + "\nSound title: " + boundSounds[playNum].Title);
                        }
                        else
                        {
                            Console.WriteLine("Queued");
                            mainform.SetMainLabel("Queued bound sound number " + playNum + "\nSound title: " + boundSounds[playNum].Title);
                            tcs = new TaskCompletionSource<bool>();
                            tcs.Task.Wait();
                            tcs = null;
                        }
                        Soundpad.PlaySound(boundSounds[playNum].Index).Wait();
                        FinishProcessing("Played " + boundSounds[playNum].Title);
                    }
                }
                soundResults = GetMatchingSounds(voiceResults);
                if (soundResults.Count == 1)
                {
                    Console.WriteLine(soundResults[0].Title);
                    if (command == "play")
                    {
                        mainform.SetMainLabel("Playing: " + soundResults[0].Title);
                    }
                    else
                    {
                        mainform.SetMainLabel("Queued: " + soundResults[0].Title);
                        tcs = new TaskCompletionSource<bool>();
                        tcs.Task.Wait();
                        tcs = null;
                    }
                    Soundpad.PlaySound(soundResults[0].Index).Wait();
                    FinishProcessing("Played " + soundResults[0].Title);
                }
                ProcessMultipleResults(soundResults, command);
            }
            else if (command == "bind")
            {
                if (!int.TryParse(reresult.Alternates.First().Words.Last().Text.ToLower(), out int bindNumber))
                {
                    Console.WriteLine("Int Parse failed");
                    mainform.SetErrorLabel("Int Parse failed\nTrying again");
                    BeginProcessing();
                }
                voiceResults.ForEach(x => x = x.Substring(0, x.Length - 2).Trim());
                for (int i = 0; i < voiceResults.Count; i++)
                {
                    voiceResults[i] = voiceResults[i].Replace(bindNumber.ToString(), "").Trim();
                }
                soundResults = GetMatchingSounds(voiceResults);
                if (soundResults.Count == 1)
                {
                    Console.WriteLine("Binding " + soundResults[0].Title + " to " + bindNumber);
                    boundSounds[bindNumber] = soundResults[0];
                    ListBoundSounds();
                    FinishProcessing("Bound " + soundResults[0].Title + " to " + bindNumber);
                }
                ProcessMultipleResults(soundResults, command, bindNumber);
            }
            else if (command == "unbind")
            {
                if (!int.TryParse(reresult.Alternates.First().Words.Last().Text.ToLower(), out int bindNumber))
                {
                    Console.WriteLine("Int Parse failed");
                    mainform.SetErrorLabel("Int Parse failed\nTrying again");
                    BeginProcessing();
                }
                boundSounds[bindNumber] = null;
                ListBoundSounds();
                FinishProcessing("Unbound " + bindNumber);
            }
        }

        void ListBoundSounds()
        {
            string listString = "Bound Sounds:\n";
            Sound s;
            for (int i = 0; i < 50; i++)
            {
                s = boundSounds[i];
                if (s == null)
                {
                    continue;
                }
                listString += i + ": " + s.Title + "\n";
            }
            mainform.SetListLabel(listString);
        }

        List<Sound> GetMatchingSounds(List<string> voiceResults)
        {
            List<Sound> soundResults = new List<Sound>();
            foreach (string voiceResult in voiceResults)
            {
                soundResults.AddRange(soundList.FindAll(x => CalcLevenshteinDistance(voiceResult, x.Title.ToLower()) < 3));
                List<Sound> test = soundList.FindAll(x => (" " + x.Title.ToLower() + " ").Contains(" " + voiceResult + " "));
                soundResults.AddRange(test);
            }
            return soundResults.DistinctBy(x => new { x.Url }).ToList();
        }

        void ProcessMultipleResults(List<Sound> soundResults, string command, int bindNumber = -1)
        {
            recognizer.UnloadAllGrammars();
            recognizer.LoadGrammar(exitcancel);
            recognizer.LoadGrammar(number);
            recognizer.LoadGrammar(restart);

            string labelString = "";
            for (int i = 0; i < soundResults.Count; i++)
                labelString += i + ": " + soundResults[i].Title + " in " +
                    Path.GetFileName(Path.GetDirectoryName(soundResults[i].Url)) + "\n";
            mainform.SetMainLabel("Multiple options: \n" + labelString);

            RecognitionResult reresult = GetRecognize(0);
            List<string> voiceResults = GetVoiceResults(reresult);
            if (voiceResults == null) return;
            if (voiceResults[0].Contains("restart"))
            {
                BeginProcessing();
            }
            String result = voiceResults[0].ToLower();
            Console.WriteLine("Recognized number: " + result);
            if (!int.TryParse(result, out int intResult))
            {
                mainform.SetErrorLabel("Voice to number failed\nTrying again");
                ProcessMultipleResults(soundResults, command, bindNumber);
            }
            try
            {
                Sound soundResult = soundResults[intResult];
                Console.WriteLine(soundResult.Title);
                if (command == "play")
                {
                    mainform.SetMainLabel("Playing: " + soundResult.Title);
                    Soundpad.PlaySound(soundResult.Index).Wait();
                    FinishProcessing("Played " + soundResult.Title);
                }
                else if (command == "queue")
                {
                    mainform.SetMainLabel("Queued: " + soundResult.Title);
                    tcs = new TaskCompletionSource<bool>();
                    tcs.Task.Wait();
                    tcs = null;
                    Soundpad.PlaySound(soundResult.Index).Wait();
                    FinishProcessing("Played " + soundResult.Title);
                }
                else if (command == "bind")
                {
                    Console.WriteLine("Binding " + soundResult.Title + " to " + bindNumber);
                    boundSounds[bindNumber] = soundResult;
                    ListBoundSounds();
                    FinishProcessing("Bound " + soundResult.Title + " to " + bindNumber);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                mainform.SetErrorLabel("Index " + intResult + " is out of range\nTrying again");
                ProcessMultipleResults(soundResults, command, bindNumber);
            }
        }

        void FinishProcessing(string processResult)
        {
            mainform.SetMainLabel(processResult);
            mainform.SetErrorLabel("");
            processing = false;
            Thread.CurrentThread.Abort();
        }

        RecognitionResult GetRecognize(int numPrevAttempts)
        {
            RecognitionResult reresult = recognizer.Recognize();
            mainform.SetErrorLabel("");
            if (reresult == null || reresult.Alternates.Count == 0)
            {
                if (numPrevAttempts >= 4)
                {
                    FinishProcessing("Could not recognize voice input");
                }
                Console.WriteLine("Not recognized");
                mainform.SetErrorLabel("Not recognized\nTry again\n" + (4 - numPrevAttempts) + " attempts remaining");
                reresult = GetRecognize(numPrevAttempts + 1);
            }
            return reresult;
        }

        List<string> GetVoiceResults(RecognitionResult reresult)
        {
            List<string> voiceResults = new List<string>();
            foreach (RecognizedPhrase r in reresult.Alternates)
            {
                string rtext = r.Text.ToLower();
                Console.WriteLine("Recognized text: " + rtext);
                voiceResults.Add(rtext);
            }
            if (voiceResults[0].Contains("exit"))
            {
                using (StreamWriter sw = File.CreateText(boundSoundsFilePath))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        if (boundSounds[i] == null)
                        {
                            sw.WriteLine();
                        }
                        else
                        {
                            sw.WriteLine(boundSounds[i].Url);
                        }
                    }
                }
                mainform.MainForm_FormClosing();
                Unsubscribe();
                Application.Exit();
            }
            if (voiceResults[0].Contains("cancel"))
            {
                FinishProcessing("Operation Cancelled");
            }
            return voiceResults;
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDown -= GotMouseDown;
            m_GlobalHook.MouseUp -= GotMouseUp;

            m_GlobalHook.Dispose();
        }

        private int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }
    }
}
