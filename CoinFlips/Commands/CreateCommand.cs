using System;
using System.Drawing;
using CoinFlips.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace CoinFlips.Commands;

[Command("create")]
[CommandDescription("Create coinflip")]
[CommandAlias("c")]
[CommandParent(typeof(CoinFlipCommand))]
[CommandActor(typeof(UnturnedUser))]
[CommandSyntax("<amount>")]
public class CreateCommand : UnturnedCommand
{
    private readonly ILogger<CreateCommand> _logger;
    private readonly ICoinFlipManager _coinFlipManager;
    private readonly IStringLocalizer _localizer;

    public CreateCommand(IServiceProvider serviceProvider, 
        ILogger<CreateCommand> logger, 
        ICoinFlipManager coinFlipManager, IStringLocalizer localizer) : base(serviceProvider)
    {
        _logger = logger;
        _coinFlipManager = coinFlipManager;
        _localizer = localizer;
    }

    protected override async UniTask OnExecuteAsync()
    {
        var uUser = (UnturnedUser)Context.Actor;

        if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);

        var amount = await Context.Parameters.GetAsync<uint>(0);

        await _coinFlipManager.CreateCoinFlip(uUser.Player, (int)amount);

        await PrintAsync(_localizer["success", new { Amount = amount.ToString("N0") }], Color.Lime);
    }
}