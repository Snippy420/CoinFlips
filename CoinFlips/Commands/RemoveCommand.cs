using System;
using CoinFlips.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace CoinFlips.Commands;

[Command("remove")]
[CommandDescription("Remove your coinflip")]
[CommandAlias("r")]
[CommandActor(typeof(UnturnedUser))]
public class RemoveCommand : UnturnedCommand
{
    private readonly ILogger<RemoveCommand> _logger;
    private readonly ICoinFlipManager _coinFlipManager;

    public RemoveCommand(IServiceProvider serviceProvider, 
        ILogger<RemoveCommand> logger, 
        ICoinFlipManager coinFlipManager) : base(serviceProvider)
    {
        _logger = logger;
        _coinFlipManager = coinFlipManager;
    }

    protected override async UniTask OnExecuteAsync()
    {
        var uUser = (UnturnedUser)Context.Actor;

        await _coinFlipManager.RemoveCoinFlip(uUser.Player);
    }
}