# Discord Bot

A custom Discord bot built for my guild — tailored to our needs, featuring AI-powered chat, steam gaming updates, and other small commands.

## Features

- **Chat with AI**: Integrated AI [OpenRouter](https://openrouter.ai/) for initiating chat with a bot with several working modes.
- **Upcoming Game Alerts**: Notifies about new multiplayer games released on Steam.

## Getting Started

### Prerequisites
- Discord Bot Token ([Discord Developer Portal](https://discord.com/developers/applications))
- OpenRouter Token ([OpenRouter API](https://openrouter.ai/)) (also support openAI)

### Configuration
Must specify API keys in appsettings.json

```json
"BotConfiguration": {
  "Token": "<YOUR_DISCORD_BOT_TOKEN>",
  "Prefix": "/",
  "DatabaseOptions": {
    "UseInMemoryDatabase": true,
    "ConnectionString": ""
  }
},
"OpenAiSettings": {
  "ApiKey": "<YOUR_OPENROUTER_API_KEY>",
  "UseOpenRouter": true,
  "Model": "meta-llama/llama-3.3-70b-instruct:free"
}
```

## Start

1. Clone:

   ```bash
   git clone https://github.com/maximbtw/discord-bot.git
   cd discord-bot
   
2. Build:

   ```bash
   docker build -t bot -f Bot.Host/Dockerfile ../../discrod-bot && docker run --rm bot
