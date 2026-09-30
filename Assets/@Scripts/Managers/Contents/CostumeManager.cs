using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using CostumeData = Data.CostumeData;

// v2 의상(치장, ADR0043, 설계서 4.16) — 새 그림 없이 주인공 그림을 부위별로 다시 칠한 8벌(CostumeData, _tools/gen_costumes.py).
//  부위는 색으로 가른다(Define COSTUME_*): 가죽·천(색상 이동·채도·밝기) / 흰 옷·머리·강철(Tint 곱) / 그 밖(살갗·바지·외곽선)은 그대로.
//  얻는 법: 기본 · 크리스탈 · 레벨 · 시련 단계 · 심연 최고 층. 능력치는 없다(설계서 5장 치장 — 칭호와 같은 원칙, 돈으로 힘을 사지 않는다).
//  칠하기: 애니메이터가 매 프레임 끼우는 원본 스프라이트를 PlayerController.LateUpdate 가 Skin() 사본으로 바꾼다.
//   시트(텍스처)는 처음 보일 때 한 장씩 GPU 로 복사해 읽고 칠한다(원본은 읽기 불가 — 임포트 설정을 바꾸지 않는다).
//  입는 곳은 미션 창 의상 탭(UI_MissionPopup), HUD 초상화도 같은 색으로(UI_GameScene → Portrait·Preview).
//  세이브: GameData.Costumes(산 것 + 달성해 알린 것) · CostumeId(입은 것).
public class CostumeManager
{
    public const string UNLOCK_DEFAULT   = "Default";
    public const string UNLOCK_CRYSTAL   = "Crystal";
    public const string UNLOCK_LEVEL     = "Level";
    public const string UNLOCK_PROMOTION = "Promotion";
    public const string UNLOCK_ABYSS     = "Abyss";

    public event Action OnChanged;

    #region 목록 · 얻기
    // 데이터는 씬마다 다시 읽힐 수 있다(GameScene 이 에디터에서 Data.Init 을 한 번 더) — 사전이 바뀌면 목록도 새로
    List<CostumeData> _list;
    Dictionary<int, CostumeData> _listSource;
    public List<CostumeData> List
    {
        get
        {
            var dic = Managers.Data?.CostumeDic;
            if (dic == null) return _list ??= new List<CostumeData>();
            if (_list == null || _listSource != dic)
            {
                _listSource = dic;
                _list = new List<CostumeData>(dic.Values);
                _list.Sort((a, b) => a.DataId.CompareTo(b.DataId));
            }
            return _list;
        }
    }

    public bool IsOwned(CostumeData c)
    {
        var g = Managers.Game;
        if (c == null || g == null) return false;
        switch (c.Unlock)
        {
            case UNLOCK_DEFAULT:   return true;
            case UNLOCK_CRYSTAL:   return g.Costumes.Contains(c.DataId);
            case UNLOCK_LEVEL:     return g.Level >= c.UnlockValue;
            case UNLOCK_PROMOTION: return g.PromotionTier >= c.UnlockValue;
            case UNLOCK_ABYSS:     return g.AbyssBest >= c.UnlockValue;
        }
        return false;
    }

    // 입은 의상 — 고른 적이 없거나 못 가진 것(손댄 세이브)이면 기본
    public CostumeData Equipped
    {
        get
        {
            var dic = Managers.Data?.CostumeDic;
            if (dic != null && Managers.Game != null && dic.TryGetValue(Managers.Game.CostumeId, out var c) && IsOwned(c)) return c;
            return List.Find(x => x.Unlock == UNLOCK_DEFAULT);
        }
    }

    public string UnlockText(CostumeData c) => c.Unlock switch
    {
        UNLOCK_LEVEL     => string.Format(Util.Localize("CostumeLevelFormat"), c.UnlockValue),
        UNLOCK_PROMOTION => string.Format(Util.Localize("CostumePromotionFormat"), c.UnlockValue),
        UNLOCK_ABYSS     => string.Format(Util.Localize("CostumeAbyssFormat"), c.UnlockValue),
        _                => "",
    };

    // 달성으로 얻은 의상 중 아직 알리지 않은 것
    bool IsUnseen(CostumeData c) => c.Unlock != UNLOCK_DEFAULT && c.Unlock != UNLOCK_CRYSTAL
                                    && !Managers.Game.Costumes.Contains(c.DataId) && IsOwned(c);

    // 레벨·시련·심연으로 새로 얻은 의상을 한 번씩 알린다 — 레벨업·시련 통과가 거치는 RefreshStats 와 심연 층 넘김이 부른다
    public void CheckNew()
    {
        var g = Managers.Game;
        if (g == null) return;
        List<string> names = null;
        foreach (var c in List)
        {
            if (!IsUnseen(c)) continue;
            g.Costumes.Add(c.DataId);
            (names ??= new List<string>()).Add(Util.Localize(c.NameKey));
        }
        if (names == null) return;
        g.SaveGame();
        Managers.UI?.ShowToast(string.Format(Util.Localize("CostumeNewFormat"), string.Join(", ", names)));
        OnChanged?.Invoke();
    }

    // 크리스탈 의상 사기 — 사면 바로 입는다. 크리스탈이 모자라면 false(부른 쪽이 알린다)
    public bool TryBuy(CostumeData c)
    {
        var g = Managers.Game;
        if (c == null || g == null || c.Unlock != UNLOCK_CRYSTAL || IsOwned(c) || g.Crystal < c.UnlockValue) return false;
        g.Crystal -= c.UnlockValue;
        g.Costumes.Add(c.DataId);
        g.CostumeId = c.DataId;
        ResetSkin();
        Managers.UI.ShowToast(string.Format(Util.Localize("CostumeBoughtFormat"), Util.Localize(c.NameKey)));
        OnChanged?.Invoke();
        return true;
    }

    public void Equip(CostumeData c)
    {
        var g = Managers.Game;
        if (c == null || g == null || !IsOwned(c) || Equipped?.DataId == c.DataId) return;
        g.CostumeId = c.DataId;
        ResetSkin();
        OnChanged?.Invoke();
    }
    #endregion

    #region 칠하기
    readonly Dictionary<Texture2D, Texture2D> _sheets = new Dictionary<Texture2D, Texture2D>();   // 원본 시트 → 입은 의상으로 칠한 사본
    readonly Dictionary<Sprite, Sprite> _sprites = new Dictionary<Sprite, Sprite>();              // 원본 → 사본
    readonly HashSet<Sprite> _made = new HashSet<Sprite>();                                       // 사본(다시 칠하지 않게)
    readonly Dictionary<int, Sprite> _previews = new Dictionary<int, Sprite>();                   // 의상별 초상화

    // HUD 초상화 원본(UI_GameScene 이 넣는다) — 의상 탭 줄 아이콘과 HUD 초상화의 바탕
    Sprite _portrait;
    public Sprite Portrait
    {
        get => _portrait;
        set
        {
            if (value == _portrait) return;
            foreach (var s in _previews.Values) DestroyCopy(s);
            _previews.Clear();
            _portrait = value;
        }
    }

    static bool IsPlain(CostumeData c) => Mathf.Approximately(c.Hue, 0f) && Mathf.Approximately(c.Sat, 1f)
                                          && Mathf.Approximately(c.Val, 1f) && string.IsNullOrEmpty(c.Tint);

    // 애니메이터가 끼운 스프라이트 → 입은 의상 색 사본(기본 의상·이미 사본이면 그대로)
    public Sprite Skin(Sprite cur)
    {
        if (cur == null || _made.Contains(cur)) return cur;
        var c = Equipped;
        if (c == null || IsPlain(c)) return cur;
        if (_sprites.TryGetValue(cur, out var s)) return s;
        var tex = cur.texture;
        if (tex == null || cur.packed) return cur;   // 아틀라스에 묶인 스프라이트는 칸 좌표가 달라 칠하지 않는다(주인공 시트는 낱장)
        if (!_sheets.TryGetValue(tex, out var sheet))
            _sheets[tex] = sheet = Paint(tex, new RectInt(0, 0, tex.width, tex.height), c);
        s = Copy(cur, sheet, cur.rect);
        _sprites[cur] = s;
        _made.Add(s);
        return s;
    }

    // 의상 탭 줄 아이콘 — 초상화를 그 의상 색으로
    public Sprite Preview(CostumeData c)
    {
        if (_portrait == null || c == null || IsPlain(c)) return _portrait;
        if (_previews.TryGetValue(c.DataId, out var s) && s != null) return s;
        var r = _portrait.rect;
        var tex = Paint(_portrait.texture, new RectInt((int)r.x, (int)r.y, (int)r.width, (int)r.height), c);
        return _previews[c.DataId] = Copy(_portrait, tex, new Rect(0f, 0f, r.width, r.height));
    }

    // 의상을 바꾸면 칠해 둔 사본을 버린다 — 몸에 끼워진 사본은 원본으로 되돌려 둔다(다음 LateUpdate 에 새 색으로 다시 칠해진다)
    void ResetSkin()
    {
        var p = Managers.Game?.Player;
        var sr = p != null ? p.GetComponent<SpriteRenderer>() : null;
        if (sr != null)
            foreach (var kv in _sprites)
                if (kv.Value == sr.sprite) { sr.sprite = kv.Key; break; }
        foreach (var s in _sprites.Values) UnityEngine.Object.Destroy(s);
        foreach (var t in _sheets.Values) UnityEngine.Object.Destroy(t);
        _sprites.Clear();
        _sheets.Clear();
        _made.Clear();
    }

    static void DestroyCopy(Sprite s)
    {
        if (s == null) return;
        UnityEngine.Object.Destroy(s.texture);
        UnityEngine.Object.Destroy(s);
    }

    // 읽기 불가 원본의 r 영역을 GPU 로 복사해 읽은 뒤 의상 색으로 칠한 새 텍스처. 같은 색은 한 번만 계산한다.
    static Texture2D Paint(Texture2D src, RectInt r, CostumeData c)
    {
        var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(r.width, r.height, TextureFormat.RGBA32, false)
        {
            name = src.name + "_Costume",
            filterMode = src.filterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
        tex.ReadPixels(new Rect(r.x, r.y, r.width, r.height), 0, 0, false);
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        Color tint = Color.white;
        bool hasTint = !string.IsNullOrEmpty(c.Tint) && ColorUtility.TryParseHtmlString("#" + c.Tint, out tint);
        var px = tex.GetPixels32();
        var done = new Dictionary<int, Color32>();
        for (int i = 0; i < px.Length; i++)
        {
            var p = px[i];
            if (p.a == 0) continue;
            int key = p.r << 16 | p.g << 8 | p.b;
            if (!done.TryGetValue(key, out var q)) done[key] = q = Recolor(p, c, hasTint, tint);
            q.a = p.a;
            px[i] = q;
        }
        tex.SetPixels32(px);
        tex.Apply(false, true);   // 칠한 뒤엔 CPU 사본을 버린다
        return tex;
    }

    static Color32 Recolor(Color32 p, CostumeData c, bool hasTint, Color tint)
    {
        Color.RGBToHSV(p, out float h, out float s, out float v);
        if (v < COSTUME_LINE_VAL) return p;
        if (s >= COSTUME_CLOTH_SAT && (v <= COSTUME_SKIN_VAL || h >= COSTUME_SKIN_HUE))
            return Color.HSVToRGB(Mathf.Repeat(h + c.Hue / 360f, 1f), Mathf.Clamp01(s * c.Sat), Mathf.Clamp01(v * c.Val));
        if (hasTint && s < COSTUME_WHITE_SAT && v >= COSTUME_WHITE_VAL)
            return (Color)p * tint;
        return p;
    }

    // 원본과 같은 칸·피벗·PPU·외곽(딱 맞는 메시)의 사본 — 발밑 바운즈를 쓰는 연출(ShowRankUpEffectOn)도 그대로다
    static Sprite Copy(Sprite src, Texture2D tex, Rect rect)
    {
        var pivot = new Vector2(src.pivot.x / src.rect.width, src.pivot.y / src.rect.height);
        var s = Sprite.Create(tex, rect, pivot, src.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        s.name = src.name + "_Costume";
        var v = src.vertices;   // 피벗 기준 유닛 → 칸 안 픽셀
        var px = new Vector2[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            var q = v[i] * src.pixelsPerUnit + src.pivot;
            px[i] = new Vector2(Mathf.Clamp(q.x, 0f, rect.width), Mathf.Clamp(q.y, 0f, rect.height));
        }
        s.OverrideGeometry(px, src.triangles);
        return s;
    }
    #endregion
}
