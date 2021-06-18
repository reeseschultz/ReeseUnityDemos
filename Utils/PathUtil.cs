using Unity.Mathematics;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace Reese.Path
{
    public static class PathUtil
    {
        /// <summary>Gets the agent type from the string name.</summary>
        public static int GetAgentType(string agentName)
        {
            for (var i = 0; i < NavMesh.GetSettingsCount(); ++i)
                if (agentName == NavMesh.GetSettingsNameFromID(NavMesh.GetSettingsByIndex(i).agentTypeID))
                    return i;

            return -1;
        }

        /// <summary>Returns true if a status `a` has `b`. Note that a
        /// PathQueryStatus is a set of flags which can be simultaneously
        /// active.</summary>
        public static bool HasStatus(PathQueryStatus a, PathQueryStatus b)
            => (a & b) == a;
    }
}
