using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CoinFlips.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;

namespace CoinFlips.Commands
{
    [Command("coinflip")]
    [CommandDescription("lists active coinflips or start coinflip")]
    [CommandAlias("cf")]
    [CommandSyntax("[page | player]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CoinFlipCommand : UnturnedCommand
    {
        private readonly ILogger<CoinFlipCommand> _logger;
        private readonly ICoinFlipManager _coinFlipManager;
        private readonly IUnturnedUserDirectory _userDirectory;
        private readonly IUserManager _userManager;
        private readonly IStringLocalizer _localizer;

        public CoinFlipCommand(IServiceProvider serviceProvider, 
            ILogger<CoinFlipCommand> logger, 
            ICoinFlipManager coinFlipManager, 
            IUnturnedUserDirectory userDirectory, 
            IUserManager userManager, IStringLocalizer localizer) : base(
            serviceProvider)
        {
            _logger = logger;
            _coinFlipManager = coinFlipManager;
            _userDirectory = userDirectory;
            _userManager = userManager;
            _localizer = localizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var uUser = (UnturnedUser)Context.Actor;

            var raw = Context.Parameters.Length > 0
                ? Context.Parameters[0].Trim()
                : string.Empty;
            
            if (raw.Equals("r") || raw.Equals("remove"))
            {
                await _coinFlipManager.RemoveCoinFlip(uUser.Player);
                await uUser.PrintMessageAsync(_localizer["remove"], Color.Lime);
                return;
            }

            var user = await FindCFUserAsync(KnownActorTypes.Player, raw, UserSearchMode.FindByNameOrId);

            if (user == null)
            {
                int.TryParse(raw, out var page);
                if (page == 0) page = 1;
                
                var text = await _coinFlipManager.ActiveCoinFlips(page);
                if (string.IsNullOrEmpty(text))
                    throw new UserFriendlyException(_localizer["none_active"]);

                await PrintAsync(_localizer["cf"]);
                await PrintAsync(text);
                await PrintAsync(_localizer["page", new { Page = page }]);
                return;
            }

            _coinFlipManager.AcceptCoinFlip(uUser.Player, user.SteamId).Forget();
        }
        
        public virtual async Task<UnturnedUser?> FindCFUserAsync(string userType, string searchString, UserSearchMode searchMode)
        {
            if(string.IsNullOrEmpty(userType))
            {
                throw new ArgumentException(nameof(userType));
            }

            if (string.IsNullOrEmpty(searchString))
            {
                return null;
            }

            foreach (var userProvider in _userManager.UserProviders.Where(d => d.SupportsUserType(userType)))
            {
                var user = await userProvider.FindUserAsync(userType, searchString, searchMode);
                
                if (user != null)
                {
                    var uUser = user as UnturnedUser;
                    if (uUser == null) continue;

                    if (!_coinFlipManager.CoinFlips.ContainsKey(uUser.SteamId)) continue;
                    
                    return uUser;
                }
            }

            return null;
        }
    }
}