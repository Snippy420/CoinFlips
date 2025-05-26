using System;
using CoinFlip.Models;
using CoinFlip.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace CoinFlip.Commands
{
    [Command("coinflip")]
    [CommandAlias("cf")]
    [CommandSyntax("<amount> [target]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CFCommand : UnturnedCommand
    {
        private readonly ICoinFlipManager _coinFlipManager;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<CFCommand> _logger;
        
        public int _MinBet;
        public int _MaxBet;
    
        public CFCommand(IServiceProvider serviceProvider, 
            ICoinFlipManager coinFlipManager, 
            IConfiguration configuration, 
            IStringLocalizer localizer, ILogger<CFCommand> logger) : base(serviceProvider)
        {
            _coinFlipManager = coinFlipManager;
            _configuration = configuration;
            _localizer = localizer;
            _logger = logger;

            _MinBet = _configuration.GetValue<int>("min_bet");
            _MaxBet = _configuration.GetValue<int>("max_bet");
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);
            var uUser = (UnturnedUser)Context.Actor;
            var amount = await Context.Parameters.GetAsync<int>(0);
            
            UnturnedUser? target = null;
            if (Context.Parameters.Length > 1) target = await Context.Parameters.GetAsync<UnturnedUser>(1);
            
            if (amount < _MinBet || amount > _MaxBet)
            {
                _logger.LogDebug($"Unable to create CF due to amount ({amount}) being outside range ({_MinBet} - {_MaxBet}).");
                throw new UserFriendlyException(_localizer["errors:outside_bet_range", 
                    new { min = _MinBet.ToString("N0"), max = _MaxBet.ToString("N0") }]);
            }

            CoinFlipInfo? info;
            if (target == null) info = await _coinFlipManager.CreateCoinFlipAsync(uUser.Player, amount, null);
            else info = await _coinFlipManager.CreateCoinFlipAsync(uUser.Player, amount, target.Player);

            if (info == null) throw new UserFriendlyException(_localizer["errors:error"]);
        }
    }
}