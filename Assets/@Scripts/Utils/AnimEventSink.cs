using System;
using UnityEngine;

/// <summary>애니메이션 클립에 박힌 AnimationEvent(`TriggerUnityEvent`)를 받아 흘려보낸다.
///
/// 두 가지 일을 한다.
///
/// 1) **경고 제거** — 받을 컴포넌트가 없으면 유니티가 프레임마다 "has no receiver"를 뱉는다.
///    외주 클립(슬라임, 이모지 등)에는 우리가 안 쓰는 이벤트가 남아 있어 로그가 도배됐다.
///    구독자가 없으면 조용히 삼킨다.
///
/// 2) **연출 신호 전달** — 반대로 어떤 클립은 이벤트가 곧 연출 대본이다.
///    대장간 강화 클립(Forge Smith/Success.anim)이 그렇다:
///        t=0.08 SmashSound / t=0.33 Shake / t=1.08 SmashSound / … / t=3.83 Success
///    원본(비주얼 스크립팅)은 이 문자열마다 소리와 흔들림을 걸어 뒀다. C# 포트에서는
///    아무도 받지 않아 **망치질 소리가 한 번뿐**이었다. 구독해서 같은 대본을 재현한다.
///
/// 이벤트 이름은 클립이 정한다(stringParameter). 바꾸면 연출이 끊긴다.
[DisallowMultipleComponent]
public class AnimEventSink : MonoBehaviour
{
    /// <summary>클립이 보낸 신호. 인자는 stringParameter(예: "SmashSound").</summary>
    public event Action<string> OnAnimEvent;

    // 이름이 곧 계약이다 — 클립의 AnimationEvent가 이 이름을 호출한다.
    public void TriggerUnityEvent(string id) => OnAnimEvent?.Invoke(id);
}
