using UnityEngine;
using System.Collections;

public class TargetPositioning : MonoBehaviour
{

		private bool state1 = false;
		private bool state2 = false;
	

		private Vector3 initialSF;
		private Vector3 initialPos;
		public GameObject model;

		private float counter = 4;


		void Start ()
		{
				initialSF = this.transform.localScale;
				initialPos = this.model.transform.localPosition;

		}
	
		// Update is called once per frame
		void Update ()
		{
		if (DefaultTrackableEventHandler.TRACKING) {
			float f = -this.transform.position.y;
			float test = (2 / (this.initialSF.x * f));
			if (f != 0f && this.initialSF.x != 0f && !float.IsInfinity(test)) {
				this.model.transform.localScale = new Vector3 (counter / (this.initialSF.x * f), counter / (this.initialSF.y * f), counter / (this.initialSF.z * f));
				this.model.transform.localPosition = new Vector3((this.transform.position.x-this.initialPos.x)/(f*f) ,this.initialPos.y,(this.transform.position.z-this.initialPos.x)/(f*f));
				//Debug.Log(f);
			}
			state1 = true;
		} else if (state1) {
			state1 = false;
		}
				if (TrackableEventHandler.TRACKING) {
						/*if(!state2){
							psOn.Play();
						}*/
						if (this.transform.position.y > 100) {
								Vector3 v = new Vector3 (0, -200, 0);
								this.transform.position += v;
						}
						state2 = true;
				} else if (state2) {
						state2 = false;
				}
		}

	private float symLog(float value){
		if (value < 0) {
			return (-Mathf.Log((-value)+1,10));
		} else if (value > 0) {
			return (Mathf.Log (value+1,10));
		} else {
			return 0f;
		}
	}
	
}
