using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sharklib.ShineShader.Examples {
    public class PageSwitcher : MonoBehaviour {

        [SerializeField] Text imgName;
        [SerializeField] GameObject[] pages = null;
        [SerializeField] string[] names = null;

        int index = 0;

        void Start() {
            SetPage(0);
        }

        public void OffsetPage(int offset) {
            SetPage(index + offset);
        }

        void SetPage(int newIndex) {
            if (newIndex >= pages.Length)
                index = 0;
            else if (newIndex < 0)
                index = pages.Length - 1;
            else
                index = newIndex;

            imgName.text = names[index];

            for (int i = 0; i < pages.Length; i++) {
                pages[i].gameObject.SetActive(i == index);
            }
        }

    }
}