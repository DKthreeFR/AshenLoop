# Unity Addressables 通用加载器（统一进度条）

下面给你一版可直接用的“通用加载器”：

- 支持任意资源列表（`AssetReference`）
- 自动权重（按下载体积 `GetDownloadSizeAsync`）
- 失败重试（场景和资源都可重试）
- 一个统一进度值（`0~1`）
- 等“场景+资源全部完成”后再激活场景

## 1) `LevelLoadConfig.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LevelLoadConfig", menuName = "Game/Level Load Config")]
public class LevelLoadConfig : ScriptableObject
{
    [Header("Target Scene")]
    public AssetReference scene;

    [Header("Required Assets In Scene (player/UI/config/etc.)")]
    public List<AssetReference> requiredAssets = new();

    [Header("Retry")]
    [Min(0)] public int maxRetry = 2;
    [Min(0f)] public float retryDelaySeconds = 0.5f;
}
```

## 2) `UnifiedAddressablesLoader.cs`

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class UnifiedAddressablesLoader : MonoBehaviour
{
    public float Progress { get; private set; } // 0~1
    public bool IsLoading { get; private set; }

    public SceneInstance LoadedScene { get; private set; }
    public IReadOnlyDictionary<string, UnityEngine.Object> LoadedAssets => _loadedAssets;

    private readonly Dictionary<string, UnityEngine.Object> _loadedAssets = new();

    private class OpNode
    {
        public string id;
        public AssetReference reference;
        public bool isScene;
        public int maxRetry;
        public float retryDelay;
        public float weight = 1f;
        public float progress;
        public bool success;
        public bool done;
        public string error;
        public int attempts;
        public AsyncOperationHandle<SceneInstance> sceneHandle;
        public AsyncOperationHandle<UnityEngine.Object> assetHandle;
        public SceneInstance sceneResult;
        public UnityEngine.Object assetResult;
    }

    public IEnumerator Load(
        LevelLoadConfig config,
        Action<float> onProgress = null,
        Action onCompleted = null,
        Action<string> onFailed = null)
    {
        if (IsLoading)
        {
            onFailed?.Invoke("Loader is already running.");
            yield break;
        }

        if (config == null || config.scene == null)
        {
            onFailed?.Invoke("Invalid LevelLoadConfig.");
            yield break;
        }

        IsLoading = true;
        Progress = 0f;
        _loadedAssets.Clear();

        // 1) Build op list: scene + required assets
        List<OpNode> nodes = new();
        nodes.Add(new OpNode
        {
            id = "scene",
            reference = config.scene,
            isScene = true,
            maxRetry = config.maxRetry,
            retryDelay = config.retryDelaySeconds
        });

        for (int i = 0; i < config.requiredAssets.Count; i++)
        {
            AssetReference ar = config.requiredAssets[i];
            if (ar == null) continue;

            nodes.Add(new OpNode
            {
                id = $"asset_{i}_{ar.RuntimeKey}",
                reference = ar,
                isScene = false,
                maxRetry = config.maxRetry,
                retryDelay = config.retryDelaySeconds
            });
        }

        // 2) Auto weights by download size (fallback equal when all zero)
        yield return StartCoroutine(CalcWeights(nodes));

        // 3) Start all operations in parallel (each has retry)
        int completedCount = 0;
        bool anyFailed = false;

        foreach (var node in nodes)
        {
            StartCoroutine(RunNodeWithRetry(node, () =>
            {
                completedCount++;
                if (!node.success) anyFailed = true;
            }));
        }

        // 4) Unified progress loop
        while (completedCount < nodes.Count)
        {
            float p = 0f;
            for (int i = 0; i < nodes.Count; i++)
            {
                p += nodes[i].progress * nodes[i].weight;
            }

            Progress = Mathf.Clamp01(p);
            onProgress?.Invoke(Progress);
            yield return null;
        }

        // 5) Check result before activation
        if (anyFailed)
        {
            string err = BuildError(nodes);
            IsLoading = false;
            onFailed?.Invoke(err);
            yield break;
        }

        // 6) Activate scene after everything ready
        OpNode sceneNode = nodes[0];
        if (sceneNode.sceneHandle.IsValid())
        {
            var activate = sceneNode.sceneResult.ActivateAsync();
            while (!activate.isDone)
            {
                // Keep progress visually near end during activation
                Progress = Mathf.Lerp(0.95f, 1f, activate.progress);
                onProgress?.Invoke(Progress);
                yield return null;
            }
        }

        Progress = 1f;
        onProgress?.Invoke(Progress);
        IsLoading = false;
        onCompleted?.Invoke();
    }

    private IEnumerator CalcWeights(List<OpNode> nodes)
    {
        long totalBytes = 0;
        long[] sizes = new long[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            var h = Addressables.GetDownloadSizeAsync(nodes[i].reference);
            yield return h;

            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                sizes[i] = Math.Max(0, h.Result);
                totalBytes += sizes[i];
            }

            Addressables.Release(h);
        }

        if (totalBytes <= 0)
        {
            float equal = 1f / nodes.Count;
            for (int i = 0; i < nodes.Count; i++) nodes[i].weight = equal;
            yield break;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].weight = (float)sizes[i] / totalBytes;
        }
    }

    private IEnumerator RunNodeWithRetry(OpNode node, Action onDone)
    {
        node.done = false;
        node.success = false;
        node.progress = 0f;
        node.error = null;

        int maxAttempts = Mathf.Max(1, node.maxRetry + 1);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            node.attempts = attempt;
            node.progress = 0f;

            if (node.isScene)
            {
                node.sceneHandle = Addressables.LoadSceneAsync(node.reference, LoadSceneMode.Single, activateOnLoad: false);

                while (!node.sceneHandle.IsDone)
                {
                    node.progress = node.sceneHandle.PercentComplete;
                    yield return null;
                }

                if (node.sceneHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    node.sceneResult = node.sceneHandle.Result;
                    node.progress = 1f;
                    node.success = true;
                    node.done = true;
                    onDone?.Invoke();
                    yield break;
                }

                node.error = $"Scene load failed: {node.reference.RuntimeKey}";
            }
            else
            {
                node.assetHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(node.reference);

                while (!node.assetHandle.IsDone)
                {
                    node.progress = node.assetHandle.PercentComplete;
                    yield return null;
                }

                if (node.assetHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    node.assetResult = node.assetHandle.Result;
                    _loadedAssets[node.reference.RuntimeKey.ToString()] = node.assetResult;
                    node.progress = 1f;
                    node.success = true;
                    node.done = true;
                    onDone?.Invoke();
                    yield break;
                }

                node.error = $"Asset load failed: {node.reference.RuntimeKey}";
            }

            // failed this attempt
            if (attempt < maxAttempts && node.retryDelay > 0f)
                yield return new WaitForSeconds(node.retryDelay);
        }

        node.progress = 1f; // prevent progress freeze when failed
        node.done = true;
        node.success = false;
        onDone?.Invoke();
    }

    private string BuildError(List<OpNode> nodes)
    {
        List<string> errors = new();
        foreach (var n in nodes)
        {
            if (!n.success)
                errors.Add($"{n.id} failed after {n.attempts} attempt(s). {n.error}");
        }
        return string.Join("\n", errors);
    }

    // 可选：切场景后或退出关卡时释放资源
    public void ReleaseLoadedAssets()
    {
        foreach (var kv in _loadedAssets)
        {
            // 这里只释放由 Addressables.LoadAssetAsync 得到的对象引用
            Addressables.Release(kv.Value);
        }
        _loadedAssets.Clear();
    }
}
```

## 3) 用法示例

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingEntry : MonoBehaviour
{
    public LevelLoadConfig config;
    public UnifiedAddressablesLoader loader;
    public Slider progressBar;

    private void Start()
    {
        StartCoroutine(BeginLoad());
    }

    private IEnumerator BeginLoad()
    {
        yield return loader.Load(
            config,
            onProgress: p => progressBar.value = p,
            onCompleted: () =>
            {
                Debug.Log("All ready. Scene activated.");
                // 例如取玩家预制体（key 就是 RuntimeKey.ToString()）
                // var player = loader.LoadedAssets[config.requiredAssets[0].RuntimeKey.ToString()] as GameObject;
                // Instantiate(player);
            },
            onFailed: err => Debug.LogError(err)
        );
    }
}
```

如果你要，我下一步可以给你再加两项：

1. `requiredAssets` 分“只加载”和“自动实例化”两种模式  
2. 失败后切到错误页并支持“重试按钮”完整流程
