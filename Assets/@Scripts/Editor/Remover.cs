using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class RemoveScriptMachine
{
    [MenuItem("Tools/Remove All ScriptMachines And Variables")]
    static void RemoveAll()
    {
        ScriptMachine[] machines = Object.FindObjectsOfType<ScriptMachine>(true);

        int count = machines.Length;

        foreach (var m in machines)
        {
            Undo.DestroyObjectImmediate(m);
        }

        Debug.Log($"Removed {count} ScriptMachine components.");

        Variables[] variables = Object.FindObjectsOfType<Variables>(true);

        int count2 = variables.Length;

        foreach (var v in variables)
        {
            Undo.DestroyObjectImmediate(v);
        }

        Debug.Log($"Removed {count} variables components.");

        StateMachine[] sms = Object.FindObjectsOfType<StateMachine>(true);

        int smsCount = sms.Length;

        foreach (var sm in sms)
        {
            Undo.DestroyObjectImmediate(sm);
        }

        Debug.Log($"Removed {count} StateMachine components.");
    }
}