using UnityEngine;

public class 指示器 : MonoBehaviour
{
	[Header("References")]
	public Transform player;          // 玩家
	public Transform target;          // 任务目标
	public RectTransform icon;        // 橘色点
	public RectTransform mapRoot;     // 小地图圆形根节点（circlemask）

	[Header("Settings")]
	public float worldRadius = 50f;   // 小地图在世界中的覆盖半径
	public float edgePadding = 8f;    // 防止贴边被裁掉

	float uiRadius;

	void Start()
	{
		// UI 圆形半径
		uiRadius = mapRoot.rect.width * 0.5f - edgePadding;
	}

	void Update()
	{
		if (!player || !target || !icon) return;

		// 1️⃣ 世界方向（XZ 平面）
		Vector3 worldOffset = target.position - player.position;
		Vector2 dir = new Vector2(worldOffset.x, worldOffset.z);

		// 2️⃣ 世界 → UI 缩放
		Vector2 uiPos = (dir / worldRadius) * uiRadius;

		// 3️⃣ 超出范围 → 贴圆边
		if (uiPos.magnitude > uiRadius)
		{
			uiPos = uiPos.normalized * uiRadius;
		}

		// 4️⃣ 设置位置
		icon.anchoredPosition = uiPos;
	}
}