using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Transformations.EyeGuy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public class AlienIdentityGlobalNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public FasttrackNpcState Fasttrack;
    public AstrodactylNpcState Astrodactyl;
    public BlitzwolferNpcState Blitzwolfer;
    public EchoEchoNpcState EchoEcho;
    public FrankenstrikeNpcState Frankenstrike;
    public HumungousaurNpcState Humungousaur;
    public LodestarNpcState Lodestar;
    public WaterHazardNpcState WaterHazard;
    public GhostFreakNpcState GhostFreak;
    public JetrayNpcState Jetray;
    public BigChillNpcState BigChill;
    public HeatBlastNpcState HeatBlast;
    public EyeGuyNpcState EyeGuy;
    public WhampireNpcState Whampire;
    public RathNpcState Rath;
    public SnareOhNpcState SnareOh;
    public AlienXNpcState AlienX;
    public PeskyDustNpcState PeskyDust;
    public UpgradeNpcState Upgrade;

    public bool IsFasttrackComboActiveFor(int owner) => Fasttrack.IsComboActiveFor(owner);
    public bool IsSkyMarkedFor(int owner) => Astrodactyl.IsSkyMarkedFor(owner);
    public bool IsBlitzwolferResonantFor(int owner) => Blitzwolfer.IsResonantFor(owner);
    public bool IsEchoEchoResonantFor(int owner) => EchoEcho.IsResonantFor(owner);
    public bool IsEchoEchoResonancePrimedFor(int owner) => EchoEcho.IsResonancePrimedFor(owner);
    public bool IsEchoEchoFracturedFor(int owner) => EchoEcho.IsFracturedFor(owner);
    public bool IsUltimateEchoEchoFocusedFor(int owner) => EchoEcho.IsFocusedFor(owner);
    public bool IsFrankenstrikeConductiveFor(int owner) => Frankenstrike.IsConductiveFor(owner);
    public bool IsFrankenstrikeOverchargedFor(int owner) => Frankenstrike.IsOverchargedFor(owner);
    public bool IsHumungousaurBreachedFor(int owner) => Humungousaur.IsBreachedFor(owner);
    public bool IsHumungousaurShatteredFor(int owner) => Humungousaur.IsShatteredFor(owner);
    public bool HasLodestarPolarityFor(int owner) => Lodestar.HasPolarityFor(owner);
    public bool IsWaterHazardSoakedFor(int owner) => WaterHazard.IsSoakedFor(owner);
    public bool IsGhostFreakFearedFor(int owner) => GhostFreak.IsFearedFor(owner);
    public bool IsGhostFreakHauntedFor(int owner) => GhostFreak.IsHauntedFor(owner);
    public bool IsJetrayLockedFor(int owner) => Jetray.IsLockedFor(owner);
    public bool HasBigChillFrostbiteFor(int owner) => BigChill.HasFrostbiteFor(owner);
    public bool IsBigChillDeepFrozenFor(int owner) => BigChill.IsDeepFrozenFor(owner);
    public bool IsBigChillFrigidFracturedFor(int owner) => BigChill.IsFrigidFracturedFor(owner);
    public bool HasHeatBlastFlashpointFor(int owner) => HeatBlast.HasFlashpointFor(owner);
    public bool IsEyeGuyExposedFor(int owner) => EyeGuy.IsExposedFor(owner);
    public bool IsWhampirePreyFor(int owner) => Whampire.IsPreyFor(owner);
    public bool IsRathPreyFor(int owner) => Rath.IsPreyFor(owner);
    public bool IsSnareOhCursedFor(int owner) => SnareOh.IsCursedFor(owner);
    public bool IsAlienXJudgedFor(int owner) => AlienX.IsJudgedFor(owner);
    public bool IsDreamboundFor(int owner) => PeskyDust.IsDreamboundFor(owner);
    public bool IsUpgradeInfectedFor(int owner) => Upgrade.IsInfectedFor(owner);

    public int GetFasttrackComboStacks(int owner) => Fasttrack.GetComboStacks(owner);
    public int GetSkyMarkTime(int owner) => Astrodactyl.GetSkyMarkTime(owner);
    public int GetBlitzwolferResonanceStacks(int owner) => Blitzwolfer.GetResonanceStacks(owner);
    public int GetEchoEchoResonanceStacks(int owner) => EchoEcho.GetResonanceStacks(owner);
    public int GetEchoEchoFractureTime(int owner) => EchoEcho.GetFractureTime(owner);
    public int GetUltimateEchoEchoFocusTime(int owner) => EchoEcho.GetFocusTime(owner);
    public int GetFrankenstrikeConductiveStacks(int owner) => Frankenstrike.GetConductiveStacks(owner);
    public int GetFrankenstrikeOverchargedTime(int owner) => Frankenstrike.GetOverchargedTime(owner);
    public int GetHumungousaurBreachStacks(int owner) => Humungousaur.GetBreachStacks(owner);
    public int GetHumungousaurShatteredTime(int owner) => Humungousaur.GetShatteredTime(owner);
    public int GetLodestarPolarityTime(int owner) => Lodestar.GetPolarityTime(owner);
    public int GetLodestarPolarityDirection(int owner) => Lodestar.GetPolarityDirection(owner);
    public int GetWaterHazardSoak(int owner) => WaterHazard.GetSoak(owner);
    public int GetGhostFreakFearStacks(int owner) => GhostFreak.GetFearStacks(owner);
    public int GetGhostFreakHauntTime(int owner) => GhostFreak.IsHauntedFor(owner) ? GhostFreak.HauntTime : 0;
    public int GetJetrayLockTime(int owner) => Jetray.GetLockTime(owner);
    public int GetBigChillFrostbiteStacks(int owner) => BigChill.GetFrostbiteStacks(owner);
    public int GetBigChillFrostbiteTime(int owner) => BigChill.GetFrostbiteTime(owner);
    public int GetBigChillFrigidFractureTime(int owner) => BigChill.GetFrigidFractureTime(owner);
    public int GetHeatBlastFlashpointStacks(int owner) => HeatBlast.GetFlashpointStacks(owner);
    public int GetRathRendStacks(int owner) => Rath.GetRendStacks(owner);
    public int GetWhampirePreyTime(int owner) => Whampire.GetPreyTime(owner);
    public int GetSnareOhCurseStacks(int owner) => SnareOh.GetCurseStacks(owner);
    public int GetAlienXJudgementStacks(int owner) => AlienX.GetJudgementStacks(owner);
    public int GetPeskyDustDreamTime(int owner) => PeskyDust.GetDreamTime(owner);
    public int GetUpgradeInfectionStacks(int owner) => Upgrade.GetInfectionStacks(owner);
    public int GetUpgradeInfectionTime(int owner) => Upgrade.GetInfectionTime(owner);

    public void ApplyBigChillHoarfrost(int owner, int time, int armorPenetrationBonus = 4) {
        BigChill.ApplyHoarfrost(owner, time, armorPenetrationBonus);
    }

    public bool ConsumeBigChillHoarfrost(int owner) {
        return BigChill.ConsumeHoarfrost(owner);
    }

    public bool CanTriggerBigChillShiverburst(int owner) {
        return BigChill.CanTriggerShiverburst(owner);
    }

    public void TriggerBigChillShiverburstCooldown(int cooldown = 6) {
        BigChill.TriggerShiverburstCooldown(cooldown);
    }

    public void ApplyFasttrackCombo(int owner, int stacks, int time) {
        Fasttrack.ApplyCombo(owner, stacks, time);
    }

    public void ApplySkyMark(int owner, int time) {
        Astrodactyl.ApplySkyMark(owner, time);
    }

    public void ApplyBlitzwolferResonance(int owner, int stacks, int time) {
        Blitzwolfer.ApplyResonance(owner, stacks, time);
    }

    public int ConsumeBlitzwolferResonance(int owner) {
        return Blitzwolfer.ConsumeResonance(owner);
    }

    public void ApplyEchoEchoResonance(int owner, int sourceId, int stacks, int time) {
        EchoEcho.ApplyResonance(owner, sourceId, stacks, time);
    }

    public int ConsumeEchoEchoResonance(int owner) {
        return EchoEcho.ConsumeResonance(owner);
    }

    public void ApplyEchoEchoFracture(int owner, int time) {
        EchoEcho.ApplyFracture(owner, time);
    }

    public void ApplyUltimateEchoEchoFocus(int owner, int time) {
        EchoEcho.ApplyFocus(owner, time);
    }

    public void ApplyFrankenstrikeConductive(int owner, int stacks, int time) {
        Frankenstrike.ApplyConductive(owner, stacks, time);
    }

    public int ConsumeFrankenstrikeConductive(int owner, int amount) {
        return Frankenstrike.ConsumeConductive(owner, amount);
    }

    public int ConsumeFrankenstrikeOvercharged(int owner, int residualConductiveStacks = 0) {
        return Frankenstrike.ConsumeOvercharged(owner, residualConductiveStacks);
    }

    public void ApplyHumungousaurBreach(int owner, int stacks, int time, int shatteredTime) {
        Humungousaur.ApplyBreach(owner, stacks, time, shatteredTime);
    }

    public int ConsumeHumungousaurShattered(int owner, int residualStacks = 0) {
        return Humungousaur.ConsumeShattered(owner, residualStacks);
    }

    public void ApplyLodestarPolarity(int owner, int time, int direction) {
        Lodestar.ApplyPolarity(owner, time, direction);
    }

    public void AddWaterHazardSoak(int owner, int amount, int refreshTime) {
        WaterHazard.AddSoak(owner, amount, refreshTime);
    }

    public int ConsumeWaterHazardSoak(int owner, int amount) {
        return WaterHazard.ConsumeSoak(owner, amount);
    }

    public void ApplyGhostFreakFear(int owner, int stacks, int refreshTime) {
        GhostFreak.ApplyFear(owner, stacks, refreshTime);
    }

    public void ApplyGhostFreakHaunt(int owner, int time) {
        GhostFreak.ApplyHaunt(owner, time);
    }

    public bool ConsumeGhostFreakHaunt(int owner) {
        return GhostFreak.ConsumeHaunt(owner);
    }

    public void ApplyJetrayLock(int owner, int time) {
        Jetray.ApplyLock(owner, time);
    }

    public bool ConsumeJetrayLock(int owner) {
        return Jetray.ConsumeLock(owner);
    }

    public bool ApplyBigChillFrostbite(int owner, int stacks, int refreshTime, int deepFreezeTime,
        int armorPenetrationBonus = 8) {
        return BigChill.ApplyFrostbite(owner, stacks, refreshTime, deepFreezeTime, armorPenetrationBonus);
    }

    public bool RefreshBigChillDeepFreeze(int owner, int amount) {
        return BigChill.RefreshDeepFreeze(owner, amount);
    }

    public bool AddBigChillDeepFreezePressure(int owner, int amount, int threshold) {
        return BigChill.AddDeepFreezePressure(owner, amount, threshold);
    }

    public bool ConsumeBigChillDeepFreeze(int owner) {
        return BigChill.ConsumeDeepFreeze(owner);
    }

    public void ApplyBigChillFrigidFracture(int owner, int time) {
        BigChill.ApplyFrigidFracture(owner, time);
    }

    public int AddHeatBlastFlashpointProgress(int owner, int progress, int refreshTime, int threshold = 3, int maxStacks = 5) {
        return HeatBlast.AddFlashpointProgress(owner, progress, refreshTime, threshold, maxStacks);
    }

    public int ConsumeHeatBlastFlashpoint(int owner) {
        return HeatBlast.ConsumeFlashpoint(owner);
    }

    public bool TryTriggerHeatBlastFlarePop(int owner, int cooldown) {
        return HeatBlast.TryTriggerFlarePop(owner, cooldown);
    }

    public bool HasEyeGuyMark(int owner, EyeGuyElement element) {
        return EyeGuy.HasMark(owner, element);
    }

    public int GetEyeGuyMarkCount(int owner) {
        return EyeGuy.GetMarkCount(owner);
    }

    public EyeGuyElement GetPreferredEyeGuyMark(int owner, EyeGuyElement fallback) {
        return EyeGuy.GetPreferredMark(owner, fallback);
    }

    public bool ApplyEyeGuyMark(int owner, EyeGuyElement element, int time, int exposedTime) {
        return EyeGuy.ApplyMark(owner, element, time, exposedTime);
    }

    public bool ConsumeEyeGuyExposed(int owner) {
        return EyeGuy.ConsumeExposed(owner);
    }

    public void ApplyWhampirePrey(int owner, int time) {
        Whampire.ApplyPrey(owner, time);
    }

    public void ApplyWhampireHypnosis(int time) {
        Whampire.ApplyHypnosis(time);
    }

    public void ApplyRathPrey(int owner, int stacks, int time) {
        Rath.ApplyPrey(owner, stacks, time);
    }

    public int ConsumeRathRend(int owner) {
        return Rath.ConsumeRend(owner);
    }

    public void ClearRathPrey(int owner = -1) {
        Rath.ClearPrey(owner);
    }

    public void ApplySnareOhCurse(int owner, int stacks, int time) {
        SnareOh.ApplyCurse(owner, stacks, time);
    }

    public int ConsumeSnareOhCurse(int owner, int amount) {
        return SnareOh.ConsumeCurse(owner, amount);
    }

    public void ApplyAlienXJudgement(int owner, int stacks, int time) {
        AlienX.ApplyJudgement(owner, stacks, time);
    }

    public int ConsumeAlienXJudgement(int owner, int amount = 99) {
        return AlienX.ConsumeJudgement(owner, amount);
    }

    public void ApplyAlienXStasis(int owner, int time, int judgementStacks = 0) {
        AlienX.ApplyStasis(owner, time, judgementStacks);
    }

    public void ApplyUpgradeInfection(int owner, int stacks, int time, bool mechanicalTarget) {
        Upgrade.ApplyInfection(owner, stacks, time, mechanicalTarget);
    }

    public int ConsumeUpgradeInfection(int owner, out bool wasFullyInfected) {
        return Upgrade.ConsumeInfection(owner, out wasFullyInfected);
    }

    public void AddPeskyDrowsy(int owner, int amount, int refreshTime, int dreamThreshold, int dreamTime) {
        PeskyDust.AddDrowsy(owner, amount, refreshTime, dreamThreshold, dreamTime);
    }

    public void ApplyDreambound(int owner, int dreamTime, int residualDrowsy = 0) {
        PeskyDust.ApplyDreambound(owner, dreamTime, residualDrowsy);
    }

    public override void AI(NPC npc) {
        GhostFreak.BeforeStatusTick(npc);

        TickStatuses();

        Frankenstrike.HandleOverchargedArcs(npc);
        GhostFreak.ApplyAI(npc);
        Whampire.ApplyAI(npc);
        AlienX.ApplyAI(npc);
        BigChill.ApplyAI(npc);
        Humungousaur.ApplyAI(npc);
        PeskyDust.ApplyAI(npc);
        Upgrade.HandleOverclockPulse(npc);
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) {
        modifiers.ArmorPenetration += BigChill.GetArmorPenetration(player.whoAmI);

        if (IsFrankenstrikeOverchargedFor(player.whoAmI))
            modifiers.ArmorPenetration += 8;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        modifiers.ArmorPenetration += BigChill.GetArmorPenetration(projectile.owner);

        if (IsFrankenstrikeOverchargedFor(projectile.owner))
            modifiers.ArmorPenetration += 8;
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (npc.HasBuff(ModContent.BuffType<AlienXSupernovaBurn>())) {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= AlienXSupernovaBurn.LifeRegenPenalty;
            damage = System.Math.Max(damage, AlienXSupernovaBurn.CombatTextDamage);
        }
    }

    public override void OnKill(NPC npc) {
        int eyeGuyOwner = EyeGuy.GetExposedOwner();
        if (eyeGuyOwner < 0 || eyeGuyOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[eyeGuyOwner];
        if (!owner.active)
            return;

        owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(npc.boss ? 1.5f : 3f);
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (npc.HasBuff(ModContent.BuffType<AlienXSupernovaBurn>()))
            drawColor = Color.Lerp(drawColor, new Color(255, 188, 118), 0.34f);

        Fasttrack.ApplyDrawEffects(ref drawColor);
        Astrodactyl.ApplyDrawEffects(ref drawColor);
        Blitzwolfer.ApplyDrawEffects(ref drawColor);
        EchoEcho.ApplyDrawEffects(ref drawColor);
        Frankenstrike.ApplyDrawEffects(ref drawColor);
        Humungousaur.ApplyDrawEffects(ref drawColor);
        Lodestar.ApplyDrawEffects(ref drawColor);
        WaterHazard.ApplyDrawEffects(ref drawColor);
        GhostFreak.ApplyDrawEffects(ref drawColor);
        Jetray.ApplyDrawEffects(ref drawColor);
        BigChill.ApplyDrawEffects(ref drawColor);
        HeatBlast.ApplyDrawEffects(ref drawColor);
        EyeGuy.ApplyDrawEffects(ref drawColor);
        Rath.ApplyDrawEffects(ref drawColor);
        Whampire.ApplyDrawEffects(ref drawColor);
        SnareOh.ApplyDrawEffects(ref drawColor);
        AlienX.ApplyDrawEffects(ref drawColor);
        PeskyDust.ApplyDrawEffects(ref drawColor);
        Upgrade.ApplyDrawEffects(ref drawColor);
    }

    private void TickStatuses() {
        Fasttrack.Tick();
        Astrodactyl.Tick();
        Blitzwolfer.Tick();
        EchoEcho.Tick();
        Frankenstrike.Tick();
        Humungousaur.Tick();
        Lodestar.Tick();
        WaterHazard.Tick();
        GhostFreak.Tick();
        Jetray.Tick();
        BigChill.Tick();
        HeatBlast.Tick();
        EyeGuy.Tick();
        Whampire.Tick();
        Rath.Tick();
        SnareOh.Tick();
        AlienX.Tick();
        PeskyDust.Tick();
        Upgrade.Tick();
    }
}
