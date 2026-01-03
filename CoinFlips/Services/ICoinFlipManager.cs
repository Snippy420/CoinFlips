using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players;
using Steamworks;

namespace CoinFlips.Services;

[Service]
public interface ICoinFlipManager
{
    public Dictionary<CSteamID, int> CoinFlips { get; set; }
        
    public UniTask<string> ActiveCoinFlips(int page);
    public UniTask CreateCoinFlip(UnturnedPlayer player, int amount);
    public UniTask RemoveCoinFlip(UnturnedPlayer player);
    public UniTask AcceptCoinFlip(UnturnedPlayer player, CSteamID lister);
}