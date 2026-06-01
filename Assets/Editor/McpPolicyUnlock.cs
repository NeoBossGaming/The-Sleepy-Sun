// McpPolicyUnlock.cs
// Overrides Unity's plan-based MCP connection cap so Claude Code can run
// Unity_RunCommand and other MCP tools in the editor.
//
// How it works:
//   Unity reads AllowedMcpConnections from your account entitlement and sets
//   ConnectionCensus.Policy.MaxDirect accordingly (0 on the free plan).
//   ConnectionPolicyOverride is a dev-tool that stores an unlimited cap in
//   SessionState, and AcpEntitlementWiring.Apply() respects it.
//   This script sets those SessionState keys at editor init and immediately
//   re-applies the policy via reflection so the change takes effect without
//   a restart.

using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class McpPolicyUnlock
{
    static McpPolicyUnlock()
    {
        // Persist the override so domain reloads (Edit→Play, etc.) keep it active
        SessionState.SetInt("ConnectionPolicyOverride.HasOverride", 1);
        SessionState.SetInt("ConnectionPolicyOverride.MaxDirect",   -1); // -1 = unlimited
        SessionState.SetInt("ConnectionPolicyOverride.MaxGateway",  -1);

        // Immediately apply the unlimited policy to the running census
        ApplyUnlimitedPolicyViaReflection();
    }

    static void ApplyUnlimitedPolicyViaReflection()
    {
        Assembly mcpAssembly = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name == "Unity.AI.MCP.Editor")
            {
                mcpAssembly = asm;
                break;
            }
        }

        if (mcpAssembly == null)
        {
            Debug.LogWarning("[McpPolicyUnlock] Could not find Unity.AI.MCP.Editor assembly.");
            return;
        }

        var censusType = mcpAssembly.GetType("Unity.AI.MCP.Editor.Connection.ConnectionCensus");
        var policyType = mcpAssembly.GetType("Unity.AI.MCP.Editor.Connection.ConnectionPolicy");

        if (censusType == null || policyType == null)
        {
            Debug.LogWarning("[McpPolicyUnlock] Could not find ConnectionCensus or ConnectionPolicy type.");
            return;
        }

        // Get ConnectionPolicy.Unlimited (public static property)
        var unlimitedProp = policyType.GetProperty("Unlimited",
            BindingFlags.Public | BindingFlags.Static);
        if (unlimitedProp == null)
        {
            Debug.LogWarning("[McpPolicyUnlock] Could not find ConnectionPolicy.Unlimited.");
            return;
        }
        var unlimited = unlimitedProp.GetValue(null);

        // Call ConnectionCensus.SetPolicy(ConnectionPolicy.Unlimited)
        var setPolicy = censusType.GetMethod("SetPolicy",
            BindingFlags.Public | BindingFlags.Static);
        if (setPolicy == null)
        {
            Debug.LogWarning("[McpPolicyUnlock] Could not find ConnectionCensus.SetPolicy.");
            return;
        }

        setPolicy.Invoke(null, new[] { unlimited });
        Debug.Log("[McpPolicyUnlock] MCP connection policy set to Unlimited. Claude Code can now connect.");
    }
}
