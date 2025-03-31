using Audio.AI;

Console.WriteLine("Audio Transcriber is loading...");

string mp3FileName = @"C:\Users\princ\source\repos\dotnet\Audio.AI\Audio.AI\bbc.mp3";
(string t, string s) = AudioTranscriptionProcessor.TranscribeAndSummariseAudio(mp3FileName).Result;

Console.WriteLine($"Transcribed Audio:\n {t}");
Console.WriteLine($"Summary:\n {s}");