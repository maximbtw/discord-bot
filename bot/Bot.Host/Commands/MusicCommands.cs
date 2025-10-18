using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Bot.Application.Infrastructure.Checks.Access;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

namespace Bot.Host.Commands;

internal class MusicCommands : DiscordCommandsGroupBase<MusicCommands>
{
    private const string MusicFilePath = @"E:\programming-projects\discrod-bot\bot\test.mp3";

    public MusicCommands(ILogger<MusicCommands> logger) : base(logger)
    {
    }


    [Command("test")]
    [Description("Plays music")]
    [RoleCheck(Role.SuperUser)]
    [DirectMessageUsage(DirectMessageUsage.DenyDMs)]
    public async Task Play(CommandContext context)
    {
        // await context.DeferResponseAsync();

        ulong? channelId = context.Member!.VoiceState.ChannelId;
        if (channelId is null)
        {
            await context.RespondAsync("You are not in a voice channel.");

            return;
        }

        DiscordChannel channel = await context.Guild!.GetChannelAsync(channelId.Value);

        VoiceNextConnection connection = await channel.ConnectAsync();
        VoiceTransmitSink transmit = connection.GetTransmitSink();

        Stream pcm = ConvertAudioToPcm(MusicFilePath);
        await pcm.CopyToAsync(transmit);
        await pcm.DisposeAsync();
    }

    private Stream ConvertAudioToPcm(string filePath)
    {
        Process? ffmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        return ffmpeg!.StandardOutput.BaseStream;
    }
}