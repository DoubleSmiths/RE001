using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

// 제작 본문에서 재료 이름을 누르면 획득 장소를 연다. 드래그는 상위 ScrollRect가 처리한다.
[RequireComponent(typeof(TMP_Text))]
public class UI_ItemLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || eventData.dragging)
            return;

        var text = GetComponent<TMP_Text>();
        int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (index < 0)
            return;

        string link = text.textInfo.linkInfo[index].GetLinkID();
        if (!link.StartsWith(ITEM_LINK_PREFIX, System.StringComparison.Ordinal)
            || !int.TryParse(link.Substring(ITEM_LINK_PREFIX.Length), out int itemId)
            || !Managers.Data.ItemDic.ContainsKey(itemId))
            return;

        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(itemId);
    }
}
