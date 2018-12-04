using System.Collections;
using UnityEngine;
using System;
namespace Util {
    public class Clock {
        public float normalized;
        private readonly Func<float> getMaxTime;
        private MonoBehaviour monoBehaviour;
        public bool clocking;
        private Coroutine coroutine;
        private bool fixedUpdate;

        public Clock(Func<float> getMaxTime, MonoBehaviour monoBehaviour, bool fixedUpdate = false)
        {
            this.getMaxTime = getMaxTime;
            this.monoBehaviour = monoBehaviour;
            this.fixedUpdate = fixedUpdate;
        }
        public Clock(float maxTime, MonoBehaviour monoBehaviour, bool fixedUpdate = false)
            : this(() => maxTime, monoBehaviour, fixedUpdate)
        {
        }

        public void On() {
            if(!clocking) {
                coroutine = monoBehaviour.StartCoroutine(clockProcess());
                clocking = true;
            }
        }
        private IEnumerator clockProcess() {
            while(true) {
                normalized += Time.deltaTime / getMaxTime();
                normalized = Mathf.Min(normalized, 1);
                yield return fixedUpdate ? new WaitForFixedUpdate() : null;
            }
        }
        public void Off() {
            if(clocking) {
                clocking = false;
                monoBehaviour.StopCoroutine(coroutine);
            }
        }
        public void Reset() {
            normalized = 0;
        }
        public void SetFinished() {
            normalized = 1;
        }
        public bool Finished {
            get {
                return normalized >= 1 || getMaxTime() == 0;
            }
        }
        public float TimeCount {
            get {
                return normalized * getMaxTime();
            }
        }
        

    }
}