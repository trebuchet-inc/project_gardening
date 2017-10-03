using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewtonVR;

public class NVRHighlightComponent : MonoBehaviour {
	GameObject _highlight;
	bool _show;
	bool _isShowing;

	void Start () {
		GetComponent<NVRInteractable>().OnHovering.AddListener(show);
		FindHighlight();
	}

	void Update(){
		if (_highlight != null) {
			if (!_isShowing && _show) {
				_highlight.gameObject.SetActive (true);
				_isShowing = true;
			}
			if (_isShowing && !_show) {
				_highlight.gameObject.SetActive (false);
				_isShowing = false;
			}
			_show = false;
		}
	}


	public void FindHighlight(){
		Transform[] childs = GetComponentsInChildren<Transform>(true);
		for(int i = 0; i < childs.Length; i++){
			if (childs[i].name == "Highlight") {
				_highlight = childs[i].gameObject;
				_highlight.SetActive(false);
				return;
			}
		}
	}

	public void show () {
		_show = true;
	}
}
