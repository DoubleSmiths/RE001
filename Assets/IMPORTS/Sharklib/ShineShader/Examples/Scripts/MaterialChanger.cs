using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sharklib.ShineShader.Examples {
    public class MaterialChanger : MonoBehaviour {

        [SerializeField] Image imgShiny;

        [SerializeField] Slider sliderFreq;
        [SerializeField] Slider sliderPause;
        [SerializeField] Slider sliderWidth;
        [SerializeField] Slider sliderFade;

        [SerializeField] Text textFreq;
        [SerializeField] Text textPause;
        [SerializeField] Text textWidth;
        [SerializeField] Text textFade;

        Material mat;

        void Start() {
            mat = new Material(imgShiny.material);
            imgShiny.material = mat;

            sliderFreq.value = mat.GetFloat("_WaveFreq");
            sliderPause.value = mat.GetFloat("_WavePause");
            sliderWidth.value = mat.GetFloat("_WaveWidth");
            sliderFade.value = mat.GetFloat("_WaveFade");
        }

        public void SetFreq(float val) {
            mat.SetFloat("_WaveFreq", val);
            textFreq.text = string.Format("Frequency: {0:0.00}", val);
        }

        public void SetPause(float val) {
            mat.SetFloat("_WavePause", val);
            textPause.text = string.Format("Pause: {0:0.00}", val);
        }

        public void SetWidth(float val) {
            mat.SetFloat("_WaveWidth", val);
            textWidth.text = string.Format("Width: {0:0.00}", val);
        }

        public void SetFade(float val) {
            mat.SetFloat("_WaveFade", val);
            textFade.text = string.Format("Fade: {0:0.00}", val);
        }

        void SetFloat(string name, float value) {
            mat.SetFloat(name, value);
        }
    }
}