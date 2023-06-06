using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChildrenAlsoMatter
{
    public class ChildrenAlsoMatter : NeosMod
    {
        public override string Name => "ChildrenAlsoMatter";
        public override string Author => "eia485";
        public override string Version => "1.0.1";
        public override string Link => "https://github.com/EIA485/NeosChildrenAlsoMatter/";
        public override void OnEngineInit()
        {
            new Harmony("net.eia485.ChildrenAlsoMatter").PatchAll();
        }

        [HarmonyPatch(typeof(DelegateEditor), nameof(DelegateEditor.TryGrab))]
        class ChildrenAlsoMatter_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
            {
                int hit = 0;
                foreach (var code in codes)
                {
                    if (hit <= 0 && (code.operand as MethodInfo)?.Name == "GetGenericArguments")
                    {
                        hit++;
                        yield return new(OpCodes.Call, AccessTools.Method(typeof(ChildrenAlsoMatter), nameof(FirstGeneric)));
                    }
                    else if (hit == 1  || hit == 2)
                    {
                        hit++;
                        yield return new(OpCodes.Nop);
                    }
                    else
                    {
                        yield return code;
                    }
                }
            }
        }
        static Type FirstGeneric(Type last)
        {
            var generics = last.GetGenericArguments();
            if (generics != null && generics.Length > 0) 
            {
                foreach (var generic in generics)
                {
                    if(typeof(Delegate).IsAssignableFrom(generic)) return generic;
                }
            }
            var baseType = last.BaseType;
            if (baseType != null) return FirstGeneric(baseType);
            return null;
        }
    }
}