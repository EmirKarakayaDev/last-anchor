using UnityEngine;

namespace LastAnchor.Holds
{
    // Pooled visual indicator for a candidate hold. No art yet: just tints whatever
    // renderer is on the prefab (e.g. a small unlit sphere/quad).
    public class HoldHighlight : MonoBehaviour
    {
        private Renderer cachedRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void SetColor(Color color)
        {
            if (cachedRenderer == null)
            {
                return;
            }

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
