using UnityEngine;

/// <summary>
/// 找手机任务点 - 扩展原有的任务点功能
/// </summary>
public class PhoneMissionPoint : MissionPoint
{
    [Header("手机任务设置")]
    [Tooltip("是否包含手机")]
    public bool hasPhone = false;

    [Tooltip("是否已被搜索过")]
    private bool hasBeenSearched = false;

    /// <summary>
    /// 搜索此任务点（寻找手机）
    /// </summary>
    public bool SearchForPhone()
    {
        if (hasBeenSearched)
        {
            return false; // 已经搜索过
        }

        hasBeenSearched = true;

        if (hasPhone)
        {
            Debug.Log($"[{pointName}] 找到了手机！");
            return true;
        }
        else
        {
            Debug.Log($"[{pointName}] 没有找到手机...");
            return false;
        }
    }

    /// <summary>
    /// 重置搜索状态
    /// </summary>
    public void ResetSearch()
    {
        hasBeenSearched = false;
        hasPhone = false;
    }

    /// <summary>
    /// 是否已被搜索
    /// </summary>
    public bool HasBeenSearched()
    {
        return hasBeenSearched;
    }
}