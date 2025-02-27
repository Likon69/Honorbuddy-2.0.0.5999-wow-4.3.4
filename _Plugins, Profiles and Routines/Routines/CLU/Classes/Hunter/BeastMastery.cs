using CLU.Helpers;

using CommonBehaviors.Actions;

using TreeSharp;

namespace CLU.Classes.Hunter
{
    using global::CLU.GUI;

    class BeastMastery : RotationBase
    {
        public override string Name
        {
            get { return "BeastMastery Hunter"; }
        }

        // public static readonly HealerBase Healer = HealerBase.Instance;

        public override string KeySpell
        {
            get { return "Intimidation"; }
        }

        public override float CombatMinDistance
        {
            get { return 30f; }
        }

        // adding some help
        public override string Help
        {
            get
            {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Trap Launcher or Feign Death will halt the rotation\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Rapid Fire & Serpent Sting & Bestial Wrath & Fervor & Call of the Wild \n" +
                       "3. AoE with Multishot and Explosive Trap\n" +
                       "4. Best Suited for T13 end game raiding\n" +
                       "Fox		-haste\n" +
                       "Cat		+str/agil\n" +
                       "Core Hound	(+haste &-cast speed) exotic \n" +
                       "Silithid		(+health) exotic\n" +
                       "Wolf		(+crit)\n" +
                       "Shale Spider	(+5% stats) exotic\n" +
                       "Raptor		(-armor)\n" +
                       "Carrion Bird	(-physical damage)\n" +
                       "Sporebat	(-cast speed)\n" +
                       "Ravager		(-Phys Armor)\n" +
                       "Wind Serpent	(-Spell Armor)\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new Decorator(ret => !SettingsFile.Instance.AutoManagePauseRotation, 
                    new Sequence(
                    new PrioritySelector(
                        // Trinkets & Cooldowns
                        new Decorator(
                            ret => (Units.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("Feign Death")),
                                new PrioritySelector(
                                        Item.UseTrinkets(), 
                                        Spells.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseEngineerGloves())),
                        // Main Rotation
                        new Decorator(
                            ret => !Buff.PlayerHasBuff("Feign Death"),
                            new PrioritySelector(
                                // Extra button automation
                                Items.RunMacroText("/click ExtraActionButton1", ret => (Units.IsFadingLight() || Units.IsShrapnel() || Units.IsHourofTwilight()) && SettingsFile.Instance.AutoExtraActionButton, "[Push Button] ExtraActionButton1"),
                                Buff.CastDebuff("Hunter's Mark",           ret => !Buff.TargetHasDebuff("Hunter's Mark", Me.CurrentTarget) && (Me.CurrentTarget.CurrentHealth > 310000 || Me.CurrentTarget.MaxHealth == 1), "Hunter's Mark"),
                                Spells.CastSelfSpell("Feign Death",         ret => Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Feign Death Threat"),
                                Spells.CastSpell("Concussive Shot",         ret => Me.CurrentTarget.CurrentTargetGuid == Me.Guid, "Concussive Shot"),
                                new Decorator(
                                    ret => Me.IsMoving,
                                        new Sequence(
                                        // Waiting for a bit just incase we are only moving outa the fire!
                                            // new ActionSleep(1500),
                                            Buff.CastBuff("Aspect of the Fox", ret => Me.IsMoving, "[Aspect] of the Fox - Moving"))),
                                            Buff.CastBuff("Aspect of the Hawk", ret => !Me.IsMoving, "[Aspect] of the Hawk"),
                                Spells.CastSpell("Kill Shot",               ret => true, "Kill Shot"),
                                Spells.CastSpell("Raptor Strike",           ret => !Lists.BossList.IgnoreAoE.Contains(Me.CurrentTarget.Entry) && Me.CurrentTarget.IsWithinMeleeRange, "Raptor Strike (Melee)"),
                                Buff.CastDebuff("Wing Clip",                ret => !Lists.BossList.IgnoreAoE.Contains(Me.CurrentTarget.Entry) && Me.CurrentTarget.IsWithinMeleeRange, "Wing Clip (Melee)"),
                                // Main rotation
                                Pets.CastPetSpell("Call of the Wild",     ret => Units.IsTargetWorthy(Me.CurrentTarget) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Call of the Wild"),
                                Buff.CastBuff("Focus Fire",                 ret => Pets.PetCountBuff("Frenzy Effect") == 5 && Pets.PetHasBuff("Frenzy Effect") && !Buff.PlayerHasBuff("The Beast Within"), "Focus Fire"),
                                Buff.CastDebuff("Serpent Sting",            ret => Buff.TargetDebuffTimeLeft("Serpent Sting").TotalSeconds <= 0.5 && Units.IsTargetWorthy(Me.CurrentTarget), "Serpent Sting"),
                                Buff.CastBuff("Bestial Wrath",              ret => Me.FocusPercent > 60 && Units.IsTargetWorthy(Me.CurrentTarget), "Bestial Wrath"),
                                Spells.CastSpell("Multi-Shot",              ret => Units.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) > 3, "Multi-Shot"),
                                Spells.HunterTrapBehavior("Explosive Trap", ret => Me.CurrentTarget, ret => !Lists.BossList.IgnoreAoE.Contains(Me.CurrentTarget.Entry) && Units.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 0),
                                Spells.CastSpell("Cobra Shot",              ret => Me.IsMoving && Buff.PlayerHasBuff("Aspect of the Fox") && Units.CountEnnemiesInRange(Me.Location, 30) > 4, "Cobra Shot (Moving AoE)"),
                                Spells.CastSpell("Cobra Shot",              ret => Units.CountEnnemiesInRange(Me.Location, 30) > 4, "Cobra Shot"),
                                Buff.CastBuff("Rapid Fire",                 ret => Units.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("The Beast Within") && !Buffs.UnitHasHasteBuff(Me), "Rapid Fire"),
                                Spells.CastSelfSpell("Kill Command",        ret => Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Kill Command"),
                                Spells.CastSelfSpell("Fervor",              ret => Me.FocusPercent <= 37 && Units.IsTargetWorthy(Me.CurrentTarget), "Fervor"),
                                Spells.CastSpell("Arcane Shot",             ret => (Me.FocusPercent >= 59 || Buff.PlayerHasBuff("The Beast Within")), "Arcane Shot"),
                                Spells.CastSpell("Cobra Shot",              ret => true, "Cobra Shot"))))));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(
                        ret => Me.HealthPercent < 100 && !Buff.PlayerHasBuff("Feign Death") && SettingsFile.Instance.AutoManageHealing,
                        new PrioritySelector(
                            Items.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"),
                            Spells.CastSelfSpell("Deterrence",              ret => Me.HealthPercent < 40 && Me.HealthPercent > 1, "Deterrence"))),
                    new Decorator(
                        ret => !Buff.PlayerHasBuff("Feign Death"),
                        new PrioritySelector(
                            Pets.CastPetSpell("Mend Pet",                   ret => Me.GotAlivePet && (Me.Pet.HealthPercent < 70 || Me.Pet.HappinessPercent < 90) && !Pets.PetHasBuff("Mend Pet"), "Mend Pet"),
                            Pets.CastPetSpell("Heart of the Phoenix",       ret => !Me.GotAlivePet, "Heart of the Phoenix"))));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                            ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink") && !Buff.PlayerHasBuff("Feign Death") && !Buff.PlayerHasBuff("Trap Launcher"),
                            new PrioritySelector(
                                new Decorator(
                                    ret => !Me.GotAlivePet || Pet == null,
                                        new PrioritySelector(
                                            Pets.CastPetSummonSpell("Call Pet 1", ret => Pet == null, "Calling Pet"),
                                            new WaitContinue(2, ret => Me.GotAlivePet || Me.Combat, new ActionAlwaysSucceed()),
                                            Spells.CastSelfSpell("Revive Pet", ret => !Me.GotAlivePet, "Revive Pet")))));
            }
        }

        public override Composite PVPRotation
        {
            get { return SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return SingleRotation; }
        }
    }
}