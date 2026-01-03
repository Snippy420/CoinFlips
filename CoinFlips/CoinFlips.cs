using System;
using CoinFlips.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using MoreLinq.Extensions;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;

// For more, visit https://openmod.github.io/openmod-docs/devdoc/guides/getting-started.html

[assembly: PluginMetadata("CoinFlips", DisplayName = "CoinFlips", Author = ".f.i.n.")]

namespace CoinFlips
{
    public class CoinFlips : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<CoinFlips> m_Logger;
        private readonly ICoinFlipManager _coinFlipManager;

        public CoinFlips(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<CoinFlips> logger,
            IServiceProvider serviceProvider, ICoinFlipManager coinFlipManager) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            _coinFlipManager = coinFlipManager;
        }

        protected override async UniTask OnLoadAsync()
        {
        }

        protected override async UniTask OnUnloadAsync()
        {
        }
    }
}