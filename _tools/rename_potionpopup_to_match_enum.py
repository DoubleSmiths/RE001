"""
Apply specific m_Name renames to UI_PotionPopup.prefab so that each enum member
in UI_PotionPopup.cs has exactly one matching GameObject (PascalCase, no spaces).
"""
from pathlib import Path

PREFAB = Path(r"C:\stopresent\Unity\RE001\Assets\@Resources\Prefab\UI\Popup\UI_PotionPopup.prefab")

RENAMES = {
    "Potion Box Group":                  "PotionBoxGroup",
    "HP Potion Slot Frame":              "HPPotionSlotFrame",
    "HP Potion Slot Background":         "HPPotionSlotBackground",
    "HP Potion Slot PotionImg":          "HPPotionSlotPotionImg",
    "HP Potion Cooldown Finish Anim":    "HPPotionCooldownFinishAnim",
    "Speed Potion Slot Frame":           "SpeedPotionSlotFrame",
    "Speed Potion Slot Background":      "SpeedPotionSlotBackground",
    "Speed Potion Slot PotionImg":       "SpeedPotionSlotPotionImg",
    "Speed Potion Cooldown Finish Anim": "SpeedPotionCooldownFinishAnim",
    "Image _1":                          "Image_1",
    "AutoPotion Guage Lock":             "AutoPotionGuageLock",
    "HP Potion Left Txt":                "HPPotionLeftTxt",
    "Speed Potion Left Txt":             "SpeedPotionLeftTxt",
    "Auto Btn New Txt":                  "AutoBtnNewTxt",
    "Potion Popup Background":           "PotionPopupBackground",
    "HP Potion Slot":                    "HPPotionSlot",
    "Speed Potion Slot":                 "SpeedPotionSlot",
    "Button _1":                         "Button_1",
    "Potion Layout":                     "PotionLayout",
    "HP Potion Cooldown Bar":            "HPPotionCooldownBar",
    "Speed Potion Cooldown Bar":         "SpeedPotionCooldownBar",
    "Handle Slide Area":                 "HandleSlideArea",
}


def main():
    text = PREFAB.read_bytes().decode("utf-8")
    # Sort by length desc so longer names ("HP Potion Slot Frame") replace before
    # shorter prefixes ("HP Potion Slot") to avoid partial matches.
    applied = []
    for old in sorted(RENAMES.keys(), key=len, reverse=True):
        new = RENAMES[old]
        token = f"  m_Name: {old}\r\n"
        new_token = f"  m_Name: {new}\r\n"
        count = text.count(token)
        if count == 0:
            print(f"[SKIP] '{old}' not found in prefab")
            continue
        if count > 1:
            print(f"[WARN] '{old}' appears {count} times — refusing to rename")
            continue
        text = text.replace(token, new_token)
        applied.append((old, new))

    # Also handle possible PrefabInstance overrides for these names
    for old, new in RENAMES.items():
        token = f"      value: {old}\r\n"
        new_token = f"      value: {new}\r\n"
        if token in text:
            text = text.replace(token, new_token)
            applied.append((f"(override) {old}", new))

    PREFAB.write_bytes(text.encode("utf-8"))
    print(f"\nApplied {len(applied)} renames:")
    for o, n in applied:
        print(f"  '{o}' -> '{n}'")


if __name__ == "__main__":
    main()
