using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 找手机任务UI管理器 - 无按钮版本，自动推进
/// </summary>
public class PhoneMissionUI : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject dialogPanel;          // 对话面板
    public GameObject searchPanel;          // 搜索进度面板
    public GameObject foundPhonePanel;      // 找到手机面板
    public GameObject returnPanel;          // 返回提示面板
    public GameObject completedPanel;       // 完成面板
    public GameObject notFoundPanel;        // 未找到提示面板

    [Header("文本组件")]
    public Text dialogText;                 // 对话内容
    public Text searchProgressText;         // 搜索进度
    public Text foundPhoneText;             // 找到手机文本
    public Text returnText;                 // 返回提示文本
    public Text completedText;              // 完成文本
    public Text notFoundText;               // 未找到文本

    [Header("自动关闭设置")]
    [Tooltip("对话显示时长（秒）")]
    public float dialogDuration = 3f;

    [Tooltip("找到手机UI显示时长（秒）")]
    public float foundPhoneDuration = 2f;

    private PhoneMissionManager missionManager;

    void Start()
    {
        missionManager = FindObjectOfType<PhoneMissionManager>();

        // 初始隐藏所有UI
        HideAllUI();
    }

    /// <summary>
    /// 显示对话UI - 自动关闭
    /// </summary>
    public void ShowDialog()
    {
        HideAllUI();

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }

        if (dialogText != null)
        {
            dialogText.text = "Sorry! Ilost my phone\n" +
                            "Can you help me?\n\n" +
                            "It might be in these place";
        }

        // 自动关闭并开始搜索
        Invoke(nameof(AutoConfirmDialog), dialogDuration);
    }

    /// <summary>
    /// 自动确认对话
    /// </summary>
    void AutoConfirmDialog()
    {
        if (missionManager != null)
        {
            missionManager.OnDialogConfirmed();
        }
    }

    /// <summary>
    /// 显示搜索UI
    /// </summary>
    public void ShowSearchUI(int searched, int total)
    {
        HideAllUI();

        if (searchPanel != null)
        {
            searchPanel.SetActive(true);
        }

        UpdateSearchProgress(searched, total);
    }

    /// <summary>
    /// 更新搜索进度
    /// </summary>
    public void UpdateSearchProgress(int searched, int total)
    {
        if (searchProgressText != null)
        {
            searchProgressText.text = "Finding phone...\n" +
                                     "Found: " + searched + "/" + total;
        }
    }

    /// <summary>
    /// 显示未找到提示
    /// </summary>
    public void ShowNotFoundMessage()
    {
        if (notFoundPanel != null)
        {
            notFoundPanel.SetActive(true);
        }

        if (notFoundText != null)
        {
            notFoundText.text = "There's no phone...\nKeep finding！";
        }

        // 2秒后自动隐藏
        Invoke(nameof(HideNotFoundPanel), 2f);
    }

    void HideNotFoundPanel()
    {
        if (notFoundPanel != null)
        {
            notFoundPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示找到手机UI - 自动关闭
    /// </summary>
    public void ShowFoundPhoneUI()
    {
        HideAllUI();

        if (foundPhonePanel != null)
        {
            foundPhonePanel.SetActive(true);
        }

        if (foundPhoneText != null)
        {
            foundPhoneText.text = "Great！\nI find the phone！\n\n" +
                                 "Please return the phone to the owner";
        }

        // 自动关闭并开始返回
        Invoke(nameof(AutoConfirmFoundPhone), foundPhoneDuration);
    }

    /// <summary>
    /// 自动确认找到手机
    /// </summary>
    void AutoConfirmFoundPhone()
    {
        if (missionManager != null)
        {
            missionManager.OnFoundPhoneConfirmed();
        }
    }

    /// <summary>
    /// 显示返回UI
    /// </summary>
    public void ShowReturnUI()
    {
        HideAllUI();

        if (returnPanel != null)
        {
            returnPanel.SetActive(true);
        }

        if (returnText != null)
        {
            returnText.text = "Return to the start point\n Return the phone to the owner";
        }
    }

    /// <summary>
    /// 显示完成UI
    /// </summary>
    public void ShowCompletedUI()
    {
        HideAllUI();

        if (completedPanel != null)
        {
            completedPanel.SetActive(true);
        }

        if (completedText != null)
        {
            completedText.text = "Mission complete！\n\n" +
                               "Thank you for finding my phone！";
        }
    }

    /// <summary>
    /// 隐藏所有UI
    /// </summary>
    public void HideAllUI()
    {
        // 取消所有延迟调用，避免冲突
        CancelInvoke();

        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (searchPanel != null) searchPanel.SetActive(false);
        if (foundPhonePanel != null) foundPhonePanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
        if (completedPanel != null) completedPanel.SetActive(false);
        if (notFoundPanel != null) notFoundPanel.SetActive(false);
    }
}