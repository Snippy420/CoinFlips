using System.Threading.Tasks;
using CoinFlips.Services;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Connections.Events;
using OpenMod.Unturned.Users.Events;

namespace CoinFlips.EventListeners;

public class UserDisconnectedEventListener : IEventListener<UnturnedPlayerDisconnectedEvent>
{
    public readonly ICoinFlipManager _coinFlipManager;

    public UserDisconnectedEventListener(ICoinFlipManager coinFlipManager)
    {
        _coinFlipManager = coinFlipManager;
    }

    public async Task HandleEventAsync(object? sender, UnturnedPlayerDisconnectedEvent @event)
    {
        if (!_coinFlipManager.CoinFlips.ContainsKey(@event.Player.SteamId)) return;

        await _coinFlipManager.RemoveCoinFlip(@event.Player);
    }
}