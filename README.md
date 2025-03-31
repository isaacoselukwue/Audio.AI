# Audio.AI

## Overview
Audio.AI is a .NET application that transcribes audio files and generates summaries of the transcribed content. It uses local models for both transcription and summarisation, eliminating the need for cloud API services.

## Features
- Transcribe MP3 audio files to text using Whisper.net
- Generate concise summaries of transcribed content using local LLMs
- Automatic model downloading when required
- Fully offline operation - no API keys or internet connection needed after model download

## Dependencies
- **Whisper.net** - For audio transcription
- **NAudio** - For audio file manipulation
- **LLamaSharp** - For interfacing with local LLM models
- **Local LLM models** - The application uses local GGUF models:
  - Whisper base model for transcription
  - Gemma/Phi model for summarisation

## Setup
1. Clone the repository
2. Ensure you have .NET 6.0+ installed
3. Download a compatible GGUF model for summarization:
   - The code is configured to use: `C:\Users\princ\Downloads\gemma-3-12b-it-Q2_K.gguf`
   - Alternatively, you can use Phi-2 by uncommenting that line in the code
4. The Whisper model will be downloaded automatically on first run

## Usage
The application can be used as follows:

```csharp
// Specify the path to an MP3 file
string mp3FileName = "path/to/your/audio.mp3";

// Process the audio file and get both transcription and summary
(string transcription, string summary) = await AudioTranscriptionProcessor.TranscribeAndSummariseAudio(mp3FileName);

// Use the results
Console.WriteLine($"Transcribed Audio:\n {transcription}");
Console.WriteLine($"Summary:\n {summary}");
```

## Configuration
- The transcription uses Whisper's base model by default
- Summarization uses a local LLM model
  - Default path: `C:\Users\princ\Downloads\gemma-3-12b-it-Q2_K.gguf`
  - Model parameters can be adjusted in the `SummariseText` method

## Notes
- First-time execution will download the Whisper model (~142MB)
- LLM inference is performed on CPU by default
- For GPU acceleration, adjust the `GpuLayerCount` parameter in the code

## License
MIT License

Copyright (c) 2025

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.