using UnityEngine;
using UnityEditor;

public class ResetToBindPose : EditorWindow
{
    [MenuItem("Tools/Reset to Bind Pose")]
    public static void ResetBindPose()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"⚠️ No Animator found on: {obj.name}");
                continue;
            }

            // Restore the model to its true bind pose
            animator.avatar = animator.avatar; // Force reset the avatar
            animator.Rebind();
            animator.Update(0);

            Debug.Log($"✅ Reset to bind pose for: {obj.name}");

            // Ensure Unity updates the scene
            EditorUtility.SetDirty(obj);
            SceneView.RepaintAll();
        }

        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ No GameObject selected. Select a model with an Animator.");
        }
    }
}