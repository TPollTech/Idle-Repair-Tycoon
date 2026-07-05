using UnityEditor;
using UnityEngine;

namespace IdleRepairTycoon.EditorTools
{
    public static class IdleLabEnvironmentEditorBuilder
    {
        [MenuItem("TPoll/Idle Repair Tycoon/Criar laboratorio editavel")]
        public static void CreateEditableLab()
        {
            GameObject root = new GameObject("LabEnvironment_EDITAVEL");
            Selection.activeGameObject = root;
            Debug.Log("LabEnvironment_EDITAVEL criado.");
        }
    }
}
