# Project Mika AI 🌸

**An Anime AI Companion with Voice Interaction and Intelligent Responses**

<div align="center">
  <img src="Screenshot 2025-07-24 233533.png" alt="Mika AI Companion" width="400">
  <p><em>Meet Mika - Your Anime AI Companion</em></p>
</div>

Mika is an interactive anime AI companion built with Unity that combines advanced speech processing, artificial intelligence, and real-time avatar animation to create an engaging conversational experience. Whether you want to chat, ask questions, or simply interact with a responsive AI personality, Mika brings your anime companion dreams to life.

## ✨ Features

### 🎤 **MVP v1 - Currently Implemented**
- **Voice Input & Output**: Real-time speech-to-text and text-to-speech conversion using Google services *(Audio input only)*
- **Audio Processing**: High-quality voice processing with waifu voice synthesis
- **Unity Integration**: Seamless audio and animation command handling with synchronized visual responses
- **Real-time Response**: Fallback animation system for immediate user feedback
- **Microphone Recording**: Built-in voice capture functionality
- **Expression Control**: Dynamic facial expressions and lip sync synchronized with audio
- **Emotional Intelligence**: LLM-powered emotional reaction inference that determines appropriate animations, facial expressions, and durations based on conversation context
- **Multi-language Support**: Supports English, Spanish, and Japanese languages
- **LLM Integration**: Advanced AI conversations using Gemini Flash 2.5 and Flash Lite 2.5
- **Long-term Memory**: Conversation memory that persists for approximately 250 days without manual pruning

### 🚧 **Next Steps (Post-MVP)**
- **Time & Date Awareness**: Mika will be aware of current day, time, and up-to-date information
- **Perplexity-style Workflow**: Real-time information retrieval and processing
- **Animation System Fixes**: Resolve Unity C# animation issues
- **Distribution Ready**: Package as .exe or installation wizard for user consumption

### 🔮 **Future Versions (Not in MVP v1)**
- **Multi-modal Input/Output**: Vision processing for images and files *(Not in v1)*
- **Vector Database**: Advanced semantic search capabilities *(Not in v1)*
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
- **AI**: Gemini Flash 2.5 and Flash Lite 2.5 for conversation processing
- **Memory**: Long-term conversation memory (up to ~250 days without pruning)

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
2. **AI Conversations**: Mika features advanced AI conversations powered by Gemini models with long-term memory
3. **Multi-language**: Interact in English, Spanish, or Japanese
4. **Synchronized Responses**: Enjoy perfectly timed animations and expressions that match Mika's voice responses
5. **Extended Memory**: Mika remembers your conversations for approximately 250 days without needing memory management

## 🎯 Development Status

| Feature | Status | Notes |
|---------|--------|-------|
| **MVP v1 Features** | | |
| Speech-to-Text | ✅ Implemented | Google Enhanced STT (Audio input only) |
| Text-to-Speech | ✅ Implemented | Waifu voice synthesis |
| Audio Processing | ✅ Implemented | WAV/MP3 support |
| Unity Integration | ✅ Implemented | Animation & audio commands with sync |
| Fallback Responses | ✅ Implemented | Immediate user feedback |
| LLM Integration | ✅ Implemented | Gemini Flash 2.5 & Flash Lite 2.5 |
| Emotional Intelligence | ✅ Implemented | LLM-powered emotion inference |
| Long-term Memory | ✅ Implemented | ~250 days conversation retention |
| Multi-language Support | ✅ Implemented | English, Spanish, Japanese |
| **Next Steps** | | |
| Time/Date Awareness | 🔄 Next Priority | Current day, time, up-to-date info |
| Perplexity Workflow | 🔄 Next Priority | Real-time information processing |
| Unity Animation Fixes | 🔄 Next Priority | C# animation system improvements |
| Distribution Package | 🔄 Next Priority | .exe/installer for users |
| **Future Features** | | |
| Multi-modal Input/Output | ❌ Not in v1 | Images, files (future version) |
| Vector Database | ❌ Not in v1 | Advanced semantic search |
| URL Parsing | ❌ Not in v1 | Web content processing |
| Plugin System | 🔄 Future | Extensible functionality |
| Advanced Web Search | 🔄 Future | Real-time information with citations |
| Vision Processing | 🔄 Future | Image and file analysis |

## 🤝 Contributing

We welcome contributions to make Mika even better! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Areas We Need Help With

- 🕐 **Time/Date Integration**: Implementing real-time awareness and up-to-date information
- 🎨 **Unity Animation System**: Fixing C# animation issues and improving synchronization
- 📦 **Distribution**: Creating .exe installers and user-friendly packaging
- 🌐 **Perplexity Workflow**: Real-time information retrieval and processing
- 🔮 **Future Features**: Multi-modal support, vector databases, web parsing (post-MVP)
- 🛡️ **Safety & Moderation**: Content filtering and user protection systems

## 📋 Roadmap

### Phase 1: MVP v1 Foundation (Current - Completed)
- [x] Voice input/output (audio only)
- [x] Unity integration with synchronized animations
- [x] Audio processing pipeline with emotional intelligence
- [x] LLM integration for conversations
- [x] Long-term memory (250+ days)
- [x] Multi-language support (EN/ES/JP)

### Phase 2: MVP v1 Polish (Next - In Progress)
- [ ] Time and date awareness
- [ ] Up-to-date information processing (Perplexity-style workflow)
- [ ] Unity C# animation system fixes
- [ ] Distribution packaging (.exe/installer)

### Phase 3: Enhanced Features (Future)
- [ ] Multi-modal input (images, files, text)
- [ ] Vector database integration
- [ ] URL parsing and web content processing
- [ ] Advanced web search with citations
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