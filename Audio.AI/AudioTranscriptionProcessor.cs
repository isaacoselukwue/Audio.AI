using LLama;
using LLama.Common;
using LLama.Sampling;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace Audio.AI;

class AudioTranscriptionProcessor
{
    public static async Task TranscribeAudioFile()
    {
        // We declare three variables which we will use later, ggmlType, modelFileName and mp3FileName
        var ggmlType = GgmlType.Base;
        var modelFileName = "ggml-base.bin"; //base model - there are other models available btw
        var mp3FileName = @"C:\Users\princ\source\repos\dotnet\Audio.AI\Audio.AI\bbc.mp3";

        // This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        // This section creates the whisperFactory object which is used to create the processor object.
        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

        // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        // This section opens the mp3 file and converts it to a wav file with 16Khz sample rate.
        using var fileStream = File.OpenRead(mp3FileName);

        using var wavStream = new MemoryStream();

        using var reader = new Mp3FileReader(fileStream);
        var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
        WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

        // This section sets the wavStream to the beginning of the stream. (This is required because the wavStream was written to in the previous section)
        wavStream.Seek(0, SeekOrigin.Begin);

        // This section processes the audio file and prints the results (start time, end time and transcribed text) to the console.
        await foreach (var result in processor.ProcessAsync(wavStream))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }

    private static async Task<string> TranscribeAudio(string audioFilePath)
    {
        var ggmlType = GgmlType.Base;
        var modelFileName = "ggml-base.bin";

        // Download model if it doesn't exist
        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        // Initialize Whisper
        using var whisperFactory = WhisperFactory.FromPath(modelFileName);
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        // Process audio file
        using var fileStream = File.OpenRead(audioFilePath);
        using var wavStream = new MemoryStream();
        using var reader = new Mp3FileReader(fileStream);
        var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
        WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());
        wavStream.Seek(0, SeekOrigin.Begin);

        // Collect all segments
        var sb = new StringBuilder();
        await foreach (var result in processor.ProcessAsync(wavStream))
        {
            sb.AppendLine(result.Text);
            Console.WriteLine(result.Text);
        }

        return sb.ToString().Trim();
    }

    private static async Task<string> SummariseText(string text)
    {
        Console.WriteLine("Summarising model with phi-4");
        // Path to the LLM model tried with both for my pc and work fine (no gpu required)
        string modelPath = @"C:\Users\princ\Downloads\gemma-3-12b-it-Q2_K.gguf";
        // string modelPath = @"C:\Users\princ\Downloads\phi-2.Q4_K_M.gguf";
        // Initialize model parameters
        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 0, //if you have a gpu
            BatchSize = 64,
            UseMemoryLock = true,
            UseMemorymap = true
        };

        // Create model and context
        using var model = await LLamaWeights.LoadFromFileAsync(parameters);
        using var context = model.CreateContext(parameters);
        var executor = new InteractiveExecutor(context);

        string prompt = @"You are a helpful assistant that creates concise summaries.

Task: Create a brief summary of the following audio transcript. Don't include any questions, multiple choice options, or exercises. Just write a clear, factual summary of the main points from the transcript.

Transcript:
""" + text + @"""

Summary:";

        // Generate the summary
        InferenceParams inferenceParams = new()
        {
            MaxTokens = 512,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.1f,
                TopP = 0.9f
            }
        };

        Console.WriteLine("Generating summary...");
        StringBuilder result = new();
        await foreach (var msg in executor.InferAsync(prompt, inferenceParams))
        {
            result.Append(msg);
            //if you want to see the progress you can add a console log here
        }
        return result.ToString();
    }

    public static async Task<(string Transcription, string Summary)> TranscribeAndSummariseAudio(string audioFilePath)
    {
        // Step 1: Transcribe the audio using Whisper.net
        string transcription = await TranscribeAudio(audioFilePath);

        // Step 2: Summarise the transcription using LLamaSharp just to point out we could still use Msft.AI.Ollama as well. Here you would only need a gguf file.
        string summary = await SummariseText(transcription);

        return (transcription, summary);
    }
}