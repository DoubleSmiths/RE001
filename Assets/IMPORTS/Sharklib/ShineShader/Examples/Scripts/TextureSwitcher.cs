using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sharklib.ShineShader.Examples {
    public class TextureSwitcher : MonoBehaviour {

        [SerializeField] Image imgRaw;
        [SerializeField] Image imgShiny;
        [SerializeField] Image imgIcon;
        [SerializeField] Text imgName;
        [SerializeField] Sprite[] sprites = null;
        [SerializeField] string[] names = null;
        [SerializeField] Sprite overrideSprite;

        int index = 0;

        void Start() {
            SetImage(0);
        }

        public void OffsetImage(int offset) {
            SetImage(index + offset);
        }

        void SetImage(int newIndex) {
            if (newIndex >= sprites.Length)
                index = 0;
            else if (newIndex < 0)
                index = sprites.Length - 1;
            else
                index = newIndex;

            imgRaw.sprite = sprites[index];
            imgShiny.sprite = sprites[index];
            imgName.text = names[index];

            imgIcon.overrideSprite = (index % 2 == 0) ? null : overrideSprite;
        }

    }
}