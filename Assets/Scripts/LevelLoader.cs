using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 虽然我们不用 Slider，但可能需要用到 Image
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("核心设置")]
    // 确保你的主菜单场景名字是 MainMenu
    public string sceneToLoad = "MainMenu"; 
    public float minLoadingTime = 2.0f; // 最少显示多久，避免进度条一闪而过

    [Header("UI 绑定")]
    // 进度条填充物体的 RectTransform 组件
    public RectTransform progressBarFillRect; 
    public TMP_Text progressText; 
    
    // 存储进度条的最大宽度，用于计算百分比对应的长度
    private float maxWidth; 

    void Start()
    {
        // === 【修复关键点 1：启动加载和获取最大宽度】 ===
        
        // 1. 确保 RectTransform 已绑定并计算最大宽度
        if (progressBarFillRect != null)
        {
            // 获取父物体 (底图) 的宽度作为最大宽度
            // 确保你的 ProgressBarFill 是放在底图 (BackgroundBar) 下的子物体
            if (progressBarFillRect.parent != null)
            {
                 // 注意：如果父物体使用了锚点拉伸，使用 parent.rect.width 更安全
                maxWidth = progressBarFillRect.parent.GetComponent<RectTransform>().rect.width;
            }
            
            // 2. 启动加载协程
            Debug.Log("计算出的最大宽度 MaxWidth: " + maxWidth);
            StartCoroutine(LoadLevelAsync());
        }
        else
        {
            Debug.LogError("ProgressBarFillRect 未绑定！无法开始加载。");
        }
    }
    
    private float minVisualOffset = 0.1f; // 设置最小视觉偏移量为 10%

    IEnumerator LoadLevelAsync()
    {
        // 确保你的目标场景名字是正确的
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;
        
        float timer = 0f;
        
        while (!operation.isDone)
        {
            timer += Time.deltaTime;
            // 实际加载进度 (0.0 ~ 1.0)
            float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
        
            // 视觉进度：防止加载过快，取 "实际进度" 和 "时间流逝进度" 中较小的
            float visualProgress = Mathf.Min(actualProgress, timer / minLoadingTime);

        
            // --- 【新增关键逻辑：应用 10% 偏移】 ---
            // 将 0.0~1.0 的进度范围，映射到 0.1~1.0 的视觉范围
            // 公式: 最小偏移 + (原始进度 * (1.0 - 最小偏移))
            float finalVisualProgress = minVisualOffset + (visualProgress * (1.0f - minVisualOffset));


            // --- 进度条宽度更新逻辑 ---
            if (progressBarFillRect != null)
            {
                // 使用最终进度控制宽度
                float newWidth = maxWidth * finalVisualProgress; 
                progressBarFillRect.sizeDelta = new Vector2(newWidth, progressBarFillRect.sizeDelta.y);
            }
        
            // --- 百分比文字更新逻辑 ---
            if (progressText != null)
            {
                // 使用最终进度转换为百分比显示
                progressText.text = (finalVisualProgress * 100).ToString("F0") + "%";
            }


            // --- 场景激活逻辑（保持不变，仍使用实际加载进度 actualProgress）---
            if (actualProgress >= 1f && timer >= minLoadingTime)
            {
                // 确保进度条显示 100% 的最终状态
                if (progressBarFillRect != null)
                {
                    progressBarFillRect.sizeDelta = new Vector2(maxWidth, progressBarFillRect.sizeDelta.y);
                }
                if (progressText != null)
                {
                    progressText.text = "100%";
                }
            
                operation.allowSceneActivation = true;
                break;
            }

            yield return null;
        }
    }
}