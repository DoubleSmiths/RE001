using UnityEngine;

/// <summary>외부에서 받아온 애니메이션 클립에 박혀 있는 AnimationEvent의 빈 수신부.
///
/// 왜 필요한가
///   구매/외주 클립(슬라임, 이모지, 대장간 망치 등)에는 `TriggerUnityEvent` 같은
///   AnimationEvent가 남아 있다. 우리 게임은 그걸 쓰지 않는데, 유니티는 **받을 컴포넌트가
///   없으면 프레임마다 경고를 뱉는다**. 대장간에서 망치질 몇 번에 로그가 수십 줄로 도배돼
///   진짜 에러가 파묻혔다.
///
///   클립에서 이벤트를 지우는 방법도 있지만, 원본을 다시 받아오면 되살아난다.
///   받는 쪽을 하나 두는 편이 재발하지 않는다.
///
///   같은 이유로 CreatureController에도 동일한 빈 메서드가 있다(몬스터용).
///   이 컴포넌트는 그 밖의 UI/이펙트 애니메이터를 담당한다.
///
/// 붙이는 법: Tools/Fix Animation Event Warnings 가 클립을 훑어 필요한 곳에 자동으로 붙인다.
[DisallowMultipleComponent]
public class AnimEventSink : MonoBehaviour
{
    // 이름이 곧 계약이다 — 클립의 AnimationEvent가 이 이름을 호출한다. 바꾸면 경고가 돌아온다.
    public void TriggerUnityEvent() { }
}
