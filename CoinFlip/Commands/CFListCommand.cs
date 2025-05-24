using System;
using System.Linq;
using CoinFlip.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace CoinFlip.Commands
{
    [Command("list")]
    [CommandAlias("l")]
    [CommandSyntax("[page]")]
    [CommandParent(typeof(CFCommand))]
    public class CFListCommand : UnturnedCommand
    {
        private readonly ICoinFlipManager _coinFlipManager;
        private readonly IStringLocalizer _localizer;
        public CFListCommand(IServiceProvider serviceProvider, ICoinFlipManager coinFlipManager, IStringLocalizer localizer) : base(serviceProvider)
        {
            _coinFlipManager = coinFlipManager;
            _localizer = localizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var flips = await _coinFlipManager.GetAllCoinFlipsAsync();
            if (flips.Count < 1) throw new UserFriendlyException(_localizer["no_coinflips"]);

            var pageNumberRaw = Context.Parameters.Count > 0 ? Context.Parameters[0] : "1";
            var pageNumber = int.Parse(pageNumberRaw);

            var totalPages = (int)Math.Ceiling(flips.Count / (double)5);
            if (totalPages < pageNumber) pageNumber = totalPages;
            
            var page = flips.OrderByDescending(x => x.Amount)
                .Skip((pageNumber - 1) * 5)
                .Take(5)
                .ToList();
            string entries = string.Join("\n", page.Select((flip, index) => $"{index + 1}. {flip.Owner.Player.channel.owner.playerID.characterName} - ${flip.Amount}"));
            var message = $"{entries}\nPage: {pageNumber}/{totalPages}";
            await PrintAsync(message);
        }
    }
}