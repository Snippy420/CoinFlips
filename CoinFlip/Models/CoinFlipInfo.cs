using System;
using OpenMod.Unturned.Players;
using Steamworks;

namespace CoinFlip.Models
{
    public class CoinFlipInfo
    {
        public UnturnedPlayer Owner { get; set; }
        public int Amount { get; set; }
    
        public CoinFlipInfo(UnturnedPlayer owner, int amount)
        {
            Owner = owner;
            Amount = amount;
        }
    }
}