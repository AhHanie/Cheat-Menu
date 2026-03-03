using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), "SkillsTickInterval")]
    public static class Pawn_SkillTracker_SkillsTickInterval_Patch
    {
        private static readonly MethodInfo SkillIntervalMethod =
            AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Interval));

        private static readonly MethodInfo ShouldSkipSkillDecayMethod =
            AccessTools.Method(typeof(Pawn_SkillTracker_SkillsTickInterval_Patch), nameof(ShouldSkipSkillDecay));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            try
            {
                int insertIndex = FindSkillIntervalLoopStart(code);
                if (insertIndex < 0)
                {
                    Logger.Error("Pawn_SkillTracker_SkillsTickInterval_Patch transpiler failed: could not find skill Interval loop start.");
                    return code;
                }

                Label continueLabel = generator.DefineLabel();
                code[insertIndex].labels.Add(continueLabel);

                List<CodeInstruction> injected = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, ShouldSkipSkillDecayMethod),
                    new CodeInstruction(OpCodes.Brfalse_S, continueLabel),
                    new CodeInstruction(OpCodes.Ret)
                };

                if (code[insertIndex].labels.Count > 1)
                {
                    List<Label> existingLabels = new List<Label>(code[insertIndex].labels);
                    existingLabels.Remove(continueLabel);
                    for (int i = 0; i < existingLabels.Count; i++)
                    {
                        Label label = existingLabels[i];
                        code[insertIndex].labels.Remove(label);
                        injected[0].labels.Add(label);
                    }
                }

                code.InsertRange(insertIndex, injected);
                Logger.Message("Pawn_SkillTracker_SkillsTickInterval_Patch transpiler succeeded.");
                return code;
            }
            catch (Exception exception)
            {
                Logger.Exception(exception, "Pawn_SkillTracker_SkillsTickInterval_Patch transpiler failed");
                return code;
            }
        }

        private static int FindSkillIntervalLoopStart(List<CodeInstruction> code)
        {
            int intervalCallIndex = code.FindIndex(instruction => instruction.Calls(SkillIntervalMethod));
            if (intervalCallIndex < 0)
            {
                return -1;
            }

            for (int i = intervalCallIndex; i >= 1; i--)
            {
                if (!IsStlocInstruction(code[i]))
                {
                    continue;
                }

                if (code[i - 1].opcode == OpCodes.Ldc_I4_0)
                {
                    return i - 1;
                }
            }

            return -1;
        }

        private static bool IsStlocInstruction(CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Stloc_0 ||
                instruction.opcode == OpCodes.Stloc_1 ||
                instruction.opcode == OpCodes.Stloc_2 ||
                instruction.opcode == OpCodes.Stloc_3 ||
                instruction.opcode == OpCodes.Stloc ||
                instruction.opcode == OpCodes.Stloc_S)
            {
                return true;
            }

            return false;
        }

        private static bool ShouldSkipSkillDecay()
        {
            return Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableSkillDecayKey);
        }
    }
}
