using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BasicMusicBot.Models;
using BasicMusicBot.Extensions;
using BasicMusicBot.Services;
using BasicMusicBot.Settings;
using Serilog;
using Spectre.Console;
using System.Runtime.InteropServices;

namespace BasicMusicBot;

public class Program
{
    static bool _exitSystem = false;
    static BotService _botService;
    static YoutubeQueueService _queueService;


    static async Task Main(string[] args)
    {
        _handler += new EventHandler(Handler);
        SetConsoleCtrlHandler(_handler, true);

        if (args.Contains("-start"))
            await new Program().StartBot();

        AnsiConsole.Write(
            new FigletText("BasicMusicBot")
                .LeftJustified()
                .Color(Color.Red));

        var response = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Options:")
                .PageSize(5)
                .AddChoices(new[] {
                    "Start", "Test", "Quit",
                }));

        switch (response)
        {
            case "Start":
                await new Program().StartBot();
                break;
            case "Test":
                await new Program().TestBot();
                break;
            case "Quit":
                Environment.Exit(0);
                break;
        }
    }


    public async Task StartBot()
    {
        var discordConfig = new DiscordSocketConfig()
        {
            LogLevel = Discord.LogSeverity.Info
        };

        var botSettingsConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetSection("BotSettings").Get<BotSettings>();

        if (botSettingsConfig == null)
            throw new ApplicationException("Cannot find appsettings.json config");

        var services = new ServiceCollection()
            .AddSingleton(discordConfig)
            .AddSingleton(botSettingsConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<BotService>()
            .AddSingleton<YoutubeQueueService>()
            .BuildServiceProvider();

        _botService = services.GetRequiredService<BotService>();
        _queueService = services.GetRequiredService<YoutubeQueueService>();
        await _botService.StartAsync();
        await _queueService.StartAsync();
        await Task.Delay(-1);
    }

    public async Task TestBot()
    {
        Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Information()
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();

        var botSettingsConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetSection("BotSettings").Get<BotSettings>();

        var youtubeQueueItem = new YoutubeQueueItem
        {
            VideoId = "GvfwUfbs82w",
            Url = "https://www.youtube.com/watch?v=GvfwUfbs82w",
            CancellationToken = new CancellationTokenSource()
        };


        await youtubeQueueItem.DownloadYoutubeAudio(botSettingsConfig);

        await Task.Delay(-1);
    }


    #region Trap application termination
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);
    static EventHandler _handler;

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool Handler(CtrlType sig)
    {
        Task.Factory.StartNew(async () => 
        {
            if(_botService != null)
                await _botService.StopAsync();
            if(_queueService != null)
                await _queueService.StopAsync();
        });

        Thread.Sleep(1000); //some delay to read console

        //allow main to run off
        _exitSystem = true;

        //shutdown right away so there are no lingering threads
        Environment.Exit(-1);

        return true;
    }
    #endregion
}