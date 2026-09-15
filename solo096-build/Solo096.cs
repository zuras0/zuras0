using System;
using System.ComponentModel;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerRoles;
using ServerHandlers = Exiled.Events.Handlers.Server;

namespace Solo096
{
    public sealed class Config : IConfig
    {
        [Description("Whether or not the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether debug messages should be shown in the server console.")]
        public bool Debug { get; set; } = false;

        [Description("Chance in percent (0-100) that SCP-096 replaces the sole SCP when a round starts with exactly one SCP.")]
        public float Solo096Chance { get; set; } = 20f;
    }

    public sealed class Plugin : Plugin<Config>
    {
        private readonly Random random = new Random();

        public override string Name => "Solo096";
        public override string Author => "OpenAI for Aleks";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        public override void OnEnabled()
        {
            ServerHandlers.AllPlayersSpawned += OnAllPlayersSpawned;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ServerHandlers.AllPlayersSpawned -= OnAllPlayersSpawned;
            base.OnDisabled();
        }

        private void OnAllPlayersSpawned()
        {
            var scps = Player.List
                .Where(player => player != null && player.IsConnected && player.IsAlive && player.IsScp)
                .ToList();

            if (scps.Count != 1)
            {
                if (Config.Debug)
                    Log.Info($"[Solo096] No change: found {scps.Count} SCP players.");
                return;
            }

            Player soleScp = scps[0];

            if (soleScp.Role.Type == RoleTypeId.Scp096)
            {
                if (Config.Debug)
                    Log.Info("[Solo096] The sole SCP is already SCP-096.");
                return;
            }

            float chance = Math.Max(0f, Math.Min(100f, Config.Solo096Chance));
            double roll = random.NextDouble() * 100.0;

            if (roll >= chance)
            {
                if (Config.Debug)
                    Log.Info($"[Solo096] Roll {roll:F2} >= {chance:F2}; keeping {soleScp.Role.Type}.");
                return;
            }

            RoleTypeId previousRole = soleScp.Role.Type;
            soleScp.Role.Set(RoleTypeId.Scp096);

            Log.Info($"[Solo096] Solo SCP roll succeeded ({roll:F2} < {chance:F2}). Replaced {previousRole} with SCP-096.");
        }
    }
}
