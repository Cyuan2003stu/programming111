using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 找手机任务管理器 - 管理整个找手机任务流程
/// </summary>
public class PhoneMissionManager : MonoBehaviour
{
   
    [Tooltip("所有5个任务点")]
    public PhoneMissionPoint[] allPhonePoints;

    [Tooltip("玩家Transform")]
    public Transform playerTransform;

    [Tooltip("触发范围")]
    public float triggerDistance = 3f;



    public enum MissionState
    {
        WaitingToStart,     // 等待开始
        ShowingDialog,      // 显示对话
        Searching,          // 搜索手机中
        FoundPhone,         // 找到手机
        ReturningToStart,   // 返回起点
        Completed           // 完成
    }

    public MissionState currentState = MissionState.WaitingToStart;

    private PhoneMissionPoint startPoint;           // 触发任务的起点
    private PhoneMissionPoint phoneLocation;        // 手机所在位置
    private List<PhoneMissionPoint> searchPoints;   // 需要搜索的3个点
    private PhoneMissionUI missionUI;

    void Start()
    {
        missionUI = FindObjectOfType<PhoneMissionUI>();
        searchPoints = new List<PhoneMissionPoint>();

        if (allPhonePoints == null || allPhonePoints.Length != 5)
        {
            Debug.LogError("需要正好5个PhoneMissionPoint！");
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // 初始化：所有点都可见
        foreach (var point in allPhonePoints)
        {
            point.Show();
        }

        Debug.Log("找手机任务初始化完成");
    }

    void Update()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case MissionState.WaitingToStart:
                CheckForMissionStart();
                break;

            case MissionState.ShowingDialog:
                // 等待玩家关闭对话
                break;

            case MissionState.Searching:
                CheckSearchPoints();
                break;

            case MissionState.FoundPhone:
                // 显示找到手机UI，等待玩家确认
                break;

            case MissionState.ReturningToStart:
                CheckReturnToStart();
                break;

            case MissionState.Completed:
                // 任务完成
                break;
        }
    }

    /// <summary>
    /// 检查是否触发任务
    /// </summary>
    void CheckForMissionStart()
    {
        foreach (var point in allPhonePoints)
        {
            float distance = Vector3.Distance(playerTransform.position, point.transform.position);

            if (distance <= triggerDistance)
            {
                // 触发任务
                StartMission(point);
                break;
            }
        }
    }

    /// <summary>
    /// 开始任务
    /// </summary>
    void StartMission(PhoneMissionPoint triggerPoint)
    {
        currentState = MissionState.ShowingDialog;
        startPoint = triggerPoint;

        // 选择3个搜索点（不包括起点）
        SelectSearchPoints();

        // 显示对话UI
        if (missionUI != null)
        {
            missionUI.ShowDialog();
        }

        Debug.Log($"任务在 {startPoint.pointName} 触发！");
    }

    /// <summary>
    /// 选择3个搜索点
    /// </summary>
    void SelectSearchPoints()
    {
        searchPoints.Clear();

        // 创建可选点列表（排除起点）
        List<PhoneMissionPoint> availablePoints = new List<PhoneMissionPoint>();
        foreach (var point in allPhonePoints)
        {
            if (point != startPoint)
            {
                point.ResetSearch();
                availablePoints.Add(point);
            }
        }

        // 随机打乱
        for (int i = availablePoints.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PhoneMissionPoint temp = availablePoints[i];
            availablePoints[i] = availablePoints[j];
            availablePoints[j] = temp;
        }

        // 选择前3个
        for (int i = 0; i < 3 && i < availablePoints.Count; i++)
        {
            searchPoints.Add(availablePoints[i]);
        }

        // 隐藏剩余2个点
        foreach (var point in availablePoints)
        {
            if (!searchPoints.Contains(point))
            {
                point.Hide();
            }
        }

        // 随机选一个放手机
        if (searchPoints.Count > 0)
        {
            int phoneIndex = Random.Range(0, searchPoints.Count);
            phoneLocation = searchPoints[phoneIndex];
            phoneLocation.hasPhone = true;

            Debug.Log($"手机在 {phoneLocation.pointName}");
        }

        // 隐藏起点
        startPoint.Hide();
    }

    /// <summary>
    /// 对话确认后开始搜索
    /// </summary>
    public void OnDialogConfirmed()
    {
        currentState = MissionState.Searching;

        // 更新UI显示搜索进度
        if (missionUI != null)
        {
            missionUI.ShowSearchUI(0, searchPoints.Count);
        }
    }

    /// <summary>
    /// 检查搜索点
    /// </summary>
    void CheckSearchPoints()
    {
        foreach (var point in searchPoints)
        {
            if (point.HasBeenSearched()) continue;

            float distance = Vector3.Distance(playerTransform.position, point.transform.position);

            if (distance <= triggerDistance)
            {
                // 搜索此点
                bool foundPhone = point.SearchForPhone();

                if (foundPhone)
                {
                    // 找到手机！
                    OnPhoneFound();
                }
                else
                {
                    // 没找到，显示提示
                    if (missionUI != null)
                    {
                        int searchedCount = GetSearchedCount();
                        missionUI.ShowNotFoundMessage();
                        missionUI.UpdateSearchProgress(searchedCount, searchPoints.Count);
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// 找到手机
    /// </summary>
    void OnPhoneFound()
    {
        currentState = MissionState.FoundPhone;

        // 显示找到手机UI
        if (missionUI != null)
        {
            missionUI.ShowFoundPhoneUI();
        }

        Debug.Log("找到手机了！");
    }

    /// <summary>
    /// 确认找到手机后，开始返回
    /// </summary>
    public void OnFoundPhoneConfirmed()
    {
        currentState = MissionState.ReturningToStart;

        // 隐藏所有搜索点
        foreach (var point in searchPoints)
        {
            point.Hide();
        }

        // 显示起点
        startPoint.Show();

        // 更新UI
        if (missionUI != null)
        {
            missionUI.ShowReturnUI();
        }

        Debug.Log($"返回 {startPoint.pointName}");
    }

    /// <summary>
    /// 检查是否返回起点
    /// </summary>
    void CheckReturnToStart()
    {
        float distance = Vector3.Distance(playerTransform.position, startPoint.transform.position);

        if (distance <= triggerDistance)
        {
            // 返回成功，任务完成
            CompleteMission();
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    void CompleteMission()
    {
        currentState = MissionState.Completed;

        // 显示完成UI
        if (missionUI != null)
        {
            missionUI.ShowCompletedUI();
        }

        Debug.Log("任务完成！");

        // 3秒后重置
        Invoke(nameof(ResetMission), 3f);
    }

    /// <summary>
    /// 重置任务
    /// </summary>
    void ResetMission()
    {
        currentState = MissionState.WaitingToStart;

        // 显示所有点
        foreach (var point in allPhonePoints)
        {
            point.Show();
            point.ResetSearch();
        }

        searchPoints.Clear();
        startPoint = null;
        phoneLocation = null;

        // 隐藏UI
        if (missionUI != null)
        {
            missionUI.HideAllUI();
        }

        Debug.Log("任务已重置");
    }

    /// <summary>
    /// 获取已搜索的点数量
    /// </summary>
    int GetSearchedCount()
    {
        int count = 0;
        foreach (var point in searchPoints)
        {
            if (point.HasBeenSearched())
            {
                count++;
            }
        }
        return count;
    }

    void OnDrawGizmosSelected()
    {
        if (allPhonePoints == null) return;

        foreach (var point in allPhonePoints)
        {
            if (point == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point.transform.position, triggerDistance);
        }

        if (Application.isPlaying && phoneLocation != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(phoneLocation.transform.position, triggerDistance * 1.5f);
        }
    }
}