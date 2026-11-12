using UnityEngine;

namespace FishingGame.Views
{
    [RequireComponent(typeof(LineRenderer))]
    public class FishingLine : MonoBehaviour
    {
        public Transform hookTarget; // 鱼钩的Transform
        public Transform topAnchor;  // 鱼线顶端的锚点

        private LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            // 如果没有手动设置顶部锚点，就默认在相机上方的水面上
            if (topAnchor == null)
            {
                GameObject anchor = new GameObject("LineTopAnchor");
                anchor.transform.position = new Vector3(0, 0, 0);
                topAnchor = anchor.transform;
            }
        }

        // 使用 LateUpdate 确保在鱼钩移动完毕后再更新鱼线，避免视觉延迟
        private void LateUpdate()
        {
            if (hookTarget == null || topAnchor == null) return;

            // 让起点的Y一直保持在水面以上
            Vector3 startPos = new Vector3(hookTarget.position.x, topAnchor.position.y, 0);

            // 设置鱼线的终点（鱼钩的位置）
            Vector3 endPos = new Vector3(hookTarget.position.x, hookTarget.position.y, 0);

            // 应用坐标
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }
}