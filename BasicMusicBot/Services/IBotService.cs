namespace BasicMusicBot.Services
{
    public interface IBotService : IDisposable
    {
        public Task StartAsync();

        public Task StopAsync();
    }
}
