# Project Mika AI 🌸

**An Anime AI Companion with Voice Interaction and Intelligent Responses**

<div align="center">
  <img src="Screenshot 2025-07-24 233533.png" alt="Mika AI Companion" width="400">
  <p><em>Meet Mika - Your Anime AI Companion</em></p>
</div>

Mika is an interactive anime AI companion built with Unity that combines advanced speech processing, artificial intelligence, and real-time avatar animation to create an engaging conversational experience. Whether you want to chat, ask questions, or simply interact with a responsive AI personality, Mika brings your anime companion dreams to life.

### ✨ Recent Technical Improvements
- **Google Vertex AI Integration**: Migrated from hardcoded API URLs to official Google Vertex AI SDK for Python
- **Hybrid RAG Architecture**: Combines fuzzy search on facts table with semantic vector database (Faiss) for intelligent context retrieval
- **Optimized Memory Management**: 12,000 token context window with 200-message fast cache for enhanced performance
- **Background Processing**: Fact extraction, semantic embedding, and classification run in separate threads for responsiveness

## ✨ Features

### 🎤 **MVP v1 - Currently Implemented**
- **Voice Input & Output**: Real-time speech-to-text and text-to-speech conversion using Google services *(Audio input only)*
- **Audio Processing**: High-quality voice processing with waifu voice synthesis
- **Unity Integration**: Seamless audio and animation command handling with synchronized visual responses
- **Real-time Response**: Fallback animation system for immediate user feedback
- **Microphone Recording**: Built-in voice capture functionality
- **Expression Control**: Dynamic facial expressions and lip sync synchronized with audio - **Unity C# animation system fully implemented**
- **Emotional Intelligence**: LLM-powered emotional reaction inference that determines appropriate animations, facial expressions, and durations based on conversation context
- **Multi-language Support**: Supports English, Spanish, and Japanese languages
- **LLM Integration**: Advanced AI conversations using Gemini Flash 2.5 and Flash Lite 2.5
- **Enhanced Memory System**: Supports up to 12K tokens per request with cache for 200 most recent messages, featuring hybrid RAG approach with fuzzy search and semantic vector database
- **Time & Date Awareness**: Mika is aware of the current day, date, and tracks when the user last sent a message

### 🚧 **Next Steps (Post-MVP)**
- **Distribution Ready**: Package as .exe or installation wizard for user consumption
- **Enhanced Web Search**: Improved real-time information retrieval and processing capabilities *(Future consideration, not prioritized for v1)*

### 🔮 **Future Versions (Not in MVP v1)**
- **Multi-modal Input/Output**: Vision processing for images and files *(Not in v1)*
- **URL Parsing**: Web content processing *(Not in v1)*
- **Plugin System**: Extensible tool and API integration
- **Enhanced Web Search**: Real-time information retrieval with citations
- **Advanced AI Persona**: Character trait evolution and mood coordination
- **Safety & Moderation**: Content filtering and user protection

## 🏗️ Architecture Overview

Mika's architecture is designed as a comprehensive AI pipeline that processes various input types and generates appropriate responses through multiple channels:

```mermaid
flowchart TD
    Start["Start"] --> UserInput["User inputs prompt, files, images, or voice"]
    UserInput --> Preprocess["Input preprocessing: normalize text, file, image, or voice"]

    Preprocess --> IsVoice{"Is input voice?"}
    IsVoice -- Yes --> STT["Speech-to-Text (STT) Service (Google Enhanced, implemented, multi-language)"]
    STT --> TranscribedText["Transcribed Text (implemented, multi-language)"]
    IsVoice -- No --> TranscribedText

    TranscribedText --> ParrotCheck{"Have LLM/persona/roleplay?\n(NOW: Parrot recognized text, LATER: Use LLM/Persona)"}
    ParrotCheck -- No, Parrot --> ParrotLogic["Send recognized text to TTS (Google, implemented, multi-language)"]
    ParrotCheck -- Yes, Use LLM --> NeedsLiveInfo{"Prompt requires live info?"}

    %% Emotional intelligence injection step (NEW)
    subgraph EmotionIntelligence["Emotional Intelligence / Reaction Inference"]
        direction LR
        EmotionLLM["LLM determines emotional reaction: animation, facial expression, duration (implemented)"]
        EmotionLLM --> AnimCmdOut["Output: animation command"]
        EmotionLLM --> ExprCmdOut["Output: expression command"]
        EmotionLLM --> ExprDurOut["Output: expression duration"]
    end

    %% Fallback logic starts here
    subgraph FallbackParallel["Fallback Path (for voice input)"]
        direction LR
        AwaitInput["(After user input)"] --> RandomDelay["Random delay (1-3s)"]
        RandomDelay --> SendFallbackAnim["Send fallback animation & expression"]
        SendFallbackAnim --> SendFallbackAudio["Send fallback audio (pre-generated, multi-language)"]
        SendFallbackAudio --> WaitRealAnswer["Wait for real response pipeline"]
    end

    ParrotLogic -.->|Parallel| FallbackParallel
    NeedsLiveInfo -.->|Parallel| FallbackParallel

    %% Continue regular logic
    ParrotLogic --> TTS["TTS Service (Waifu Voice, Google, implemented, multi-language)"]
    TTS --> AudioFile["Audio File (.wav/.mp3) (implemented, multi-language)"]
    AudioFile --> ReturnAudio["Return audio file to Unity (implemented)"]
    ReturnAudio --> Store["Store message/response, analytics, feedback loop, persona update (to do)"]
    Store --> End["End"]

    NeedsLiveInfo -- Yes --> WebSearch["Perform real-time web search"]
    WebSearch --> ParseWeb["Parse web results, extract relevant content"]
    ParseWeb --> CiteInject["Inject citations and sources into context"]
    NeedsLiveInfo -- No --> SkipWeb["Skip web search"]
    CiteInject --> RetrieveChunks["Retrieve relevant chunks: vector DB, semantic/lexical search"]
    SkipWeb --> RetrieveChunks

    RetrieveChunks --> NeedsPlugin{"Prompt requires plugins/tools?"}
    NeedsPlugin -- Yes --> PluginCall["Call plugin/API/tool, execute code"]
    PluginCall --> PluginResp["Retrieve plugin/tool response"]
    NeedsPlugin -- No --> SkipPlugin["Skip plugin/tool execution"]
    PluginResp --> JoinContext["Join all context sources"]
    SkipPlugin --> JoinContext

    JoinContext --> HasImage{"Check for image/file input"}
    HasImage -- Yes --> Vision["Send to vision model/OCR, extract info"]
    Vision --> TokenizeExtracted["Tokenize extracted info and add to context"]
    HasImage -- No --> Proceed["Proceed"]
    TokenizeExtracted --> SessionAssembly["Session/context window assembly"]
    Proceed --> SessionAssembly

    SessionAssembly --> Persona["Persona/Character State Update (Mika memory, mood, traits) (to do)"]
    Persona --> Moderation["Safety/moderation checks (to do)"]
    Moderation --> Tokens["Token calculation and truncation/summarization (to do)"]
    Tokens --> TokenLimit{"Token limit exceeded? (to do)"}
    TokenLimit -- Yes --> Summarize["Summarize, further truncate, or reroute (to do)"]
    TokenLimit -- No --> ModelSelect["Select best LLM/vision model (to do)"]
    Summarize --> ModelSelect

    ModelSelect --> LLM["LLM generation: produce answer/code (Flash Lite, persona, roleplay, to do, multi-language)"]
    LLM --> RespType["Response Type Selector (text/voice/animation/image) (to do)"]

    RespType --> NeedsVoice{"Is voice response needed? (to do)"}
    NeedsVoice -- Yes --> TTS
    NeedsVoice -- No --> SkipTTS["Skip TTS (to do)"]

    RespType --> NeedsAnim{"Is animation/emotion/gesture needed? (to do)"}
    NeedsAnim -- Yes --> AnimCmd["Generate Avatar Command (Unity triggers) (to do)"]
    NeedsAnim -- No --> SkipAnim["Skip animation command (to do)"]

    AnimCmd --> ReturnAnim["Return animation command to Unity (to do)"]
    SkipAnim --> ReturnAnim

    LLM --> PostProc["Post-processing: formatting, markdown, citations, code blocks (to do)"]
    PostProc --> ReturnText["Return AI text response to Unity (to do)"]

    ReturnText --> Store
    SkipTTS --> ReturnAudio

    %% Emotional intelligence step injection points
    TranscribedText --> EmotionLLM
    EmotionLLM --> AnimCmdOut
    EmotionLLM --> ExprCmdOut
    EmotionLLM --> ExprDurOut
    AnimCmdOut -.-> ReturnAnim
    ExprCmdOut -.-> ReturnAnim
    ExprDurOut -.-> ReturnAnim

    %% Comments:
    %% - STT, ParrotLogic, TTS, ReturnAudio are implemented
    %% - LLM, persona, RAG, plugins, websearch, and multimodal outputs are planned
    %% - Fallback logic is now implemented and runs in parallel as soon as possible after user input
    %% - Emotional intelligence/reaction inference is implemented and called after text is available
    %% - All speech/text/LLM/emotion steps support dynamic language selection (English, Spanish, Japanese)
```

## 🎭 Animation & Expression System

Mika's animation and expression system works through a sophisticated WebSocket-based communication pipeline that coordinates audio playback with visual responses:

```mermaid
sequenceDiagram
    participant User
    participant NativeWebSocketExample
    participant AnimationController
    participant ExpressionController
    participant AudioPlayer

    User->>NativeWebSocketExample: Sends WebSocket message (audio or command)
    NativeWebSocketExample->>NativeWebSocketExample: If "audio:" message, set nextAudioType
    NativeWebSocketExample->>NativeWebSocketExample: If audio bytes, enqueue (audio, type)
    NativeWebSocketExample->>NativeWebSocketExample: If command, store as pending
    NativeWebSocketExample->>AudioPlayer: PlayAudioCoroutine(audio, type)
    AudioPlayer->>NativeWebSocketExample: On audio end
    alt audioType == "fallback"
        NativeWebSocketExample->>AnimationController: Play "Thinking"
        NativeWebSocketExample->>ExpressionController: Set "blink"
        NativeWebSocketExample->>NativeWebSocketExample: Clear pending commands
    else audioType == "response"
        NativeWebSocketExample->>AnimationController: Play pendingAnimation
        NativeWebSocketExample->>ExpressionController: Set pendingExpression
        NativeWebSocketExample->>NativeWebSocketExample: Clear pending commands
    end
```

This system ensures that Mika's visual responses (animations and facial expressions) are perfectly synchronized with her audio responses, providing a natural and engaging interaction experience.

## 🛠️ Technical Stack

- **Frontend**: Unity 3D with C# scripting
- **Speech Processing**: Google Enhanced Speech-to-Text and Text-to-Speech services
- **Audio**: WAV/MP3 processing with real-time lip sync
- **Networking**: WebSocket communication for real-time interactions
- **AI**: Google Vertex AI SDK with Gemini Flash 2.5 and Flash Lite 2.5 for conversation processing
- **Memory System**: Hybrid RAG approach with fuzzy search (facts table) and semantic search (Faiss vector database)
- **Context Management**: Up to 12K tokens per request with 200-message cache for optimal performance

## 🗂️ Project Structure

```
project-mika-ai/
├── Scripts/                          # Unity C# scripts
│   ├── AnimationCommandReceiver.cs   # Handles avatar animations
│   ├── AudioCommandReceiver.cs       # Processes audio commands
│   ├── MicRecorder.cs                # Microphone input handling
│   ├── MikaExpressionController.cs   # Facial expression control
│   ├── SimpleLipSync.cs              # Lip synchronization
│   ├── WavUtility.cs                 # Audio file processing
│   └── NativeWebSocketExample.cs     # WebSocket communication & animation coordination
├── mika_func.md                      # Detailed technical workflow documentation
└── README.md                         # This file
```

For detailed technical documentation about Mika's internal workflow and architecture, see [mika_func.md](mika_func.md).

## 🚀 Getting Started

### Prerequisites

- Unity 2021.3 LTS or later
- Microphone access for voice input
- Internet connection for speech services

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/osmarbetancourt/project-mika-ai.git
   cd project-mika-ai
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" and select the project folder
   - Open the project with Unity 2021.3 LTS or later

3. **Configure Audio Settings**
   - Ensure microphone permissions are granted
   - Test audio input/output in Unity's Audio settings

4. **Run the Project**
   - Open the main scene
   - Press Play to start interacting with Mika

## 💬 How to Interact with Mika

1. **Voice Input Only**: Speak into your microphone - Mika will transcribe and respond *(MVP v1 supports audio input only)*
2. **AI Conversations**: Mika features advanced AI conversations powered by Google Vertex AI with intelligent memory management
3. **Multi-language**: Interact in English, Spanish, or Japanese
4. **Synchronized Responses**: Enjoy perfectly timed animations and expressions that match Mika's voice responses with the fully implemented Unity C# animation system
5. **Intelligent Memory**: Mika uses hybrid RAG with fuzzy search and semantic vectors to remember relevant context from up to 200 recent messages
6. **Time Awareness**: Mika knows what day it is and tracks when you last sent a message, providing contextually relevant responses

## 🎯 Development Status

| Feature | Status | Notes |
|---------|--------|-------|
| **MVP v1 Features** | | |
| Speech-to-Text | ✅ Implemented | Google Enhanced STT (Audio input only) |
| Text-to-Speech | ✅ Implemented | Waifu voice synthesis |
| Audio Processing | ✅ Implemented | WAV/MP3 support |
| Unity Integration | ✅ Implemented | Animation & audio commands with sync |
| Fallback Responses | ✅ Implemented | Immediate user feedback |
| LLM Integration | ✅ Implemented | Google Vertex AI SDK with Gemini models |
| Emotional Intelligence | ✅ Implemented | LLM-powered emotion inference |
| Hybrid Memory System | ✅ Implemented | 12K tokens, 200-message cache, fuzzy + semantic search |
| Multi-language Support | ✅ Implemented | English, Spanish, Japanese |
| Time/Date Awareness | ✅ Implemented | Current day, time, last message timestamp tracking |
| Unity Animation System | ✅ Implemented | C# animation and expression system fixed |
| **Next Steps** | | |
| Distribution Package | 🔄 Next Priority | .exe/installer for users |
| Enhanced Web Search | 🔄 Future Consideration | Real-time information processing *(not v1 priority)* |
| **Future Features** | | |
| Multi-modal Input/Output | ❌ Not in v1 | Images, files (future version) |
| URL Parsing | ❌ Not in v1 | Web content processing |
| Plugin System | 🔄 Future | Extensible functionality |
| Advanced Web Search | 🔄 Future | Real-time information with citations *(deprioritized for v1)* |
| Vision Processing | 🔄 Future | Image and file analysis |

## 🤝 Contributing

We welcome contributions to make Mika even better! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

## 📋 Roadmap

### Phase 1: MVP v1 Foundation (Current - Completed)
- [x] Voice input/output (audio only)
- [x] Unity integration with synchronized animations
- [x] Audio processing pipeline with emotional intelligence
- [x] LLM integration with Google Vertex AI SDK
- [x] Hybrid memory system (12K tokens, 200-message cache)
- [x] RAG implementation (fuzzy search + semantic vector database)
- [x] Multi-language support (EN/ES/JP)
- [x] Time and date awareness with message timestamp tracking
- [x] Unity C# animation and expression system implementation

### Phase 2: MVP v1 Polish (Next - In Progress)
- [ ] Distribution packaging (.exe/installer)
- [ ] Performance optimization and stability improvements

### Phase 3: Enhanced Features (Future)
- [ ] Multi-modal input (images, files, text)
- [ ] URL parsing and web content processing
- [ ] Enhanced web search with real-time information *(reconsidering priority)*
- [ ] Plugin ecosystem and API integrations

### Phase 4: Advanced AI & Polish (Future)
- [ ] Advanced persona system with trait evolution
- [ ] Mobile platform support
- [ ] Performance optimization
- [ ] Enhanced safety and moderation systems

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Google Cloud Speech Services for STT/TTS capabilities
- Unity Technologies for the development platform
- The open-source community for inspiration and support

## 📞 Contact

For questions, suggestions, or collaboration opportunities, please open an issue on GitHub or reach out to the development team.

---

**Ready to meet Mika? Star this repository and join the journey of creating the ultimate anime AI companion! 🌟**