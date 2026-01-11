using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务UI管理器 - 控制所有UI显示
/// </summary>
public class MissionUI : MonoBehaviour
{
    [Header("UI面板")]
    [Tooltip("等待开始UI面板")]
    public GameObject waitingPanel;

    [Tooltip("游戏介绍UI面板")]
    public GameObject introPanel;

    [Tooltip("游戏进行中UI面板")]
    public GameObject gamePanel;

    [Tooltip("任务完成UI面板")]
    public GameObject completedPanel;

    [Tooltip("任务失败UI面板")]
    public GameObject failedPanel;

    [Header("文本组件")]
    [Tooltip("开始提示文本")]
    public Text startPromptText;

    [Tooltip("介绍标题文本")]
    public Text introTitleText;

    [Tooltip("介绍内容文本")]
    public Text introContentText;

    [Tooltip("任务计数文本")]
    public Text missionCountText;

    [Tooltip("倒计时文本")]
    public Text timerText;

    [Tooltip("完成提示文本")]
    public Text completedText;

    [Tooltip("失败提示文本")]
    public Text failedText;

    [Header("提示设置")]
    [Tooltip("开始提示内容")]
    public string startPromptMessage = "靠近开始游戏";

    [Tooltip("完成提示内容")]
    public string completedMessage = "任务完成！";

    [Tooltip("失败提示内容")]
    public string failedMessage = "任务失败！\n时间用完了";

    [Header("倒计时颜色")]
    [Tooltip("正常时间颜色")]
    public Color normalTimeColor = Color.white;

    [Tooltip("警告时间颜色（最后30秒）")]
    public Color warningTimeColor = Color.yellow;

    [Tooltip("危险时间颜色（最后10秒）")]
    public Color dangerTimeColor = Color.red;

    void Start()
    {
        // 初始化：显示等待UI
        ShowWaitingUI();
    }

    /// <summary>
    /// 显示等待开始UI
    /// </summary>
    public void ShowWaitingUI()
    {
        SetPanelActive(waitingPanel, true);
        SetPanelActive(introPanel, false);
        SetPanelActive(gamePanel, false);
        SetPanelActive(completedPanel, false);
        SetPanelActive(failedPanel, false);

        // 隐藏开始提示
        ShowStartPrompt(false);
    }

    /// <summary>
    /// 显示游戏介绍UI
    /// </summary>
    public void ShowIntroUI(int totalPoints, float timeLimit)
    {
        SetPanelActive(waitingPanel, false);
        SetPanelActive(introPanel, true);
        SetPanelActive(gamePanel, false);
        SetPanelActive(completedPanel, false);
        SetPanelActive(failedPanel, false);

        // 设置介绍文本
        if (introTitleText != null)
        {
            introTitleText.text = "游戏开始！";
        }

        if (introContentText != null)
        {
            int minutes = Mathf.FloorToInt(timeLimit / 60f);
            int seconds = Mathf.FloorToInt(timeLimit % 60f);

            introContentText.text = $"任务目标：完成 {totalPoints} 个任务点\n" +
                                   $"时间限制：{minutes:00}:{seconds:00}\n\n" +
                                   $"骑车到红色任务点进行打卡\n" +
                                   $"完成所有任务点即可获胜！";
        }
    }

    /// <summary>
    /// 显示游戏UI
    /// </summary>
    public void ShowGameUI()
    {
        SetPanelActive(waitingPanel, false);
        SetPanelActive(introPanel, false);
        SetPanelActive(gamePanel, true);
        SetPanelActive(completedPanel, false);
        SetPanelActive(failedPanel, false);
    }

    /// <summary>
    /// 显示完成UI
    /// </summary>
    public void ShowCompletedUI()
    {
        SetPanelActive(waitingPanel, false);
        SetPanelActive(introPanel, false);
        SetPanelActive(gamePanel, false);
        SetPanelActive(completedPanel, true);
        SetPanelActive(failedPanel, false);

        if (completedText != null)
        {
            completedText.text = completedMessage;
        }
    }

    /// <summary>
    /// 显示失败UI
    /// </summary>
    public void ShowFailedUI()
    {
        SetPanelActive(waitingPanel, false);
        SetPanelActive(introPanel, false);
        SetPanelActive(gamePanel, false);
        SetPanelActive(completedPanel, false);
        SetPanelActive(failedPanel, true);

        if (failedText != null)
        {
            failedText.text = failedMessage;
        }
    }

    /// <summary>
    /// 显示/隐藏开始提示
    /// </summary>
    public void ShowStartPrompt(bool show)
    {
        if (startPromptText != null)
        {
            startPromptText.gameObject.SetActive(show);
            if (show)
            {
                startPromptText.text = startPromptMessage;
            }
        }
    }

    /// <summary>
    /// 更新任务计数
    /// </summary>
    public void UpdateMissionCount(int completed, int total)
    {
        if (missionCountText != null)
        {
            missionCountText.text = $"Remaining Task Points: {total - completed}/{total}";
        }
    }

    /// <summary>
    /// 更新倒计时显示
    /// </summary>
    public void UpdateTimer(float remainingTime)
    {
        if (timerText == null) return;

        // 格式化时间显示（分:秒）
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = string.Format("剩余时间: {0:00}:{1:00}", minutes, seconds);

        // 根据剩余时间改变颜色
        if (remainingTime <= 10f)
        {
            // 最后10秒：红色
            timerText.color = dangerTimeColor;
        }
        else if (remainingTime <= 30f)
        {
            // 最后30秒：黄色
            timerText.color = warningTimeColor;
        }
        else
        {
            // 正常时间：白色
            timerText.color = normalTimeColor;
        }
    }

    /// <summary>
    /// 辅助方法：设置面板激活状态
    /// </summary>
    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}