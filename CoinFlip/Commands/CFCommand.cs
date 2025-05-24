using System;
using CoinFlip.Services;
using Cysharp.Threading.Tasks;
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
    
        public CFCommand(IServiceProvider serviceProvider, ICoinFlipManager coinFlipManager) : base(serviceProvider)
        {
            _coinFlipManager = coinFlipManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);
            var uUser = (UnturnedUser)Context.Actor;
            var amount = await Context.Parameters.GetAsync<int>(0);
            UnturnedUser? target = null;
            if (Context.Parameters.Length > 1) target = await Context.Parameters.GetAsync<UnturnedUser>(1);
        
            if (target == null) _coinFlipManager.CreateCoinFlipAsync(uUser.Player, amount, null);
            else _coinFlipManager.CreateCoinFlipAsync(uUser.Player, amount, target.Player);
        }
    }
}