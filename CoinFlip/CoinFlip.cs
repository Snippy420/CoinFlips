﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;

// For more, visit https://openmod.github.io/openmod-docs/devdoc/guides/getting-started.html

[assembly: PluginMetadata("CoinFlip", DisplayName = "Coin Flip")]

namespace CoinFlip
{
    public class CoinFlip : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<CoinFlip> m_Logger;

        public CoinFlip(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<CoinFlip> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
        }

        protected override async UniTask OnLoadAsync()
        {
            // await UniTask.SwitchToMainThread(); uncomment if you have to access Unturned or UnityEngine APIs
            m_Logger.LogInformation("Hello World!");

            // await UniTask.SwitchToThreadPool(); // you can switch back to a different thread
        }

        protected override async UniTask OnUnloadAsync()
        {
            // await UniTask.SwitchToMainThread(); uncomment if you have to access Unturned or UnityEngine APIs
            m_Logger.LogInformation(m_StringLocalizer["plugin_events:plugin_stop"]);
        }
    }
}