using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoinFlip.Services;
using Cysharp.Threading.Tasks;
using MoreLinq.Extensions;
using OpenMod.Unturned.Players;
using CoinFlip.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;

namespace CoinFlip.Managers
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class CoinFlipManager : ICoinFlipManager
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<CoinFlipManager> _logger;
    
        public List<CoinFlipInfo> CoinFlips { get; } = new();

        public CoinFlipManager(IConfiguration configuration, 
            IStringLocalizer localizer, 
            ILogger<CoinFlipManager> logger)
        {
            _configuration = configuration;
            _localizer = localizer;
            _logger = logger;
        }
    
        public async UniTask<List<CoinFlipInfo>> GetAllCoinFlipsAsync()
        {
            return CoinFlips.OrderByDescending(flip => flip.Amount).ToList();
        }

        public async UniTask<CoinFlipInfo?> CreateCoinFlipAsync(UnturnedPlayer player, int amount, UnturnedPlayer? target = null)
        {
            if (player.SteamId == target?.SteamId) return null;
            
            var cf = new CoinFlipInfo(player, amount);

            await player.PrintMessageAsync(_localizer["info:success_message",
                    new { amount = amount }], 
                Color.White, 
                true, 
                Provider.configData.Browser.Icon);
            
            if (target != null)
            {
                await target.PrintMessageAsync(_localizer["info:duel_message",
                        new { player = player.Player.channel.owner.playerID.characterName, amount = amount }], 
                    Color.White, 
                    true, 
                    Provider.configData.Browser.Icon);
                return cf;
            }
            CoinFlips.Add(cf);
            return cf;
        }

        public async UniTask DeleteCoinFlipAsync(UnturnedPlayer player)
        {
            var index = CoinFlips.FindIndex(x => x.Owner.SteamId == player.SteamId);
        
            if (index == -1) 
                throw new UserFriendlyException(_localizer["errors:unable_to_find_coinflip", 
                    new {steamid = player.SteamId}]);
        
            CoinFlips.RemoveAt(index);
        }

        public async UniTask JoinCoinFlipAsync(UnturnedPlayer player)
        {
        
        }

        public async UniTask JoinCoinFlipAsync(int rank)
        {
        
        }

        public async UniTask AcceptCoinFlipAsync(UnturnedPlayer player)
        {
        
        }
    }
}