using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 任务管理器 - 管理所有任务点，控制游戏流程
/// </summary>
public class MissionManager : MonoBehaviour
{
    [Header("任务设置")]
    [Tooltip("每局需要完成的任务点数量")]
    public int pointsPerMission = 5;

    [Tooltip("所有可用的任务点（包含开始点）")]
    public MissionPoint[] allMissionPoints;

    [Header("玩家设置")]
    [Tooltip("玩家Transform")]
    public Transform playerTransform;

    [Header("倒计时设置")]
    [Tooltip("任务时间限制（秒）")]
    public float missionTimeLimit = 180f; // 3分钟 = 180秒

    [Tooltip("游戏开始前的延迟时间（秒）")]
    public float gameStartDelay = 2f; // 显示介绍后等待2秒

    // 游戏状态
    public enum GameState
    {
        WaitingToStart,  // 等待开始
        ShowingIntro,    // 显示介绍（新增）
        InProgress,      // 进行中
        Completed,       // 已完成
        Failed           // 失败
    }

    private GameState currentState = GameState.WaitingToStart;
    private List<MissionPoint> activeMissionPoints = new List<MissionPoint>();
    private int completedCount = 0;
    private bool hasShownStartPrompt = false;
    private float remainingTime = 0f;
    private MissionPoint triggeredStartPoint; // 记录哪个点触发了游戏开始

    // UI 引用
    private MissionUI missionUI;

    void Start()
    {
        // 获取 UI 管理器
        missionUI = FindObjectOfType<MissionUI>();

        if (missionUI == null)
        {
            Debug.LogError("找不到 MissionUI！请在场景中添加 MissionUI。");
        }

        // 验证配置
        if (allMissionPoints == null || allMissionPoints.Length < pointsPerMission)
        {
            Debug.LogError($"任务点数量不足！需要至少 {pointsPerMission} 个任务点，当前只有 {allMissionPoints?.Length ?? 0} 个。");
            return;
        }

        if (playerTransform == null)
        {
            // 尝试查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("未设置玩家 Transform！");
            }
        }

        // 初始化：所有任务点都显示（等待状态）
        foreach (var point in allMissionPoints)
        {
            point.Show();
        }

        // 显示等待开始 UI
        if (missionUI != null)
        {
            missionUI.ShowWaitingUI();
        }

        Debug.Log($"任务系统初始化完成。所有 {allMissionPoints.Length} 个任务点都可作为开始点。");
    }

    void Update()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case GameState.WaitingToStart:
                CheckStartGame();
                break;

            case GameState.ShowingIntro:
                // 等待介绍显示完成
                break;

            case GameState.InProgress:
                UpdateTimer();
                CheckMissionPoints();
                break;

            case GameState.Completed:
            case GameState.Failed:
                // 游戏已结束，等待重新开始
                break;
        }
    }

    /// <summary>
    /// 检查是否可以开始游戏（检查所有任务点）
    /// </summary>
    void CheckStartGame()
    {
        if (allMissionPoints == null) return;

        // 遍历所有任务点，检查玩家是否靠近任意一个
        foreach (var point in allMissionPoints)
        {
            if (point.CheckPlayerInStartRange(playerTransform))
            {
                // 玩家靠近了某个任务点
                if (!hasShownStartPrompt && missionUI != null)
                {
                    missionUI.ShowStartPrompt(true);
                    hasShownStartPrompt = true;
                }

                // 记录触发游戏的任务点
                triggeredStartPoint = point;

                // 自动开始游戏（也可以改为按键触发）
                StartGame();
                return;
            }
        }

        // 玩家不在任何点的范围内
        if (hasShownStartPrompt && missionUI != null)
        {
            missionUI.ShowStartPrompt(false);
            hasShownStartPrompt = false;
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        if (currentState != GameState.WaitingToStart) return;

        // 切换到介绍状态
        currentState = GameState.ShowingIntro;

        // 随机选择任务点（但不显示UI）
        SelectRandomMissionPoints();

        // 显示开始介绍UI
        if (missionUI != null)
        {
            missionUI.ShowIntroUI(pointsPerMission, missionTimeLimit);
        }

        // 延迟后真正开始游戏
        Invoke(nameof(StartGameAfterDelay), gameStartDelay);

        Debug.Log($"显示游戏介绍，{gameStartDelay} 秒后开始游戏...");
    }

    /// <summary>
    /// 延迟后真正开始游戏
    /// </summary>
    void StartGameAfterDelay()
    {
        currentState = GameState.InProgress;
        remainingTime = missionTimeLimit; // 现在才开始倒计时

        // 显示游戏UI（任务计数和倒计时）
        if (missionUI != null)
        {
            missionUI.ShowGameUI();
            missionUI.UpdateMissionCount(completedCount, pointsPerMission);
            missionUI.UpdateTimer(remainingTime);
        }

        Debug.Log($"游戏正式开始！需要在 {missionTimeLimit} 秒内完成 {pointsPerMission} 个任务点。");
    }

    /// <summary>
    /// 随机选择任务点
    /// </summary>
    void SelectRandomMissionPoints()
    {
        // 清空之前的激活列表
        activeMissionPoints.Clear();

        // 创建可选任务点列表（排除触发游戏的那个点）
        List<MissionPoint> availablePoints = new List<MissionPoint>();
        foreach (var point in allMissionPoints)
        {
            if (point != triggeredStartPoint) // 触发游戏的点不参与任务
            {
                availablePoints.Add(point);
            }
        }

        // Fisher-Yates 洗牌算法
        for (int i = availablePoints.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            MissionPoint temp = availablePoints[i];
            availablePoints[i] = availablePoints[j];
            availablePoints[j] = temp;
        }

        // 选择前 N 个作为任务点，其他隐藏
        for (int i = 0; i < availablePoints.Count; i++)
        {
            MissionPoint point = availablePoints[i];

            if (i < pointsPerMission)
            {
                // 选中的任务点：激活
                point.Activate();
                activeMissionPoints.Add(point);
                Debug.Log($"激活任务点: {point.pointName} (ID: {point.pointID})");
            }
            else
            {
                // 未选中的任务点：隐藏
                point.Hide();
            }
        }

        // 触发游戏的点也隐藏（游戏进行中不需要显示）
        if (triggeredStartPoint != null)
        {
            triggeredStartPoint.Hide();
        }

        Debug.Log($"游戏由 {triggeredStartPoint?.pointName ?? "未知点"} 触发。已选择 {activeMissionPoints.Count} 个任务点，其余已隐藏。");
    }

    /// <summary>
    /// 更新倒计时
    /// </summary>
    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        // 更新UI显示
        if (missionUI != null)
        {
            missionUI.UpdateTimer(remainingTime);
        }

        // 检查时间是否用完
        if (remainingTime <= 0f)
        {
            FailMission();
        }
    }

    /// <summary>
    /// 检查任务点完成情况
    /// </summary>
    void CheckMissionPoints()
    {
        foreach (var point in activeMissionPoints)
        {
            if (!point.IsCompleted() && point.CheckPlayerInRange(playerTransform))
            {
                // 玩家进入范围，完成任务点
                point.Complete();
                break; // 一次只完成一个
            }
        }
    }

    /// <summary>
    /// 任务点完成回调
    /// </summary>
    public void OnPointCompleted(MissionPoint point)
    {
        completedCount++;

        Debug.Log($"完成任务点: {point.pointName} ({completedCount}/{pointsPerMission})");

        // 更新 UI
        if (missionUI != null)
        {
            missionUI.UpdateMissionCount(completedCount, pointsPerMission);
        }

        // 检查是否完成所有任务
        if (completedCount >= pointsPerMission)
        {
            CompleteMission();
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    void CompleteMission()
    {
        currentState = GameState.Completed;

        Debug.Log("任务完成！");

        // 显示完成 UI
        if (missionUI != null)
        {
            missionUI.ShowCompletedUI();
        }

        // 3秒后可以重新开始
        Invoke(nameof(ResetGame), 3f);
    }

    /// <summary>
    /// 任务失败
    /// </summary>
    void FailMission()
    {
        currentState = GameState.Failed;

        Debug.Log("任务失败！时间用完了。");

        // 显示失败 UI
        if (missionUI != null)
        {
            missionUI.ShowFailedUI();
        }

        // 3秒后可以重新开始
        Invoke(nameof(ResetGame), 3f);
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    void ResetGame()
    {
        currentState = GameState.WaitingToStart;
        completedCount = 0;
        remainingTime = 0f;
        hasShownStartPrompt = false;
        triggeredStartPoint = null;

        // 取消所有延迟调用
        CancelInvoke();

        // 重新显示所有任务点（等待状态）
        foreach (var point in allMissionPoints)
        {
            point.Show();
        }

        activeMissionPoints.Clear();

        // 显示等待 UI
        if (missionUI != null)
        {
            missionUI.ShowWaitingUI();
        }

        Debug.Log("游戏已重置，所有任务点重新显示，等待重新开始。");
    }

    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    public GameState GetGameState()
    {
        return currentState;
    }

    /// <summary>
    /// 获取剩余任务点数量
    /// </summary>
    public int GetRemainingPoints()
    {
        return pointsPerMission - completedCount;
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }

    // Gizmos 显示
    void OnDrawGizmosSelected()
    {
        // 运行时显示触发点
        if (Application.isPlaying && triggeredStartPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(triggeredStartPoint.transform.position, triggeredStartPoint.startGameDistance);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                triggeredStartPoint.transform.position + Vector3.up * 4f,
                $"任务系统管理器\n触发点: {triggeredStartPoint.pointName}\n状态: {currentState}"
            );
#endif
        }
        else
        {
            // 未运行时显示提示
#if UNITY_EDITOR
            if (allMissionPoints != null && allMissionPoints.Length > 0)
            {
                Vector3 centerPos = Vector3.zero;
                foreach (var point in allMissionPoints)
                {
                    if (point != null)
                    {
                        centerPos += point.transform.position;
                    }
                }
                centerPos /= allMissionPoints.Length;

                UnityEditor.Handles.Label(
                    centerPos + Vector3.up * 5f,
                    $"任务系统管理器\n总任务点: {allMissionPoints.Length}\n每局选择: {pointsPerMission} 个\n所有点都可作为开始点"
                );
            }
#endif
        }
    }
}