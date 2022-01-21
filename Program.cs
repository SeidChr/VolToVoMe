using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VolToVoMe.Interop;

var host = Host.CreateDefaultBuilder().Build();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

var events = new VolumeEvents();
var voiceMeterClient = new VoicemeeterClient();

void HandleVolume(float volume, bool muted)
{
    var cleanVolume = System.Math.Round(volume, 2);

    var db = cleanVolume > 0.8
        // 0.8..1 -> 0..12
        ? (cleanVolume - 0.8) * 60
        // 0..0.8 -> -60..0
        : (cleanVolume * 75) - 60;

    var cleanDb = (float)Math.Round(db, 1);

    Console.WriteLine($"{cleanVolume} {cleanDb}");
    voiceMeterClient.SetParameter("Bus[0].Gain", cleanDb);
    voiceMeterClient.SetParameter("Bus[0].Mute", muted ? 1 : 0);
}

lifetime.ApplicationStarted.Register(() => {
    events.VolumeChanged += HandleVolume;
    HandleVolume(events.Volume, events.Muted);
});

lifetime.ApplicationStopping.Register(() =>
    voiceMeterClient.Dispose());

await host.RunAsync();