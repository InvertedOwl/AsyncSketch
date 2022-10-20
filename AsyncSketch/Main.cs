using HarmonyLib;
using System.Reflection;
using System.Threading;
using UnityModManagerNet;

namespace AsyncSketch
{
#if DEBUG
    [EnableReloading]
#endif
    public static class Main
    {
        static Harmony harmony;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            harmony = new Harmony(entry.Info.Id);

            entry.OnToggle = OnToggle;
#if DEBUG
            entry.OnUnload = OnUnload;
#endif

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry entry, bool active)
        {
            if (active)
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(entry.Info.Id);
            }

            return true;
        }

#if DEBUG
        static bool OnUnload(UnityModManager.ModEntry entry) {
            return true;
        }
#endif

        [HarmonyPatch(typeof(Device), "RunWorldTickUpdate")] 
        class TickAsynchPatch
        {
            static Device inst;
            static bool first;

            private static Thread mainThread;
            public static bool Prefix(bool firstStep, Device __instance)
            {

                if (__instance.state == Device.State.Solid)
                {
                    inst = __instance;
                    first = firstStep;

                    mainThread = new Thread(asyncTick);    
                    mainThread.Start();
                }
                return false;
            }

            public static void asyncTick()
            {
                bool firstStep = first;
                Device __instance = inst;
                foreach (Agent agent in __instance.agents)
                {
                    agent.UpdateDriver();
                }
                foreach (Agent agent2 in __instance.agents)
                {
                    agent2.OnPreWorldTickUpdate();
                }
                foreach (Agent agent3 in __instance.agents)
                {
                    agent3.OnWorldTickUpdate();
                }
                foreach (Agent agent4 in __instance.agents)
                {
                    agent4.OnLateWorldTickUpdate(firstStep);
                }
                foreach (Agent agent5 in __instance.agents)
                {
                    agent5.OnPostWorldTickUpdate();
                }
            }
        }
    }
}
