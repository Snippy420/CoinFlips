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
    
        public List<CoinFlipInfo> CoinFlips { get; } = new();

        public int _MinBet;
        public int _MaxBet;

        public CoinFlipManager(IConfiguration configuration, IStringLocalizer localizer)
        {
            _configuration = configuration;
            _localizer = localizer;

            _MinBet = _configuration.GetValue<int>("CoinFlip:MinBet");
            _MaxBet = _configuration.GetValue<int>("CoinFlip:MaxBet");
        }
    
        public async UniTask<List<CoinFlipInfo>> GetAllCoinFlipsAsync()
        {
            return CoinFlips.OrderByDescending(flip => flip.Amount).ToList();
        }

        public async UniTask<CoinFlipInfo> CreateCoinFlipAsync(UnturnedPlayer player, int amount, UnturnedPlayer? target = null)
        {
            if (amount < _MinBet || amount < _MaxBet) 
                throw new UserFriendlyException(_localizer["outside_bet_range", 
                    new { min = _MinBet, max = _MaxBet }]);
        
            var cf = new CoinFlipInfo(player, amount);

            if (target != null)
            {
                await target.PrintMessageAsync(_localizer["duel_message",
                        new { player = player.Player.channel.owner.playerID.characterName, amount = amount }], 
                    Color.White, 
                    true, 
                    Provider.configData.Browser.Icon);
            }
        
            CoinFlips.Add(cf);
            return cf;
        }

        public async UniTask DeleteCoinFlipAsync(UnturnedPlayer player)
        {
            var index = CoinFlips.FindIndex(x => x.Owner.SteamId == player.SteamId);
        
            if (index == -1) 
                throw new UserFriendlyException(_localizer["unable_to_find_coinflip", 
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