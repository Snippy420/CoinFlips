using System.Collections.Generic;
using CoinFlip.Models;
using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players;

namespace CoinFlip.Services
{
    [Service]
    public interface ICoinFlipManager
    {
        public List<CoinFlipInfo> CoinFlips { get; }
    
        public UniTask<List<CoinFlipInfo>> GetAllCoinFlipsAsync();
        public UniTask<CoinFlipInfo?> CreateCoinFlipAsync(UnturnedPlayer player, int amount, UnturnedPlayer? target = null);
        public UniTask DeleteCoinFlipAsync(UnturnedPlayer player);
        public UniTask JoinCoinFlipAsync(UnturnedPlayer player);
        public UniTask JoinCoinFlipAsync(int rank);
        public UniTask AcceptCoinFlipAsync(UnturnedPlayer player);
    }
}