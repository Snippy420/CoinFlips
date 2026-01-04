using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CoinFlips.Models;
using CoinFlips.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;

namespace CoinFlips.Managers;

[PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public class CoinFlipManager : ICoinFlipManager
{
    private readonly IUserManager _userManager;
    private readonly IUnturnedUserDirectory _userDirectory;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer _localizer;
    
    public Dictionary<CSteamID, int> CoinFlips { get; set; } = new();

    public CoinFlipManager(IUserManager userManager, 
        IUnturnedUserDirectory userDirectory, 
        IConfiguration configuration, 
        IStringLocalizer localizer)
    {
        _userManager = userManager;
        _userDirectory = userDirectory;
        _configuration = configuration;
        _localizer = localizer;
    }
    
    public async UniTask<string> ActiveCoinFlips(int page)
    {
        var pageSize = 5;
        
        if (page < 1) page = 1;
        var entries = CoinFlips.ToList();
        
        var startIndex = (page - 1) * pageSize;
        if(startIndex >= entries.Count)
            return string.Empty;
        
        var pageItems = entries
            .Skip(startIndex)
            .Take(pageSize)
            .ToList();
        
        var result = new StringBuilder();
        
        for(int i = 0; i < pageItems.Count; i++)
        {
            int index = startIndex + i + 1;
            var (key, value) = (pageItems[i].Key, pageItems[i].Value);

            var user = await _userManager.FindUserAsync(KnownActorTypes.Player, key.ToString(), UserSearchMode.FindById);
            var name = user?.DisplayName ?? key.ToString();
            
            result.AppendLine($"{index}. {name}: ${value:N0}");
        }

        return result.ToString();
    }

    public async UniTask CreateCoinFlip(UnturnedPlayer player, int amount)
    {
        if (player.Player.skills.experience < amount) throw new UserFriendlyException(_localizer["insufficient_funds"]);
        if (CoinFlips.ContainsKey(player.SteamId))
            throw new UserFriendlyException(_localizer["existing_cf"]);
        
        if (amount < _configuration.GetValue<int>("min_bet")) 
            throw new UserFriendlyException(_localizer["min_bet", new { Amount = _configuration.GetValue<int>("min_bet").ToString("N0") }]);
        if (amount > _configuration.GetValue<int>("max_bet")) 
            throw new UserFriendlyException(_localizer["max_bet", new { Amount = _configuration.GetValue<int>("max_bet").ToString("N0") }]);
        
        CoinFlips.Add(player.SteamId, amount);
    }

    public async UniTask RemoveCoinFlip(UnturnedPlayer player)
    {
        if (!CoinFlips.ContainsKey(player.SteamId)) throw new UserFriendlyException(_localizer["unable_to_find"]);

        CoinFlips.Remove(player.SteamId);
    }

    public async UniTask AcceptCoinFlip(UnturnedPlayer player, CSteamID lister)
    {
        if (!CoinFlips.ContainsKey(lister)) throw new UserFriendlyException(_localizer["unable_to_find"]);
        var listing = CoinFlips.FirstOrDefault(x => x.Key == lister);

        if (player.SteamId == lister) throw new UserFriendlyException("You cannot accept your own CF");
        if (player.Player.skills.experience < listing.Value) throw new UserFriendlyException(_localizer["insufficient_funds"]);

        var uLister = _userDirectory.FindUser(lister);
        if (uLister == null)
        {
            CoinFlips.Remove(lister);
            throw new UserFriendlyException(_localizer["unknown"]);
        }

        if (uLister.Player.Player.skills.experience < listing.Value)
        {
            CoinFlips.Remove(lister);
            throw new UserFriendlyException(_localizer["lister_insufficient_funds"]);
        }
        
        CoinFlips.Remove(lister);

        for (var i = 0; i < 3; i++)
        {
            await player.PrintMessageAsync(_localizer["flipping"], Color.Lime);
            await uLister.PrintMessageAsync(_localizer["flipping"], Color.Lime);
            await UniTask.Delay(1000);
        }

        var random = new Random();

        var winner = random.Next(2) == 0 ? player.SteamId : lister;

        var uWinner = _userDirectory.FindUser(winner);
        if (uWinner == null)
        {
            throw new UserFriendlyException(_localizer["unknown"]);
        }

        await UniTask.SwitchToMainThread();
        uWinner.Player.Player.skills.ServerModifyExperience((int)(listing.Value * _configuration.GetValue<float?>("tax_multi") ?? 0.95));
        
        var url = _configuration.GetValue<string>("WEBHOOK_URL") ?? "N/A";
        
        List<Field> fields = new List<Field>();
        if (uWinner.SteamId == lister)
        {
            player.Player.skills.ServerModifyExperience(-listing.Value);
            await player.PrintMessageAsync(_localizer["you_lost", new { Player = uWinner.DisplayName }], Color.Red);
            await uWinner.PrintMessageAsync(_localizer["you_win",
                new
                {
                    Amount = (listing.Value * _configuration.GetValue<float?>("tax_multi") ?? 0.95).ToString("N0"),
                    Player = player.SteamPlayer.playerID.characterName
                }], Color.Lime);
            
            fields = new List<Field>
            {
                new()
                {
                    name = "Winner",
                    value = uWinner.DisplayName,
                    inline = true
                },
                new()
                {
                    name = "Loser",
                    value = player.SteamPlayer.playerID.characterName,
                    inline = true
                },
                new()
                {
                    name = "Amount",
                    value = (listing.Value * _configuration.GetValue<float?>("tax_multi") ?? 0.95).ToString("N0"),
                    inline = true
                }
            };

        }
        else
        {
            var uLoser = _userDirectory.FindUser(lister);
            if (uLoser == null)
            {
                throw new UserFriendlyException("An unknown error has occured");
            }
            
            uLoser.Player.Player.skills.ServerModifyExperience(-listing.Value);
            await uLoser.PrintMessageAsync(_localizer["you_lost", new { Player = uWinner.DisplayName }], Color.Red);
            await uWinner.PrintMessageAsync(_localizer["you_win",
                new
                {
                    Amount = (listing.Value * _configuration.GetValue<float?>("tax_multi") ?? 0.95).ToString("N0"),
                    Player = uLoser.DisplayName
                }], Color.Lime);
            
            fields = new List<Field>
            {
                new()
                {
                    name = "Winner",
                    value = uWinner.DisplayName,
                    inline = true
                },
                new()
                {
                    name = "Loser",
                    value = uLoser.DisplayName,
                    inline = true
                },
                new()
                {
                    name = "Amount",
                    value = (listing.Value * _configuration.GetValue<float?>("tax_multi") ?? 0.95).ToString("N0"),
                    inline = true
                }
            };
        }

        var webhook = new Webhook()
        {
            embeds = new List<Embed>()
            {
                new Embed()
                {
                    title = "Coinflip",
                    color = 0x1294F1,
                    fields = fields,
                    timestamp = DateTime.UtcNow
                }
            }
        };
        
        webhook.send(url);
    }
}