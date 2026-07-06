using LastAnchor.Holds;
using UnityEngine;

namespace LastAnchor.Level
{
    // Editor-time helper only: right-click this component's header in the Inspector and
    // choose "Generate Hold Grid" to populate a tall facade with HoldPoints without placing
    // each one by hand. Re-running clears and regenerates its children.
    public class HoldGridGenerator : MonoBehaviour
    {
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float maxHeight = 95f;
        [SerializeField] private float verticalSpacing = 2f;
        [SerializeField] private float[] columnX = { -3f, -1f, 1f, 3f };
        [SerializeField] private float wallZ = 4.6f;

        [ContextMenu("Generate Hold Grid")]
        private void GenerateGrid()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            for (float y = minHeight; y <= maxHeight; y += verticalSpacing)
            {
                foreach (float x in columnX)
                {
                    var holdObject = new GameObject("HoldPoint");
                    holdObject.transform.SetParent(transform);
                    holdObject.transform.position = new Vector3(x, y, wallZ);
                    holdObject.AddComponent<HoldPoint>();
                }
            }
        }
    }
}
