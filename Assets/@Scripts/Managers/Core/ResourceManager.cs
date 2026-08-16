using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    // 실제 로드한 리소스.
    Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

    #region 리소스 로드
    public T Load<T>(string key) where T : Object
    {
        // ⚠️ 타입이 맞을 때만 반환한다. 같은 주소에 **종류가 다른 에셋**이 있을 수 있다 —
        //  고블린은 애니메이터 주소도 "Mon_2003"이라, 캐시에서 애니메이터가 먼저 잡히고
        //  as Sprite가 null이 된 채 반환돼 아래 ".sprite" 조회로 넘어가지 못했다(도감 카드 로드 실패).
        //  타입이 안 맞으면 못 찾은 셈 치고 계속 찾는다.
        if (_resources.TryGetValue(key, out Object resource) && resource is T typed)
        {
            return typed;
        }

        //스프라이트 로드할때 항상 .sprite가 붙어 있어야하는데 데이터시트에 .sprite가 붙어있지 않은 데이터가 많음
        //임시로 붙임 -드래곤
        if (typeof(T) == typeof(Sprite))
        {
            key = key + ".sprite";
            if (_resources.TryGetValue(key, out Object temp))
            {
                return temp as T;
            }
        }

        // 캐시에 없으면 즉시 로드(동기) 후 캐싱.
        //  PreLoad에 전부 밀어넣으면 시작 로딩이 에셋 수에 비례해 길어진다(몬스터 애니메이터 503개를
        //  넣었을 때 30초 이상). 자주 안 쓰는 대형 에셋은 PreLoad에서 빼고 여기서 필요할 때 받는다.
        return LoadSync<T>(key);
    }

    // 주소가 없거나 타입이 안 맞으면 null. 실패도 캐싱해 매 프레임 재시도하지 않는다.
    HashSet<string> _missing = new HashSet<string>();

    T LoadSync<T>(string key) where T : Object
    {
        if (string.IsNullOrEmpty(key) || _missing.Contains(key))
            return null;

        try
        {
            string loadKey = key;
            if (typeof(T) == typeof(Sprite) && key.Contains(".sprite"))
                loadKey = $"{key}[{key.Replace(".sprite", "")}]";

            // 주소가 없으면 **로드를 시도하지 않는다.**
            //  Addressables는 미등록 키를 로드하면 InvalidKeyException을 콘솔에 빨간 로그로 찍은 뒤 던진다.
            //  try/catch로 삼켜도 로그는 이미 출력된 뒤라 플레이 중 빨간 오류가 계속 쌓인다.
            //  (실제로 item_rabijelly / FX_Combo_Break / Active Ef 등에서 반복 발생했다)
            var locOp = Addressables.LoadResourceLocationsAsync(loadKey, typeof(T));
            var locs = locOp.WaitForCompletion();
            bool exists = locs != null && locs.Count > 0;
            Addressables.Release(locOp);
            if (!exists)
            {
                _missing.Add(key);
                return null;
            }

            var op = Addressables.LoadAssetAsync<T>(loadKey);
            T result = op.WaitForCompletion();
            if (result == null)
            {
                _missing.Add(key);
                return null;
            }
            if (!_resources.ContainsKey(key))
                _resources.Add(key, result);
            return result;
        }
        catch (Exception)
        {
            _missing.Add(key);   // 미등록 주소 — 조용히 실패(호출부가 null 처리)
            return null;
        }
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
            return Managers.Pool.Pop(prefab);

        GameObject go = Object.Instantiate(prefab, parent);

        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        if (Managers.Pool.Push(go))
            return;

        Object.Destroy(go);
    }

    #endregion
    #region 어드레서블

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        //스프라이트인 경우 하위객체의 찐이름으로 로드하면 스프라이트로 로딩이 됌.
        // 단, 실제 Sprite 로드일 때만 [서브이름] 변환 — Material 등이 .sprite 주소로 잘못 등록된 경우
        // 변환하면 존재하지 않는 서브에셋을 요청해 InvalidKeyException이 나므로 raw 주소로 로드.
        string loadKey = key;
        if (typeof(T) == typeof(Sprite) && key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) =>
        {
            // 잘못 등록된 엔트리(타입 불일치 / 존재하지 않는 서브스프라이트 등)는 캐시에 넣지 않는다.
            // 단 콜백은 반드시 호출한다 — LoadAllAsync가 콜백 횟수로 진행도를 세고,
            // count == totalCount 시점에 Managers.Data.Init()이 돌기 때문. 여기서 return해버리면
            // 실패한 에셋 하나 때문에 카운터가 영영 안 차고 **모든 데이터 딕셔너리가 빈 채로** 게임이 시작된다.
            if (op.Status != AsyncOperationStatus.Succeeded || op.Result == null)
            {
                Debug.LogWarning($"[ResourceManager] 로드 실패 — 캐시 제외: '{key}' (주소/타입 등록 확인 필요)");
                callback?.Invoke(null);
                return;
            }

            // 캐시 확인.
            if (_resources.TryGetValue(key, out Object resource))
            {
                callback?.Invoke(op.Result);
                return;
            }

            _resources.Add(key, op.Result);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;

            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                if (result.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }

    #endregion
}
